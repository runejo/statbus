using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using nscreg.Data.Entities;
using nscreg.Resources.Languages;
using nscreg.Utilities.Extensions;

namespace nscreg.Server.Common.Helpers
{
    public partial class StatUnitCreationHelper
    {
        private async Task<T> CreateStatUnitAsync<T>(T entity) where T : class
        {
            var result = _dbContext.Set<T>().Add(entity);
            
            try
            {
                await _dbContext.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception e)
            {
                throw new BadRequestException(nameof(Resource.SaveError), e);
            }
        }
        
        private async Task<LocalUnit> CreateLocalForLegalAsync(LegalUnit legalUnit)
        {
            var localUnit = new LocalUnit
            {
                AddressId = legalUnit.AddressId,
                ActualAddressId = legalUnit.ActualAddressId,
                LegalUnitId = legalUnit.RegId
            };

            Mapper.Map(legalUnit, localUnit);
            _dbContext.LocalUnits.Add(localUnit);
            await _dbContext.SaveChangesAsync();

            legalUnit.HistoryLocalUnitIds = localUnit.RegId.ToString();
            _dbContext.LegalUnits.Update(legalUnit);
            await _dbContext.SaveChangesAsync();

            CreateActivitiesAndPersonsAndForeignParticipations(legalUnit.Activities, legalUnit.PersonsUnits, legalUnit.ForeignParticipationCountriesUnits, localUnit.RegId);
            await _dbContext.SaveChangesAsync();

            return localUnit;
        }

        private async Task<EnterpriseUnit> CreateEnterpriseForLegalAsync(LegalUnit legalUnit)
        {
            var enterpriseUnit = new EnterpriseUnit
            {
                AddressId = legalUnit.AddressId,
                ActualAddressId = legalUnit.ActualAddressId,
                HistoryLegalUnitIds = legalUnit.RegId.ToString()
            };
            Mapper.Map(legalUnit, enterpriseUnit);
            _dbContext.EnterpriseUnits.Add(enterpriseUnit);
            await _dbContext.SaveChangesAsync();

            legalUnit.EnterpriseUnitRegId = enterpriseUnit.RegId;
            _dbContext.LegalUnits.Update(legalUnit);
            await _dbContext.SaveChangesAsync();

            CreateActivitiesAndPersonsAndForeignParticipations(legalUnit.Activities, legalUnit.PersonsUnits, legalUnit.ForeignParticipationCountriesUnits, enterpriseUnit.RegId);
            await _dbContext.SaveChangesAsync();

            return enterpriseUnit;
        }

        private async Task<EnterpriseGroup> CreateGroupForEnterpriseAsync(EnterpriseUnit enterpriseUnit)
        {
            var enterpriseGroup = new EnterpriseGroup
            {
                AddressId = enterpriseUnit.AddressId,
                ActualAddressId = enterpriseUnit.ActualAddressId,
                HistoryEnterpriseUnitIds = enterpriseUnit.RegId.ToString()
            };

            Mapper.Map(enterpriseUnit, enterpriseGroup);
            _dbContext.EnterpriseGroups.Add(enterpriseGroup);
            await _dbContext.SaveChangesAsync();

            enterpriseUnit.EntGroupId = enterpriseGroup.RegId;
            _dbContext.EnterpriseUnits.Update(enterpriseUnit);
            await _dbContext.SaveChangesAsync();

            return enterpriseGroup;
        }

        private void CreateActivitiesAndPersonsAndForeignParticipations(IEnumerable<Activity> activities, IEnumerable<PersonStatisticalUnit> persons, IEnumerable<CountryStatisticalUnit> foreignPartCountries, int statUnitId)
        {
            activities.ForEach(x =>
            {
                _dbContext.ActivityStatisticalUnits.Add(new ActivityStatisticalUnit
                {
                    ActivityId = x.Id,
                    UnitId = statUnitId
                });
            });
            persons.ForEach(x =>
            {
                _dbContext.PersonStatisticalUnits.Add(new PersonStatisticalUnit
                {
                    PersonId = x.PersonId,
                    UnitId = statUnitId,
                    PersonTypeId = x.PersonTypeId,
                    EnterpriseGroupId = x.EnterpriseGroupId
                });
            });

            foreignPartCountries.ForEach(x =>
            {
                _dbContext.CountryStatisticalUnits.Add(new CountryStatisticalUnit
                {
                    UnitId = statUnitId,
                    CountryId = x.CountryId
                });

            });

        }
    }
}
