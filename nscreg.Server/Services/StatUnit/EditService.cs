﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using nscreg.Data;
using nscreg.Data.Entities;
using nscreg.Data.Helpers;
using nscreg.Resources.Languages;
using nscreg.Server.Core;
using nscreg.Server.Models.Lookup;
using nscreg.Server.Models.StatUnits;
using nscreg.Server.Models.StatUnits.Edit;
using nscreg.Utilities;
using nscreg.Utilities.Extensions;
using static nscreg.Server.Services.StatUnit.Common;

namespace nscreg.Server.Services.StatUnit
{
    public class EditService
    {
        private readonly NSCRegDbContext _dbContext;
        private readonly UserService _userService;

        public EditService(NSCRegDbContext dbContext)
        {
            _dbContext = dbContext;
            _userService = new UserService(dbContext);
        }

        public async Task EditLegalUnit(LegalUnitEditM data, string userId)
            => await EditUnitContext<LegalUnit, LegalUnitEditM>(
                data,
                m => m.RegId.Value,
                userId,
                unit =>
                {
                    if (HasAccess<LegalUnit>(data.DataAccess, v => v.LocalUnits))
                    {
                        var localUnits = _dbContext.LocalUnits.Where(x => data.LocalUnits.Contains(x.RegId));
                        unit.LocalUnits.Clear();
                        foreach (var localUnit in localUnits)
                        {
                            unit.LocalUnits.Add(localUnit);
                        }
                    }
                    return Task.CompletedTask;
                });

        public async Task EditLocalUnit(LocalUnitEditM data, string userId)
            => await EditUnitContext<LocalUnit, LocalUnitEditM>(
                data,
                v => v.RegId.Value,
                userId,
                null);

        public async Task EditEnterpiseUnit(EnterpriseUnitEditM data, string userId)
            => await EditUnitContext<EnterpriseUnit, EnterpriseUnitEditM>(
                data,
                m => m.RegId.Value,
                userId,
                unit =>
                {
                    if (HasAccess<EnterpriseUnit>(data.DataAccess, v => v.LocalUnits))
                    {
                        var localUnits = _dbContext.LocalUnits.Where(x => data.LocalUnits.Contains(x.RegId));
                        unit.LocalUnits.Clear();
                        foreach (var localUnit in localUnits)
                        {
                            unit.LocalUnits.Add(localUnit);
                        }
                    }
                    if (HasAccess<EnterpriseUnit>(data.DataAccess, v => v.LegalUnits))
                    {
                        var legalUnits = _dbContext.LegalUnits.Where(x => data.LegalUnits.Contains(x.RegId));
                        unit.LegalUnits.Clear();
                        foreach (var legalUnit in legalUnits)
                        {
                            unit.LegalUnits.Add(legalUnit);
                        }
                    }
                    return Task.CompletedTask;
                });

        public async Task EditEnterpiseGroup(EnterpriseGroupEditM data, string userId)
            => await EditContext<EnterpriseGroup, EnterpriseGroupEditM>(
                data,
                m => m.RegId.Value,
                userId,
                unit =>
                {
                    if (HasAccess<EnterpriseGroup>(data.DataAccess, v => v.EnterpriseUnits))
                    {
                        var enterprises = _dbContext.EnterpriseUnits.Where(x => data.EnterpriseUnits.Contains(x.RegId));
                        unit.EnterpriseUnits.Clear();
                        foreach (var enterprise in enterprises)
                        {
                            unit.EnterpriseUnits.Add(enterprise);
                        }
                    }
                    if (HasAccess<EnterpriseGroup>(data.DataAccess, v => v.LegalUnits))
                    {
                        unit.LegalUnits.Clear();
                        var legalUnits = _dbContext.LegalUnits.Where(x => data.LegalUnits.Contains(x.RegId)).ToList();
                        foreach (var legalUnit in legalUnits)
                        {
                            unit.LegalUnits.Add(legalUnit);
                        }
                    }
                    return Task.CompletedTask;
                });

        private async Task EditUnitContext<TUnit, TModel>(
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
                async unit =>
                {
                    //Merge activities
                    if (HasAccess<TUnit>(data.DataAccess, v => v.Activities))
                    {
                        var activities = new List<ActivityStatisticalUnit>();
                        var srcActivities = unit.ActivitiesUnits.ToDictionary(v => v.ActivityId);
                        var activitiesList = data.Activities ?? new List<ActivityM>();

                        //Get Ids for codes
                        var activityService = new CodeLookupService<ActivityCategory>(_dbContext);
                        var codesList = activitiesList.Select(v => v.ActivityRevxCategory.Code).ToList();

                        var codesLookup = new CodeLookupProvider<CodeLookupVm>(
                            nameof(Resource.ActivityCategoryLookup),
                            await activityService.List(false, v => codesList.Contains(v.Code))
                        );

                        foreach (var model in activitiesList)
                        {
                            ActivityStatisticalUnit activityAndUnit;

                            if (model.Id.HasValue && srcActivities.TryGetValue(model.Id.Value, out activityAndUnit))
                            {
                                var currentActivity = activityAndUnit.Activity;
                                if (model.ActivityRevxCategory.Id == currentActivity.ActivityRevx &&
                                    ObjectComparer.SequentialEquals(model, currentActivity))
                                {
                                    activities.Add(activityAndUnit);
                                    continue;
                                }
                            }
                            var newActivity = new Activity();
                            Mapper.Map(model, newActivity);
                            newActivity.UpdatedBy = userId;
                            newActivity.ActivityRevx = codesLookup.Get(model.ActivityRevxCategory.Code).Id;
                            activities.Add(new ActivityStatisticalUnit() {Activity = newActivity});
                        }
                        var activitiesUnits = unit.ActivitiesUnits;
                        activitiesUnits.Clear();
                        unit.ActivitiesUnits.AddRange(activities);
                    }

                    if (work != null)
                    {
                        await work(unit);
                    }
                });

        private async Task EditContext<TUnit, TModel>(
            TModel data,
            Func<TModel, int> idSelector,
            string userId,
            Func<TUnit, Task> work)
            where TModel : IStatUnitM
            where TUnit : class, IStatisticalUnit, new()
        {
            var unit = (TUnit) await ValidateChanges<TUnit>(data, idSelector(data));
            await InitializeDataAccessAttributes(_userService, data, userId, unit.UnitType);

            var hUnit = new TUnit();
            Mapper.Map(unit, hUnit);
            Mapper.Map(data, unit);

            //External Mappings
            if (work != null)
            {
                await work(unit);
            }

            AddAddresses(_dbContext, unit, data);
            if (IsNoChanges(unit, hUnit)) return;

            unit.UserId = userId;
            unit.ChangeReason = data.ChangeReason;
            unit.EditComment = data.EditComment;

            _dbContext.Set<TUnit>().Add((TUnit) TrackHistory(unit, hUnit));

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                //TODO: Processing Validation Errors
                throw new BadRequestException(nameof(Resource.SaveError), e);
            }
        }

        private async Task<IStatisticalUnit> ValidateChanges<T>(IStatUnitM data, int regid)
            where T : class, IStatisticalUnit
        {
            var unit = await GetStatisticalUnitByIdAndType(
                _dbContext,
                regid,
                StatisticalUnitsTypeHelper.GetStatUnitMappingType(typeof(T)),
                false);

            if (!unit.Name.Equals(data.Name) &&
                !NameAddressIsUnique<T>(_dbContext, data.Name, data.Address, data.ActualAddress))
                throw new BadRequestException(
                    $"{typeof(T).Name} {nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);
            if (data.Address != null && data.ActualAddress != null && !data.Address.Equals(unit.Address) &&
                !data.ActualAddress.Equals(unit.ActualAddress) &&
                !NameAddressIsUnique<T>(_dbContext, data.Name, data.Address, data.ActualAddress))
                throw new BadRequestException(
                    $"{typeof(T).Name} {nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);
            if (data.Address != null && !data.Address.Equals(unit.Address) &&
                !NameAddressIsUnique<T>(_dbContext, data.Name, data.Address, null))
                throw new BadRequestException(
                    $"{typeof(T).Name} {nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);
            if (data.ActualAddress != null && !data.ActualAddress.Equals(unit.ActualAddress) &&
                !NameAddressIsUnique<T>(_dbContext, data.Name, null, data.ActualAddress))
                throw new BadRequestException(
                    $"{typeof(T).Name} {nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);

            return unit;
        }

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
            var statUnit = unit as StatisticalUnit;
            if (statUnit == null) return true;
            var hstatUnit = (StatisticalUnit) hUnit;
            return hstatUnit.ActivitiesUnits.CompareWith(statUnit.ActivitiesUnits, v => v.ActivityId);
        }
    }
}
