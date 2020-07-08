using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using nscreg.Data;
using nscreg.Data.Constants;
using nscreg.Data.Entities;
using nscreg.Server.Common.Models.Addresses;
using nscreg.Server.Common.Models.StatUnits;
using nscreg.Server.Common.Models.StatUnits.Create;
using nscreg.Server.Common.Models.StatUnits.Edit;
using nscreg.Server.Common.Services;
using nscreg.Server.Common.Services.StatUnit;
using nscreg.Server.Test.Extensions;
using System.Linq;
using nscreg.Utilities.Configuration;
using nscreg.Utilities.Configuration.DBMandatoryFields;
using nscreg.Utilities.Configuration.StatUnitAnalysis;
using nscreg.Utilities.Enums;
using Activity = nscreg.Data.Entities.Activity;
using EnterpriseGroup = nscreg.Data.Entities.EnterpriseGroup;
using LegalUnit = nscreg.Data.Entities.LegalUnit;

// ReSharper disable once CheckNamespace
namespace nscreg.Server.Test
{
    public class StatUnitTestHelper
    {
        private const int RegionId = 100;
        private readonly StatUnitAnalysisRules _analysisRules;
        private readonly DbMandatoryFields _mandatoryFields;
        private readonly ValidationSettings _validationSettings;

        public StatUnitTestHelper(StatUnitAnalysisRules analysisRules, DbMandatoryFields mandatoryFields, ValidationSettings validationSettings)
        {
            _analysisRules = analysisRules;
            _mandatoryFields = mandatoryFields;
            _validationSettings = validationSettings;
        }

        public async Task<LegalUnit> CreateLegalUnitAsync(NSCRegDbContext context, List<ActivityM> activities,
            AddressM address, string unitName)
        {
            await new CreateService(context, _analysisRules, _mandatoryFields, _validationSettings, StatUnitTypeOfSave.WebApplication).CreateLegalUnit(new LegalUnitCreateM
            {
                DataAccess = DbContextExtensions.DataAccessLegalUnit,
                Name = unitName,
                Address = address ?? await CreateAddressAsync(context),
                Activities = activities ?? new List<ActivityM>(),
                DataSource = nameof(LegalUnitCreateM.DataSource),
                DataSourceClassificationId = 1,
                RegistrationReasonId = 1,
                ContactPerson = Guid.NewGuid().ToString(),
                ShortName = Guid.NewGuid().ToString(),
                TelephoneNo = Guid.NewGuid().ToString(),
                Persons = new List<PersonM>
                {
                    new PersonM
                    {
                        Role = 1
                    },
                    new PersonM
                    {
                        Role = 2
                    }
                },
                UnitStatusId = 1
            }, DbContextExtensions.UserId);

            return context.LegalUnits.FirstOrDefault();
        }

        public async Task CreateLocalUnitAsync(NSCRegDbContext context, List<ActivityM> activities, AddressM address,
            string unitName, int legalUnitRegId)
        {
            await new CreateService(context, _analysisRules, _mandatoryFields, _validationSettings, StatUnitTypeOfSave.WebApplication).CreateLocalUnit(new LocalUnitCreateM
            {
                DataAccess = DbContextExtensions.DataAccessLocalUnit,
                Name = unitName,
                Address = address ?? await CreateAddressAsync(context),
                Activities = activities ?? await CreateActivitiesAsync(context),
                DataSource = nameof(LegalUnitCreateM.DataSource),
                DataSourceClassificationId = 1,
                RegistrationReasonId = 1,
                ContactPerson = Guid.NewGuid().ToString(),
                ShortName = Guid.NewGuid().ToString(),
                TelephoneNo = Guid.NewGuid().ToString(),
                LegalUnitId = legalUnitRegId,
                Persons = new List<PersonM>
                {
                    new PersonM
                    {
                        Role = 1
                    },
                    new PersonM
                    {
                        Role = 2
                    }
                },
                UnitStatusId = 1
            }, DbContextExtensions.UserId);
        }

        public async Task CreateEnterpriseUnitAsync(NSCRegDbContext context, List<ActivityM> activities,
            AddressM address, string unitName, int[] legalUnitIds, int? enterpriseGroupId)
        {
            await new CreateService(context, _analysisRules, _mandatoryFields, _validationSettings, StatUnitTypeOfSave.WebApplication).CreateEnterpriseUnit(new EnterpriseUnitCreateM
            {
                DataAccess = DbContextExtensions.DataAccessEnterpriseUnit,
                Name = unitName,
                Address = address ?? await CreateAddressAsync(context),
                Activities = activities,
                DataSource = nameof(LegalUnitCreateM.DataSource),
                DataSourceClassificationId = 1,
                RegistrationReasonId = 1,
                ContactPerson = Guid.NewGuid().ToString(),
                ShortName = Guid.NewGuid().ToString(),
                TelephoneNo = Guid.NewGuid().ToString(),
                LegalUnits = legalUnitIds,
                EntGroupId = enterpriseGroupId,
                Persons = CreatePersons(),
                UnitStatusId = 1
            }, DbContextExtensions.UserId);
        }

        public async Task<EnterpriseGroup> CreateEnterpriseGroupAsync(NSCRegDbContext context, AddressM address,
            string unitName, int[] enterpriseUnitsIds, int[] legalUnitsIds)
        {
            await new CreateService(context, _analysisRules, _mandatoryFields, _validationSettings, StatUnitTypeOfSave.WebApplication).CreateEnterpriseGroup(new EnterpriseGroupCreateM
            {
                DataAccess = DbContextExtensions.DataAccessEnterpriseGroup,
                Name = unitName,
                Address = address ?? await CreateAddressAsync(context),
                DataSource = nameof(LegalUnitCreateM.DataSource),
                DataSourceClassificationId = 1,
                RegistrationReasonId = 1,
                ContactPerson = Guid.NewGuid().ToString(),
                ShortName = Guid.NewGuid().ToString(),
                TelephoneNo = Guid.NewGuid().ToString(),
                EnterpriseUnits = enterpriseUnitsIds,
            }, DbContextExtensions.UserId);

            return context.EnterpriseGroups.FirstOrDefault();
        }


        public async Task<AddressM> CreateAddressAsync(NSCRegDbContext context)
        {
            var address = await new AddressService(context).CreateAsync(new AddressModel
            {
                AddressPart1 = Guid.NewGuid().ToString(),
                RegionId = RegionId
            });

            return new AddressM
            {
                Id = address.Id,
                AddressPart1 = address.AddressPart1,
                RegionId = address.RegionId
            };
        }

        public async Task<List<ActivityM>> CreateActivitiesAsync(NSCRegDbContext context)
        {
            var localActivity = context.Activities.Add(new Activity
            {
                ActivityYear = 2017,
                Employees = 888,
                Turnover = 2000000,
                ActivityCategory = new ActivityCategory
                {
                    Code = "01.13.1",
                    Name = "Activity Category",
                    Section = "A"
                },
                ActivityType = ActivityTypes.Secondary,
            });
            await context.SaveChangesAsync();

            return new List<ActivityM>
            {
                new ActivityM
                {
                    Id = localActivity.Entity.Id,
                    ActivityYear = localActivity.Entity.ActivityYear,
                    Employees = localActivity.Entity.Employees,
                    Turnover = localActivity.Entity.Turnover,
                    ActivityCategoryId = localActivity.Entity.ActivityCategory.Id,
                    ActivityType = ActivityTypes.Primary,
                }
            };
        }

        private List<PersonM> CreatePersons()
        {
            return new List<PersonM>
            {
                new PersonM
                {
                    Role = 1
                },
                new PersonM
                {
                    Role = 2
                }
            };
        }


        public async Task EditLegalUnitAsync(NSCRegDbContext context, List<ActivityM> activities, int unitId,
            string unitNameEdit)
        {
            await new EditService(context, _analysisRules, _mandatoryFields, _validationSettings).EditLegalUnit(new LegalUnitEditM
            {
                RegId = unitId,
                Name = unitNameEdit,
                DataAccess = DbContextExtensions.DataAccessLegalUnit,
                DataSourceClassificationId = 1,
                RegistrationReasonId = 1,
                Address = await CreateAddressAsync(context),
                Activities = activities ?? new List<ActivityM>(),
                DataSource = nameof(LegalUnitCreateM.DataSource),
                ContactPerson = nameof(LegalUnitCreateM.ContactPerson),
                ShortName = nameof(LegalUnitCreateM.ShortName),
                TelephoneNo = nameof(LegalUnitCreateM.TelephoneNo),
                Persons = new List<PersonM>
                {
                    new PersonM
                    {
                        Role = 1
                    },
                    new PersonM
                    {
                        Role = 2
                    }
                },
                UnitStatusId = 1
            }, DbContextExtensions.UserId);
        }

        public async Task EditLocalUnitAsync(NSCRegDbContext context, List<ActivityM> activities, int unitId,
            string unitNameEdit, int legalUnitRegId)
        {
            await new EditService(context, _analysisRules, _mandatoryFields, _validationSettings).EditLocalUnit(new LocalUnitEditM
            {
                DataAccess = DbContextExtensions.DataAccessLocalUnit,
                RegId = unitId,
                Name = unitNameEdit,
                Address = await CreateAddressAsync(context),
                Activities = activities,
                DataSource = nameof(LegalUnitCreateM.DataSource),
                DataSourceClassificationId = 1,
                RegistrationReasonId = 1,
                ContactPerson = nameof(LegalUnitCreateM.ContactPerson),
                ShortName = nameof(LegalUnitCreateM.ShortName),
                TelephoneNo = nameof(LegalUnitCreateM.TelephoneNo),
                LegalUnitId = legalUnitRegId,
                Persons = CreatePersons(),
                UnitStatusId = 1
            }, DbContextExtensions.UserId);
        }

        public async Task EditEnterpriseUnitAsync(NSCRegDbContext context, List<ActivityM> activities,
            int[] legalUnitsIds, int unitId, string unitNameEdit, int? enterpriseGroupId)
        {
            await new EditService(context, _analysisRules, _mandatoryFields, _validationSettings).EditEnterpriseUnit(new EnterpriseUnitEditM
            {
                RegId = unitId,
                Name = unitNameEdit,
                LegalUnits = legalUnitsIds,
                DataAccess = DbContextExtensions.DataAccessEnterpriseUnit,
                DataSourceClassificationId = 1,
                RegistrationReasonId = 1,
                Address = await CreateAddressAsync(context),
                Activities = activities,
                DataSource = nameof(LegalUnitCreateM.DataSource),
                ContactPerson = nameof(LegalUnitCreateM.ContactPerson),
                ShortName = nameof(LegalUnitCreateM.ShortName),
                TelephoneNo = nameof(LegalUnitCreateM.TelephoneNo),
                EntGroupId = enterpriseGroupId,
                Persons = CreatePersons(),
                UnitStatusId = 1
            }, DbContextExtensions.UserId);
        }

        public async Task EditEnterpriseGroupAsync(NSCRegDbContext context, int unitId, string unitNameEdit,
            int[] enterpriseUnitsIds, int[] legalUnitsIds)
        {
            await new EditService(context, _analysisRules, _mandatoryFields, _validationSettings).EditEnterpriseGroup(new EnterpriseGroupEditM
            {
                RegId = unitId,
                Name = unitNameEdit,
                EnterpriseUnits = enterpriseUnitsIds,
                DataAccess = DbContextExtensions.DataAccessEnterpriseGroup,
                DataSourceClassificationId = 1,
                RegistrationReasonId = 1,
                Address = await CreateAddressAsync(context),
                DataSource = nameof(LegalUnitCreateM.DataSource),
                ContactPerson = nameof(LegalUnitCreateM.ContactPerson),
                ShortName = nameof(LegalUnitCreateM.ShortName),
                TelephoneNo = nameof(LegalUnitCreateM.TelephoneNo),
            }, DbContextExtensions.UserId);
        }

    }
}
