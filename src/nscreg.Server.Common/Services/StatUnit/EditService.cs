using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using nscreg.Data;
using nscreg.Data.Core;
using nscreg.Data.Entities;
using nscreg.Resources.Languages;
using nscreg.Server.Common.Helpers;
using nscreg.Server.Common.Models.StatUnits;
using nscreg.Server.Common.Models.StatUnits.Edit;
using nscreg.Server.Common.Services.Contracts;
using nscreg.Server.Common.Validators.Extentions;
using nscreg.Utilities;
using nscreg.Utilities.Configuration;
using nscreg.Utilities.Configuration.DBMandatoryFields;
using nscreg.Utilities.Configuration.StatUnitAnalysis;
using nscreg.Utilities.Enums;
using nscreg.Utilities.Extensions;
using Activity = nscreg.Data.Entities.Activity;
using EnterpriseGroup = nscreg.Data.Entities.EnterpriseGroup;
using LegalUnit = nscreg.Data.Entities.LegalUnit;
using LocalUnit = nscreg.Data.Entities.LocalUnit;
using Person = nscreg.Data.Entities.Person;

namespace nscreg.Server.Common.Services.StatUnit
{
    public class EditService
    {
        private readonly NSCRegDbContext _dbContext;
        private readonly StatUnitAnalysisRules _statUnitAnalysisRules;
        private readonly DbMandatoryFields _mandatoryFields;
        private readonly UserService _userService;
        private readonly Common _commonSvc;
        private readonly ElasticService _elasticService;
        private readonly ValidationSettings _validationSettings;
        private readonly DataAccessService _dataAccessService;
        private readonly DeleteService _deleteService;
        private readonly int? _liquidateStatusId;
        private readonly int? _deletedStatusId;
        private readonly List<ElasticStatUnit> _editArrayStatisticalUnits;
        private readonly List<ElasticStatUnit> _addArrayStatisticalUnits;

        public EditService(NSCRegDbContext dbContext, StatUnitAnalysisRules statUnitAnalysisRules,
            DbMandatoryFields mandatoryFields, ValidationSettings validationSettings)
        {
            _dbContext = dbContext;
            _statUnitAnalysisRules = statUnitAnalysisRules;
            _mandatoryFields = mandatoryFields;
            _userService = new UserService(dbContext);
            _commonSvc = new Common(dbContext);
            _elasticService = new ElasticService(dbContext);
            _validationSettings = validationSettings;
            _dataAccessService = new DataAccessService(dbContext);
            _deleteService = new DeleteService(dbContext);
            _liquidateStatusId = _dbContext.Statuses.FirstOrDefault(x => x.Code == "7")?.Id;
            _deletedStatusId = _dbContext.Statuses.FirstOrDefault(x => x.Code == "8")?.Id;
            _editArrayStatisticalUnits = new List<ElasticStatUnit>();
            _addArrayStatisticalUnits = new List<ElasticStatUnit>();
        }

        /// <summary>
        /// Метод редактирования правовой единицы
        /// </summary>
        /// <param name="data">Данные</param>
        /// <param name="userId">Id пользователя</param>
        /// <returns></returns>
        public async Task<Dictionary<string, string[]>> EditLegalUnit(LegalUnitEditM data, string userId)
            => await EditUnitContext<LegalUnit, LegalUnitEditM>(
                data,
                m => m.RegId ?? 0,
                userId, unit =>
                {
                    if (_deletedStatusId != null && unit.UnitStatusId == _deletedStatusId)
                    {
                        _deleteService.CheckBeforeDelete(unit, true);
                    }

                    if (Common.HasAccess<LegalUnit>(data.DataAccess, v => v.LocalUnits))
                    {
                        var localUnits = _dbContext.LocalUnits.Where(x => data.LocalUnits.Contains(x.RegId) && x.UnitStatusId != _liquidateStatusId);
                        
                        unit.LocalUnits.Clear();
                        unit.HistoryLocalUnitIds = null;

                        if (_liquidateStatusId != null && unit.UnitStatusId == _liquidateStatusId)
                        {
                            var enterpriseUnit = _dbContext.EnterpriseUnits.Include(x => x.LegalUnits).FirstOrDefault(x => unit.EnterpriseUnitRegId == x.RegId);
                            var legalUnits = enterpriseUnit.LegalUnits.Where(x => !x.IsDeleted && x.UnitStatusId != _liquidateStatusId).ToList();
                            if (enterpriseUnit != null && legalUnits.Count == 0)
                            {
                                enterpriseUnit.UnitStatusId = unit.UnitStatusId;
                                enterpriseUnit.LiqReason = unit.LiqReason;
                                enterpriseUnit.LiqDate = unit.LiqDate;
                                _editArrayStatisticalUnits.Add(Mapper.Map<IStatisticalUnit, ElasticStatUnit>(enterpriseUnit));
                            }
                        }
                        
                        if (data.LocalUnits == null) return Task.CompletedTask;
                        foreach (var localUnit in localUnits)
                        {
                            if (_liquidateStatusId != null && unit.UnitStatusId == _liquidateStatusId)
                            {
                                localUnit.UnitStatusId = unit.UnitStatusId;
                                localUnit.LiqReason = unit.LiqReason;
                                localUnit.LiqDate = unit.LiqDate;
                            }
                            unit.LocalUnits.Add(localUnit);
                            _addArrayStatisticalUnits.Add(Mapper.Map<IStatisticalUnit, ElasticStatUnit>(localUnit));
                        }

                        if (data.LocalUnits != null)
                            unit.HistoryLocalUnitIds = string.Join(",", data.LocalUnits);
                    }
                    return Task.CompletedTask;
                });

        /// <summary>
        /// Метод редактирования местной единицы
        /// </summary>
        /// <param name="data">Данные</param>
        /// <param name="userId">Id пользователя</param>
        /// <returns></returns>
        public async Task<Dictionary<string, string[]>> EditLocalUnit(LocalUnitEditM data, string userId)
            => await EditUnitContext<LocalUnit, LocalUnitEditM>(
                data,
                v => v.RegId ?? 0,
                userId,
                unit =>
                {
                    if (_deletedStatusId != null && unit.UnitStatusId == _deletedStatusId)
                    {
                        _deleteService.CheckBeforeDelete(unit, true);
                    }

                    if (_liquidateStatusId != null && unit.UnitStatusId == _liquidateStatusId)
                    {
                        var legalUnit = _dbContext.LegalUnits.Include(x => x.LocalUnits).FirstOrDefault(x => unit.LegalUnitId == x.RegId && !x.IsDeleted);
                        if (legalUnit != null && legalUnit.LocalUnits.Where(x => !x.IsDeleted && x.UnitStatusId != _liquidateStatusId.Value).ToList().Count == 0)
                        {
                            throw new BadRequestException(nameof(Resource.LiquidateLegalUnit));
                        }
                    } 
                    return Task.CompletedTask;
                });

        /// <summary>
        /// Метод редактирования предприятия
        /// </summary>
        /// <param name="data">Данные</param>
        /// <param name="userId">Id пользователя</param>
        /// <returns></returns>
        public async Task<Dictionary<string, string[]>> EditEnterpriseUnit(EnterpriseUnitEditM data, string userId)
            => await EditUnitContext<EnterpriseUnit, EnterpriseUnitEditM>(
                data,
                m => m.RegId ?? 0,
                userId,
                unit =>
                {
                    if (_deletedStatusId != null && unit.UnitStatusId == _deletedStatusId)
                    {
                        _deleteService.CheckBeforeDelete(unit, true);
                    }

                    if (_liquidateStatusId != null && unit.UnitStatusId == _liquidateStatusId)
                    {
                        throw new BadRequestException(nameof(Resource.LiquidateEntrUnit));
                    }
                    if (Common.HasAccess<EnterpriseUnit>(data.DataAccess, v => v.LegalUnits))
                    {
                        var legalUnits = _dbContext.LegalUnits.Where(x => data.LegalUnits.Contains(x.RegId));
                        unit.LegalUnits.Clear();
                        unit.HistoryLegalUnitIds = null;
                        foreach (var legalUnit in legalUnits)
                        {
                            unit.LegalUnits.Add(legalUnit);
                            _addArrayStatisticalUnits.Add(Mapper.Map<IStatisticalUnit, ElasticStatUnit>(legalUnit));
                        }
                        if (data.LegalUnits != null)
                            unit.HistoryLegalUnitIds = string.Join(",", data.LegalUnits);
                    }
                    return Task.CompletedTask;
                });

        /// <summary>
        /// Метод редактирования группы предприятий
        /// </summary>
        /// <param name="data">Данные</param>
        /// <param name="userId">Id пользователя</param>
        /// <returns></returns>
        public async Task<Dictionary<string, string[]>> EditEnterpriseGroup(EnterpriseGroupEditM data, string userId)
            => await EditContext<EnterpriseGroup, EnterpriseGroupEditM>(
                data,
                m => m.RegId ?? 0,
                userId,
                (unit, oldUnit) =>
                {
                    if (_deletedStatusId != null && unit.UnitStatusId == _deletedStatusId)
                    {
                        _deleteService.CheckBeforeDelete(unit, true);
                    }

                    if (Common.HasAccess<EnterpriseGroup>(data.DataAccess, v => v.EnterpriseUnits))
                    {
                        var enterprises = _dbContext.EnterpriseUnits.Where(x => data.EnterpriseUnits.Contains(x.RegId));
                        unit.EnterpriseUnits.Clear();
                        unit.HistoryEnterpriseUnitIds = null;
                        foreach (var enterprise in enterprises)
                        {
                            unit.EnterpriseUnits.Add(enterprise);
                            _addArrayStatisticalUnits.Add(Mapper.Map<IStatisticalUnit, ElasticStatUnit>(enterprise));
                        }

                        if (data.EnterpriseUnits != null)
                            unit.HistoryEnterpriseUnitIds = string.Join(",", data.EnterpriseUnits);
                    }

                    return Task.CompletedTask;
                });

        /// <summary>
        /// Метод редактирования контекста стат. единцы
        /// </summary>
        /// <param name="data">Данные</param>
        /// <param name="idSelector">Id Селектора</param>
        /// <param name="userId">Id пользователя</param>
        /// <param name="work">В работе</param>
        /// <returns></returns>
        private async Task<Dictionary<string, string[]>> EditUnitContext<TUnit, TModel>(
            TModel data,
            Func<TModel, int> idSelector,
            string userId,
            Func<TUnit, Task> work)
            where TModel : StatUnitModelBase
            where TUnit : StatisticalUnit, new()
            => await EditContext<TUnit, TModel>(
                data,
                idSelector,
                userId,
                async (unit, oldUnit) =>
                {
                    //Merge activities
                    if (Common.HasAccess<TUnit>(data.DataAccess, v => v.Activities))
                    {
                        var activities = new List<ActivityStatisticalUnit>();
                        var srcActivities = unit.ActivitiesUnits.ToDictionary(v => v.ActivityId);
                        var activitiesList = data.Activities ?? new List<ActivityM>();

                        foreach (var model in activitiesList)
                        {
                            if (model.Id.HasValue && srcActivities.TryGetValue(model.Id.Value,
                                    out ActivityStatisticalUnit activityAndUnit))
                            {
                                var currentActivity = activityAndUnit.Activity;
                                if (model.ActivityCategoryId == currentActivity.ActivityCategoryId &&
                                    ObjectComparer.SequentialEquals(model, currentActivity))
                                {
                                    activities.Add(activityAndUnit);
                                    continue;
                                }
                            }
                            var newActivity = new Activity();
                            Mapper.Map(model, newActivity);
                            newActivity.UpdatedBy = userId;
                            newActivity.ActivityCategoryId = model.ActivityCategoryId;
                            activities.Add(new ActivityStatisticalUnit() {Activity = newActivity});
                        }
                        var activitiesUnits = unit.ActivitiesUnits;
                        activitiesUnits.Clear();
                        unit.ActivitiesUnits.AddRange(activities);
                    }


                    var srcCountries = unit.ForeignParticipationCountriesUnits.ToDictionary(v => v.CountryId);
                    var countriesList = data.ForeignParticipationCountriesUnits ?? new List<int>();
                    var countryBindingsToAdd = countriesList.Where(id => !srcCountries.ContainsKey(id)).ToList();
                    foreach (var id in countryBindingsToAdd)
                        unit.ForeignParticipationCountriesUnits.Add(
                            new CountryStatisticalUnit {CountryId = id});

                    var countryBindingsToRemove = srcCountries
                        .Where(b => !countriesList.Contains(b.Key)).Select(x => x.Value).ToList();

                    foreach (var binding in countryBindingsToRemove)
                        unit.ForeignParticipationCountriesUnits.Remove(binding);

                    var persons = new List<PersonStatisticalUnit>();
                    var srcPersons = unit.PersonsUnits.ToDictionary(v => v.PersonId);
                    var personsList = data.Persons ?? new List<PersonM>();

                    foreach (var model in personsList)
                    {
                        if (model.Id.HasValue && model.Id > 0)
                        {
                            if (srcPersons.TryGetValue(model.Id.Value, out PersonStatisticalUnit personStatisticalUnit))
                            {
                                var currentPerson = personStatisticalUnit.Person;
                                if (model.Id == currentPerson.Id)
                                {
                                    currentPerson.UpdateProperties(model);
                                    persons.Add(personStatisticalUnit);
                                    continue;
                                }
                            } else
                            {
                                persons.Add(new PersonStatisticalUnit { PersonId = (int)model.Id, PersonTypeId = model.Role });
                                continue;
                            }
                        }
                        var newPerson = Mapper.Map<PersonM, Person>(model);
                        persons.Add(new PersonStatisticalUnit { Person = newPerson, PersonTypeId = model.Role });
                    }
                    var statUnitsList = data.PersonStatUnits ?? new List<PersonStatUnitModel>();

                    foreach (var unitM in statUnitsList)
                    {
                        if (unitM.StatRegId.HasValue )
                        {
                            var personStatisticalUnit = unit.PersonsUnits.First(x => x.UnitId == unitM.StatRegId.Value);
                            var currentUnit = personStatisticalUnit.Unit;
                            if (unitM.StatRegId == currentUnit.RegId)
                            {
                                currentUnit.UpdateProperties(unitM);
                                persons.Add(personStatisticalUnit);
                                continue;
                            }
                        }
                        persons.Add(new PersonStatisticalUnit
                        {
                            UnitId = unit.RegId,
                            EnterpriseGroupId = null,
                            PersonId = null,
                            PersonTypeId = unitM.RoleId
                        });
                    }

                    var groupUnits = unit.PersonsUnits.Where(su => su.EnterpriseGroupId != null)
                        .ToDictionary(su => su.EnterpriseGroupId);

                    foreach (var unitM in statUnitsList)
                    {
                        if (unitM.GroupRegId.HasValue &&
                            groupUnits.TryGetValue(unitM.GroupRegId, out var personStatisticalUnit))
                        {
                            var currentUnit = personStatisticalUnit.Unit;
                            if (unitM.GroupRegId == currentUnit.RegId)
                            {
                                currentUnit.UpdateProperties(unitM);
                                persons.Add(personStatisticalUnit);
                                continue;
                            }
                        }
                        persons.Add(new PersonStatisticalUnit
                        {
                            UnitId = unit.RegId,
                            EnterpriseGroupId = unitM.GroupRegId,
                            PersonId = null
                        });
                    }

                    var statisticalUnits = unit.PersonsUnits;
                    statisticalUnits.Clear();
                    unit.PersonsUnits.AddRange(persons);

                    if (data.LiqDate != null || !string.IsNullOrEmpty(data.LiqReason) || (_liquidateStatusId != null && data.UnitStatusId == _liquidateStatusId))
                    {
                        unit.UnitStatusId = _liquidateStatusId;
                        unit.LiqDate = unit.LiqDate ?? DateTime.Now;
                    }

                    if ((oldUnit.LiqDate != null && data.LiqDate == null)  || (!string.IsNullOrEmpty(oldUnit.LiqReason) &&  string.IsNullOrEmpty(data.LiqReason)))
                    {
                        unit.LiqDate = oldUnit.LiqDate != null && data.LiqDate == null ? oldUnit.LiqDate : data.LiqDate;
                        unit.LiqReason = !string.IsNullOrEmpty(oldUnit.LiqReason) && string.IsNullOrEmpty(data.LiqReason) ? oldUnit.LiqReason : data.LiqReason;
                    }

                    if (work != null)
                    {
                        await work(unit);
                    }
                    
                });

        /// <summary>
        /// Метод редактирования контекста
        /// </summary>
        /// <param name="data">Данные</param>
        /// <param name="idSelector">Id Селектора</param>
        /// <param name="userId">Id пользователя</param>
        /// <param name="work">В работе</param>
        /// <returns></returns>
        private async Task<Dictionary<string, string[]>> EditContext<TUnit, TModel>(
            TModel data,
            Func<TModel, int> idSelector,
            string userId,
            Func<TUnit, TUnit, Task> work)
            where TModel : IStatUnitM
            where TUnit : class, IStatisticalUnit, new()
        {


            var unit = (TUnit) await ValidateChanges<TUnit>(idSelector(data));
            if (_dataAccessService.CheckWritePermissions(userId, unit.UnitType))
            {
                return new Dictionary<string, string[]> { { nameof(UserAccess.UnauthorizedAccess), new []{ nameof(Resource.Error403) } } };
            }

            await _commonSvc.InitializeDataAccessAttributes(_userService, data, userId, unit.UnitType);

            var unitsHistoryHolder = new UnitsHistoryHolder(unit);

            var hUnit = new TUnit();
            Mapper.Map(unit, hUnit);
            Mapper.Map(data, unit);
            
            var deleteEnterprise = false;
            var isDeleted = false;
            var existingLeuEntRegId = (int?) 0;
            if (unit is LegalUnit)
            {
                var legalUnit = unit as LegalUnit;
                existingLeuEntRegId = _dbContext.LegalUnits.Where(leu => leu.RegId == legalUnit.RegId)
                    .Select(leu => leu.EnterpriseUnitRegId).FirstOrDefault();
                if (existingLeuEntRegId != legalUnit.EnterpriseUnitRegId &&
                    !_dbContext.LegalUnits.Any(leu => leu.EnterpriseUnitRegId == existingLeuEntRegId))
                    deleteEnterprise = true;
            }

            if (_liquidateStatusId != null && hUnit.UnitStatusId == _liquidateStatusId && unit.UnitStatusId != hUnit.UnitStatusId)
            {
                throw new BadRequestException(nameof(Resource.UnitHasLiquidated));
            }
            
            //External Mappings
            if (work != null)
            {
                await work(unit, hUnit);
            }

            _commonSvc.AddAddresses<TUnit>(unit, data);
            if (IsNoChanges(unit, hUnit)) return null;

            unit.UserId = userId;
            if (_deletedStatusId != null && unit.UnitStatusId == _deletedStatusId)
            {
                unit.IsDeleted = true;
                unit.ChangeReason = ChangeReasons.Delete;
                unit.EditComment = null;
                isDeleted = true;

            } else
            {
                unit.ChangeReason = data.ChangeReason;
                unit.EditComment = data.EditComment;
            }

            IStatUnitAnalyzeService analysisService =
                new AnalyzeService(_dbContext, _statUnitAnalysisRules, _mandatoryFields, _validationSettings);
            var analyzeResult = analysisService.AnalyzeStatUnit(unit);
            if (analyzeResult.Messages.Any()) return analyzeResult.Messages;

            var mappedHistoryUnit = _commonSvc.MapUnitToHistoryUnit(hUnit);
            _commonSvc.AddHistoryUnitByType(Common.TrackHistory(unit, mappedHistoryUnit));

            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var changeDateTime = DateTime.Now;
                    _commonSvc.AddHistoryUnitByType(Common.TrackHistory(unit, mappedHistoryUnit, changeDateTime));
                   await _dbContext.SaveChangesAsync();

                    _commonSvc.TrackRelatedUnitsHistory(unit, hUnit, userId, data.ChangeReason, data.EditComment,
                        changeDateTime, unitsHistoryHolder);
                    await _dbContext.SaveChangesAsync();

                    if (deleteEnterprise)
                    {
                        _dbContext.EnterpriseUnits.Remove(_dbContext.EnterpriseUnits.First(eu => eu.RegId == existingLeuEntRegId));
                        await _dbContext.SaveChangesAsync();
                    }

                    transaction.Commit();

                    if (_addArrayStatisticalUnits.Any())
                        foreach (var addArrayStatisticalUnit in _addArrayStatisticalUnits)
                        {
                            await _elasticService.AddDocument(addArrayStatisticalUnit);   
                        }
                    if (_editArrayStatisticalUnits.Any())
                        foreach (var editArrayStatisticalUnit in _editArrayStatisticalUnits)
                        {
                            await _elasticService.EditDocument(editArrayStatisticalUnit);
                        }

                    await _elasticService.EditDocument(Mapper.Map<IStatisticalUnit, ElasticStatUnit>(unit));

                    if (isDeleted)
                    {
                        _deleteService.StatUnitPostDeleteActions(unit, true, userId);
                    }
                }
                catch (Exception e)
                {
                    //TODO: Processing Validation Errors
                    throw new BadRequestException(nameof(Resource.SaveError), e);
                }
            }

            return null;
        }

        /// <summary>
        /// Метод валидации изменений данных
        /// </summary>
        /// <param name="regid">Регистрационный Id</param>
        /// <returns></returns>
        private async Task<IStatisticalUnit> ValidateChanges<T>(int regid)
            where T : class, IStatisticalUnit
        {
            var unit = await _commonSvc.GetStatisticalUnitByIdAndType(
                regid,
                StatisticalUnitsTypeHelper.GetStatUnitMappingType(typeof(T)),
                false);

            return unit;
        }

        /// <summary>
        /// Метод проверки на неизменность данных
        /// </summary>
        /// <param name="unit">Стат. единицы</param>
        /// <param name="hUnit">История стат. единицы</param>
        /// <returns></returns>
        private static bool IsNoChanges(IStatisticalUnit unit, IStatisticalUnit hUnit)
        {
            var unitType = unit.GetType();
            var propertyInfo = unitType.GetProperties();
            foreach (var property in propertyInfo)
            {
                var unitProperty = unitType.GetProperty(property.Name).GetValue(unit, null);
                var hUnitProperty = unitType.GetProperty(property.Name).GetValue(hUnit, null);
                if (!Equals(unitProperty, hUnitProperty)) return false;
            }
            if (!(unit is StatisticalUnit statUnit)) return true;
            var hstatUnit = (StatisticalUnit) hUnit;
            return hstatUnit.ActivitiesUnits.CompareWith(statUnit.ActivitiesUnits, v => v.ActivityId)
                   && hstatUnit.PersonsUnits.CompareWith(statUnit.PersonsUnits, p => p.PersonId);
        }

    }
}
