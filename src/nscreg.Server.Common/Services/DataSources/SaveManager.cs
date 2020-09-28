using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using nscreg.Data;
using nscreg.Data.Constants;
using nscreg.Data.Entities;
using nscreg.Server.Common.Models.StatUnits;
using nscreg.Server.Common.Models.StatUnits.Create;
using nscreg.Server.Common.Models.StatUnits.Edit;
using nscreg.Server.Common.Services.StatUnit;
using nscreg.Utilities.Extensions;

namespace nscreg.Server.Common.Services.DataSources
{
    public class SaveManager
    {
        private readonly Dictionary<StatUnitTypes, Func<StatisticalUnit, string, Task>> _createByType;

        private readonly QueueService _queueSvc;

        private readonly Dictionary<StatUnitTypes, Func<StatisticalUnit, string, Task>> _updateByType;
        private readonly NSCRegDbContext _ctx;

        private readonly UserService _usrService;

        public SaveManager(NSCRegDbContext context, QueueService queueService, CreateService createSvc,
            EditService editSvc)
        {
            _ctx = context;
            _queueSvc = queueService;
            _usrService = new UserService(context);
            
            _createByType = new Dictionary<StatUnitTypes, Func<StatisticalUnit, string, Task>>
            {
                [StatUnitTypes.LegalUnit] = (unit, userId) =>
                    createSvc.CreateLegalUnit(MappedUnitM(unit, StatUnitTypes.LegalUnit, "LegalUnitCreateM", userId), userId),
                [StatUnitTypes.LocalUnit] = (unit, userId) =>
                    createSvc.CreateLocalUnit(MappedUnitM(unit, StatUnitTypes.LocalUnit, "LocalUnitCreateM", userId), userId),
                [StatUnitTypes.EnterpriseUnit] = (unit, userId) =>
                    createSvc.CreateEnterpriseUnit(MappedUnitM(unit, StatUnitTypes.EnterpriseUnit, "EnterpriseUnitCreateM", userId), userId)
            };
            _updateByType = new Dictionary<StatUnitTypes, Func<StatisticalUnit, string, Task>>
            {
                [StatUnitTypes.LegalUnit] = (unit, userId) =>
                    editSvc.EditLegalUnit(MappedUnitM(unit, StatUnitTypes.LegalUnit, "LegalUnitEditM", userId), userId),
                [StatUnitTypes.LocalUnit] = (unit, userId) =>
                    editSvc.EditLocalUnit(MappedUnitM(unit, StatUnitTypes.LocalUnit, "LocalUnitEditM", userId), userId),
                [StatUnitTypes.EnterpriseUnit] = (unit, userId) =>
                    editSvc.EditEnterpriseUnit(MappedUnitM(unit, StatUnitTypes.EnterpriseUnit, "EnterpriseUnitEditM", userId), userId)
            };
        }

        private async Task<(string, bool)> SaveStatUnitsUpload(StatisticalUnit parsedUnit, DataSource dataSource,
            string userId)
        {
            // TODO: вероятно, лишний запрос. Такое делается до Analyze (Populate возвращает isNew)
            var unitExists = await _queueSvc.CheckIfUnitExists(dataSource.StatUnitType, parsedUnit.StatId);

            if (dataSource.Priority != DataSourcePriority.Trusted &&
                (dataSource.Priority != DataSourcePriority.Ok || unitExists))
                return (null, false);

            var saveAction =
                unitExists && ( dataSource.AllowedOperations == DataSourceAllowedOperation.Alter || dataSource.AllowedOperations == DataSourceAllowedOperation.CreateAndAlter) ? _updateByType[dataSource.StatUnitType] : _createByType[dataSource.StatUnitType];

            try
            {
                await saveAction(parsedUnit, userId);
            }
            catch (Exception ex)
            {
                return (GetFullExceptionMessage(ex), false);
            }
            return (null, true);
        }

        private string GetFullExceptionMessage(Exception ex)
        {
            return ex.Message + (ex.InnerException != null ? Environment.NewLine + GetFullExceptionMessage(ex.InnerException) : "");
        }


        public async Task<(string, bool)> SaveUnit(StatisticalUnit parsedUnit, DataSource dataSource, string userId)
        {
            return await SaveStatUnitsUpload(parsedUnit, dataSource, userId);
        }

        private dynamic MappedUnitM(StatisticalUnit unit, StatUnitTypes type, string mapperType, string userId)
        {
            var dataAccess = _usrService.GetDataAccessAttributes(userId, type);
            var mappedActivities = new List<ActivityM>();
            var mappedPersons = new List<PersonM>();
            var mappedForeignParticipationCountriesUnits = new List<int>();
            var mappedUnit = new StatUnitModelBase();

            if (type == StatUnitTypes.LocalUnit && mapperType == "LocalUnitCreateM")
                mappedUnit = Mapper.Map<LocalUnitCreateM>(unit);
            else if (type == StatUnitTypes.LegalUnit && mapperType == "LegalUnitCreateM")
                mappedUnit = Mapper.Map<LegalUnitCreateM>(unit);
            else if (type == StatUnitTypes.EnterpriseUnit && mapperType == "EnterpriseUnitCreateM")
                mappedUnit = Mapper.Map<EnterpriseUnitCreateM>(unit);
            else if (type == StatUnitTypes.LocalUnit && mapperType == "LocalUnitEditM")
                mappedUnit = Mapper.Map<LocalUnitEditM>(unit);
            else if (type == StatUnitTypes.LegalUnit && mapperType == "LegalUnitEditM")
                mappedUnit = Mapper.Map<LegalUnitEditM>(unit);
            else if (type == StatUnitTypes.EnterpriseUnit && mapperType == "EnterpriseUnitEditM")
                mappedUnit = Mapper.Map<EnterpriseUnitEditM>(unit);


            mappedUnit.DataAccess = dataAccess.Result;
            mappedUnit.Address = Mapper.Map<AddressM>(unit.Address);
            mappedUnit.ActualAddress = Mapper.Map<AddressM>(unit.ActualAddress);
            mappedUnit.PostalAddress = Mapper.Map<AddressM>(unit.PostalAddress);

            unit.Activities.ForEach(activity => mappedActivities.Add(Mapper.Map<ActivityM>(activity)));
            foreach (var personStatisticalUnit in unit.PersonsUnits)
            {
                var person = Mapper.Map<PersonM>(personStatisticalUnit.Person);
                if (personStatisticalUnit.PersonTypeId != null)
                {
                    person.Role = (int) personStatisticalUnit.PersonTypeId;
                }
                mappedPersons.Add(person);
            }
            unit.ForeignParticipationCountriesUnits.ForEach(fpcu => mappedForeignParticipationCountriesUnits.Add(fpcu.Id));
            mappedUnit.ForeignParticipationCountriesUnits = mappedForeignParticipationCountriesUnits;
            mappedUnit.ForeignParticipationId = unit.ForeignParticipation?.Id;
            mappedUnit.Activities = mappedActivities;
            mappedUnit.Persons = mappedPersons;

            return mappedUnit;
        }

    }
}
