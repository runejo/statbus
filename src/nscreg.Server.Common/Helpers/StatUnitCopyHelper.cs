using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using nscreg.Data.Entities;
using nscreg.Utilities.Extensions;

namespace nscreg.Server.Common.Helpers
{
    public partial class StatUnitCreationHelper
    {
        private async Task<T> CreateStatUnitAsync<T>(T entity) where T : class
        {
            var result = await _dbContext.Set<T>().AddAsync(entity);
            return result.Entity;
        }
        
        private async Task<LocalUnit> CreateLocalForLegalAsync(LegalUnit legalUnit)
        {
            var localUnit = new LocalUnit
            {
                AddressId = legalUnit.AddressId,
                ActualAddressId = legalUnit.ActualAddressId,
                LegalUnit = legalUnit
            };

            Mapper.Map(legalUnit, localUnit);
            await _dbContext.LocalUnits.AddAsync(localUnit);

            CreateActivitiesAndPersonsAndForeignParticipations(legalUnit.Activities, legalUnit.PersonsUnits, legalUnit.ForeignParticipationCountriesUnits, localUnit);

            return localUnit;
        }

        private async Task<EnterpriseUnit> CreateEnterpriseForLegalAsync(LegalUnit legalUnit)
        {
            var enterpriseUnit = new EnterpriseUnit
            {
                AddressId = legalUnit.AddressId,
                ActualAddressId = legalUnit.ActualAddressId,
            };
            Mapper.Map(legalUnit, enterpriseUnit);
            await _dbContext.EnterpriseUnits.AddAsync(enterpriseUnit);
            legalUnit.EnterpriseUnit = enterpriseUnit;

            CreateActivitiesAndPersonsAndForeignParticipations(legalUnit.Activities, legalUnit.PersonsUnits, legalUnit.ForeignParticipationCountriesUnits, enterpriseUnit);

            return enterpriseUnit;
        }

        private async Task<EnterpriseGroup> CreateGroupForEnterpriseAsync(EnterpriseUnit enterpriseUnit)
        {
            var enterpriseGroup = new EnterpriseGroup
            {
                AddressId = enterpriseUnit.AddressId,
                ActualAddressId = enterpriseUnit.ActualAddressId,
            };

            Mapper.Map(enterpriseUnit, enterpriseGroup);
            enterpriseUnit.EnterpriseGroup = enterpriseGroup;
            await _dbContext.EnterpriseGroups.AddAsync(enterpriseGroup);

            return enterpriseGroup;
        }

        private void CreateActivitiesAndPersonsAndForeignParticipations(IEnumerable<Activity> activities, IEnumerable<PersonStatisticalUnit> persons, IEnumerable<CountryStatisticalUnit> foreignPartCountries, StatisticalUnit unit)
        {
            activities.ForEach(x =>
            {
                _dbContext.ActivityStatisticalUnits.Add(new ActivityStatisticalUnit
                {
                    ActivityId = x.Id,
                    Unit = unit
                });
            });
            persons.ForEach(x =>
            {
                _dbContext.PersonStatisticalUnits.Add(new PersonStatisticalUnit
                {
                    PersonId = x.PersonId,
                    Unit = unit,
                    PersonTypeId = x.PersonTypeId,
                    EnterpriseGroupId = x.EnterpriseGroupId
                });
            });

            foreignPartCountries.ForEach(x =>
            {
                _dbContext.CountryStatisticalUnits.Add(new CountryStatisticalUnit
                {
                    Unit = unit,
                    CountryId = x.CountryId
                });

            });

        }
    }
}
