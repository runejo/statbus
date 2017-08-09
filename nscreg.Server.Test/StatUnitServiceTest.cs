﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using nscreg.Data.Constants;
using nscreg.Data.Entities;
using nscreg.Server.Common;
using nscreg.Server.Common.Models.Lookup;
using nscreg.Server.Common.Models.OrgLinks;
using nscreg.Server.Common.Models.StatUnits;
using nscreg.Server.Common.Models.StatUnits.Edit;
using nscreg.Server.Common.Services;
using nscreg.Server.Common.Services.StatUnit;
using nscreg.Server.Core;
using nscreg.Server.Test.Extensions;
using Xunit;
using static nscreg.TestUtils.InMemoryDb;

namespace nscreg.Server.Test
{
    public partial class StatUnitServiceTest
    {
        public StatUnitServiceTest()
        {
            StartupConfiguration.ConfigureAutoMapper();
        }

        #region SearchTests

        [Theory]
        [InlineData(StatUnitTypes.LegalUnit)]
        [InlineData(StatUnitTypes.LocalUnit)]
        [InlineData(StatUnitTypes.EnterpriseUnit)]
        [InlineData(StatUnitTypes.EnterpriseGroup)]
        public async Task SearchByNameOrAddressTest(StatUnitTypes unitType)
        {
            var unitName = Guid.NewGuid().ToString();
            var addressPart = Guid.NewGuid().ToString();
            var address = new Address {AddressPart1 = addressPart};
            using (var context = CreateDbContext())
            {
                context.Initialize();
                IStatisticalUnit unit;
                switch (unitType)
                {
                    case StatUnitTypes.LocalUnit:
                        unit = new LocalUnit {Name = unitName, Address = address};
                        context.LocalUnits.Add((LocalUnit) unit);
                        break;
                    case StatUnitTypes.LegalUnit:
                        unit = new LegalUnit {Name = unitName, Address = address};
                        context.LegalUnits.Add((LegalUnit) unit);
                        break;
                    case StatUnitTypes.EnterpriseUnit:
                        unit = new EnterpriseUnit {Name = unitName, Address = address};
                        context.EnterpriseUnits.Add((EnterpriseUnit) unit);
                        break;
                    case StatUnitTypes.EnterpriseGroup:
                        unit = new EnterpriseGroup {Name = unitName, Address = address};
                        context.EnterpriseGroups.Add((EnterpriseGroup) unit);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null);
                }
                context.SaveChanges();
                var service = new SearchService(context);

                var query = new SearchQueryM {Wildcard = unitName.Remove(unitName.Length - 1)};
                var result = await service.Search(query, DbContextExtensions.UserId);
                Assert.Equal(1, result.TotalCount);

                query = new SearchQueryM {Wildcard = addressPart.Remove(addressPart.Length - 1)};
                result = await service.Search(query, DbContextExtensions.UserId);
                Assert.Equal(1, result.TotalCount);
            }
        }

        [Fact]
        public async Task SearchByNameMultiplyResultTest()
        {

            var commonName = Guid.NewGuid().ToString();
            var legal = new LegalUnit {Name = commonName + Guid.NewGuid()};
            var local = new LocalUnit {Name = Guid.NewGuid() + commonName + Guid.NewGuid()};
            var enterprise = new EnterpriseUnit {Name = Guid.NewGuid() + commonName};
            var group = new EnterpriseGroup {Name = Guid.NewGuid() + commonName};
            using (var context = CreateDbContext())
            {
                context.Initialize();
                context.LegalUnits.Add(legal);
                context.LocalUnits.Add(local);
                context.EnterpriseUnits.Add(enterprise);
                context.EnterpriseGroups.Add(group);
                context.SaveChanges();
                var query = new SearchQueryM {Wildcard = commonName};

                var result = await new SearchService(context).Search(query, DbContextExtensions.UserId);

                Assert.Equal(4, result.TotalCount);
            }
        }

        [Theory]
        [InlineData("2017", 3)]
        [InlineData("2016", 1)]
        public async Task SearchUnitsByCode(string code, int rows)
        {
            using (var context = CreateDbContext())
            {
                context.Initialize();
                var list = new StatisticalUnit[]
                {
                    new LegalUnit {StatId = "201701", Name = "Unit1"},
                    new LegalUnit {StatId = "201602", Name = "Unit2"},
                    new LocalUnit {StatId = "201702", Name = "Unit3"},
                };
                context.StatisticalUnits.AddRange(list);
                var group = new EnterpriseGroup {StatId = "201703", Name = "Unit4"};
                context.EnterpriseGroups.Add(group);
                await context.SaveChangesAsync();

                var result = await new SearchService(context).Search(code);

                Assert.Equal(rows, result.Count);
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 2)]
        public async void SearchUsingSectorCodeIdTest(int sectorCodeId, int rows)
        {
            using (var context = CreateDbContext())
            {
                context.Initialize();
                var service = new SearchService(context);

                var list = new StatisticalUnit[]
                {
                    new LegalUnit {InstSectorCodeId = 1, Name = "Unit1"},
                    new LegalUnit {InstSectorCodeId = 2, Name = "Unit2"},
                    new EnterpriseUnit() {InstSectorCodeId = 2, Name = "Unit4"},
                    new LocalUnit {Name = "Unit3"},
                };
                context.StatisticalUnits.AddRange(list);

                var group = new EnterpriseGroup {Name = "Unit5"};
                context.EnterpriseGroups.Add(group);

                await context.SaveChangesAsync();

                var query = new SearchQueryM
                {
                    SectorCodeId = sectorCodeId,
                };

                var result = await service.Search(query, DbContextExtensions.UserId);

                Assert.Equal(rows, result.TotalCount);
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 0)]
        public async void SearchUsingLegalFormIdTest(int legalFormId, int rows)
        {
            using (var context = CreateDbContext())
            {
                context.Initialize();
                var service = new SearchService(context);

                var list = new StatisticalUnit[]
                {
                    new LegalUnit {LegalFormId = 1, Name = "Unit1"},
                    new LegalUnit {Name = "Unit2"},
                    new EnterpriseUnit() {InstSectorCodeId = 2, Name = "Unit4"},
                    new LocalUnit {Name = "Unit3"},
                };
                context.StatisticalUnits.AddRange(list);

                var group = new EnterpriseGroup {Name = "Unit5"};
                context.EnterpriseGroups.Add(group);

                await context.SaveChangesAsync();

                var query = new SearchQueryM
                {
                    LegalFormId = legalFormId,
                };

                var result = await service.Search(query, DbContextExtensions.UserId);

                Assert.Equal(rows, result.TotalCount);
            }
        }

        [Theory]
        [InlineData(StatUnitTypes.LegalUnit)]
        [InlineData(StatUnitTypes.LocalUnit)]
        [InlineData(StatUnitTypes.EnterpriseUnit)]
        [InlineData(StatUnitTypes.EnterpriseGroup)]
        private async Task SearchUsingUnitTypeTest(StatUnitTypes type)
        {
            using (var context = CreateDbContext())
            {
                context.Initialize();
                var unitName = Guid.NewGuid().ToString();
                var legal = new LegalUnit {Name = unitName};
                var local = new LocalUnit {Name = unitName};
                var enterprise = new EnterpriseUnit {Name = unitName};
                var group = new EnterpriseGroup {Name = unitName};
                context.LegalUnits.Add(legal);
                context.LocalUnits.Add(local);
                context.EnterpriseUnits.Add(enterprise);
                context.EnterpriseGroups.Add(group);
                context.SaveChanges();

                var query = new SearchQueryM
                {
                    Wildcard = unitName,
                    Type = type
                };

                var result = await new SearchService(context).Search(query, DbContextExtensions.UserId);

                Assert.Equal(1, result.TotalCount);
            }
        }

        #endregion

        #region CreateTest

        [Fact]
        public async Task CreateLegalUnit()
        {
            var unitName = Guid.NewGuid().ToString();

            using (var context = CreateDbContext())
            {
                context.Initialize();

                var activities = await CreateActivitiesAsync(context);
                var address = await CreateAddressAsync(context);
                await CreateLegalUnitAsync(context, activities, address, unitName);

                Assert.IsType<LegalUnit>(
                    context.LegalUnits.Single(x => x.Name == unitName &&
                                                   x.Address.AddressPart1 == address.AddressPart1 && !x.IsDeleted));

                Type actual = null;
                try
                {
                    await CreateLegalUnitAsync(context, activities, address, unitName);
                }
                catch (Exception e)
                {
                    actual = e.GetType();
                }
                Assert.Equal(typeof(BadRequestException), actual);
            }
        }

        [Fact]
        public async Task CreateLocalUnit()
        {
            var unitName = Guid.NewGuid().ToString();

            using (var context = CreateDbContext())
            {
                context.Initialize();

                var activities = await CreateActivitiesAsync(context);
                var legalUnit = await CreateLegalUnitAsync(context, activities, null, unitName);
                var address = await CreateAddressAsync(context);

                await CreateLocalUnitAsync(context, activities, address, unitName, legalUnit.RegId);

                Assert.IsType<LocalUnit>(
                    context.LocalUnits.Single(x => x.Name == unitName &&
                                                   x.Address.AddressPart1 == address.AddressPart1 && !x.IsDeleted));

                Type actual = null;
                try
                {
                    await CreateLocalUnitAsync(context, activities, address, unitName, legalUnit.RegId);
                    Assert.Equal(1, activities.Count);
                }
                catch (Exception e)
                {
                    actual = e.GetType();
                }
                Assert.Equal(typeof(BadRequestException), actual);
            }
        }

        [Fact]
        public async Task CreateEnterpriseUnit()
        {
            var unitName = Guid.NewGuid().ToString();

            using (var context = CreateDbContext())
            {
                context.Initialize();
                var address = await CreateAddressAsync(context);
                var activities = await CreateActivitiesAsync(context);
                var legalUnit = await CreateLegalUnitAsync(context, activities, null, Guid.NewGuid().ToString());
                var legalUnitIds = new[] {legalUnit.RegId};
                var enterpriseGroup =
                    await CreateEnterpriseGroupAsync(context, null, unitName, new int[] { }, legalUnitIds);

                await CreateEnterpriseUnitAsync(context, activities, address, unitName, legalUnitIds,
                    enterpriseGroup?.RegId);

                Assert.IsType<EnterpriseUnit>(
                    context.EnterpriseUnits.Single(x => x.Name == unitName &&
                                                        x.Address.AddressPart1 == address.AddressPart1 &&
                                                        !x.IsDeleted));

                var expected = typeof(BadRequestException);
                Type actual = null;
                try
                {
                    await CreateEnterpriseUnitAsync(context, activities, address, unitName, legalUnitIds,
                        enterpriseGroup?.RegId);
                }
                catch (Exception e)
                {
                    actual = e.GetType();
                }
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public async Task CreateEnterpriseGroup()
        {
            var unitName = Guid.NewGuid().ToString();

            using (var context = CreateDbContext())
            {
                context.Initialize();

                var address = await CreateAddressAsync(context);
                var activities = await CreateActivitiesAsync(context);
                var legalUnit = await CreateLegalUnitAsync(context, activities, null, Guid.NewGuid().ToString());
                var legalUnitIds = new[] {legalUnit.RegId};
                await CreateEnterpriseGroupAsync(context, address, unitName, new int[] { }, legalUnitIds);

                Assert.IsType<EnterpriseGroup>(
                    context.EnterpriseGroups.Single(x => x.Name == unitName &&
                                                         x.Address.AddressPart1 == address.AddressPart1 &&
                                                         !x.IsDeleted));

                Type actual = null;
                try
                {
                    await CreateEnterpriseGroupAsync(context, address, unitName, new int[] { }, legalUnitIds);
                }
                catch (Exception e)
                {
                    actual = e.GetType();
                }
                Assert.Equal(typeof(BadRequestException), actual);
            }
        }

        #endregion

        #region EditTest

        [Fact]
        public async Task EditDataAccessAttributes()
        {

            using (var context = CreateDbContext())
            {
                context.Initialize();

                var user = context.Users.Include(v => v.Roles).Single(v => v.Id == DbContextExtensions.UserId);
                user.DataAccessArray = user.DataAccessArray
                    .Where(v => !v.EndsWith(nameof(LegalUnit.ShortName))).ToArray();

                var roleIds = user.Roles.Select(v => v.RoleId).ToList();
                var rolesList = context.Roles.Where(v => roleIds.Contains(v.Id)).ToList();

                foreach (var role in rolesList)
                {
                    role.StandardDataAccessArray = role.StandardDataAccessArray
                        .Where(v => !v.EndsWith(nameof(LegalUnit.ShortName))).ToArray();
                }

                var userService = new UserService(context);

                const string unitName = "Legal with Data Access Limits";
                const string unitShortName = "Default Value";

                var unit = new LegalUnit
                {
                    Name = unitName,
                    UserId = DbContextExtensions.UserId,
                    ShortName = unitShortName,
                };
                context.LegalUnits.Add(unit);
                await context.SaveChangesAsync();

                await new EditService(context).EditLegalUnit(new LegalUnitEditM
                {
                    DataAccess =
                        await userService.GetDataAccessAttributes(DbContextExtensions.UserId, StatUnitTypes.LegalUnit),
                    RegId = unit.RegId,
                    Name = unitName,
                    ShortName = "qwerty 666 / 228 / 322"
                }, DbContextExtensions.UserId);

                await context.SaveChangesAsync();

                var name = context.LegalUnits.Where(v => v.Name == unitName && v.ParentId == null)
                    .Select(v => v.ShortName).Single();
                Assert.Equal(unitShortName, name);
            }


        }

        [Fact]
        public async Task EditActivities()
        {


            const string unitName = "Legal with activities";
            var activity1 = new Activity
            {
                ActivityYear = 2017,
                Employees = 666,
                Turnover = 1000000,
                ActivityRevxCategory =
                    new ActivityCategory {Code = "01.12.0", Name = "����������� ����", Section = "A"},
                ActivityRevy = 2,
                ActivityType = ActivityTypes.Primary,
            };

            var activity2 = new Activity
            {
                ActivityYear = 2017,
                Employees = 888,
                Turnover = 2000000,
                ActivityRevxCategory = new ActivityCategory
                {
                    Code = "01.13",
                    Name = "����������� ������, ����, �����- � ������������",
                    Section = "A"
                },
                ActivityRevy = 3,
                ActivityType = ActivityTypes.Secondary,
            };

            var activity3 = new Activity
            {
                ActivityYear = 2017,
                Employees = 999,
                Turnover = 3000000,
                ActivityRevxCategory = new ActivityCategory
                {
                    Code = "01.13.1",
                    Name = "����������� �������� ������ � �� �����",
                    Section = "A"
                },
                ActivityRevy = 4,
                ActivityType = ActivityTypes.Ancilliary,
            };


            var activityCategory = new ActivityCategory
            {
                Code = "02.3",
                Name = "���� ������������ ����������� �������������",
                Section = "A"
            };

            using (var context = CreateDbContext())
            {
                context.Initialize();

                context.ActivityCategories.Add(activityCategory);
                context.LegalUnits.AddRange(new List<LegalUnit>
                {
                    new LegalUnit
                    {
                        Name = unitName,
                        UserId = DbContextExtensions.UserId,
                        ActivitiesUnits = new List<ActivityStatisticalUnit>
                        {
                            new ActivityStatisticalUnit
                            {
                                Activity = activity1
                            },
                            new ActivityStatisticalUnit
                            {
                                Activity = activity2
                            },
                            new ActivityStatisticalUnit
                            {
                                Activity = activity3
                            }
                        }
                    },
                });
                context.SaveChanges();

                var unitId = context.LegalUnits.Single(x => x.Name == unitName).RegId;
                const int changedEmployees = 9999;
                var legalEditResult = await new EditService(context).EditLegalUnit(new LegalUnitEditM
                {
                    RegId = unitId,
                    Name = "new name test",
                    DataAccess = DbContextExtensions.DataAccessLegalUnit,
                    Activities = new List<ActivityM>()
                    {
                        new ActivityM //New
                        {
                            ActivityRevxCategory = new CodeLookupVm()
                            {
                                Id = activityCategory.Id,
                                Code = activityCategory.Code,
                            },
                            ActivityRevy = 1,
                            ActivityType = ActivityTypes.Primary,
                            Employees = 2,
                            Turnover = 10,
                            ActivityYear = 2016,
                            IdDate = new DateTime(2017, 03, 28),
                        },
                        new ActivityM //Not Changed
                        {
                            Id = activity1.Id,
                            ActivityRevxCategory = new CodeLookupVm()
                            {
                                Id = activity1.ActivityRevxCategory.Id,
                                Code = activity1.ActivityRevxCategory.Code
                            },
                            ActivityRevy = activity1.ActivityRevy,
                            ActivityType = activity1.ActivityType,
                            IdDate = activity1.IdDate,
                            Employees = activity1.Employees,
                            Turnover = activity1.Turnover,
                            ActivityYear = activity1.ActivityYear,
                        },
                        new ActivityM //Changed
                        {
                            Id = activity2.Id,
                            ActivityRevxCategory = new CodeLookupVm()
                            {
                                Id = activity2.ActivityRevxCategory.Id,
                                Code = activity2.ActivityRevxCategory.Code
                            },
                            ActivityRevy = activity2.ActivityRevy,
                            ActivityType = activity2.ActivityType,
                            IdDate = activity2.IdDate,
                            Employees = changedEmployees,
                            Turnover = activity2.Turnover,
                            ActivityYear = activity2.ActivityYear,
                        }
                    }
                }, DbContextExtensions.UserId);
                if (legalEditResult != null && legalEditResult.Any()) return;

                var unitResult = context.LegalUnits
                    .Include(v => v.ActivitiesUnits)
                    .ThenInclude(v => v.Activity)
                    .Single(v => v.RegId == unitId).Activities;

                var activities = unitResult as Activity[] ?? unitResult.ToArray();
                Assert.Equal(3, activities.Length);
                Assert.DoesNotContain(activities, v => v.Id == activity3.Id);
                Assert.Contains(activities, v => v.Id == activity1.Id);
                Assert.Contains(activities, v => v.Employees == changedEmployees);
                Assert.NotEqual(activity2.Id, activities.First(v => v.Employees == changedEmployees).Id);
            }
        }

        [Fact]
        public async Task EditLegalUnit()
        {
            var unitName = Guid.NewGuid().ToString();
            var unitNameEdit = Guid.NewGuid().ToString();
            var duplicateName = Guid.NewGuid().ToString();

            using (var context = CreateDbContext())
            {
                context.Initialize();

                var activities = await CreateActivitiesAsync(context);
                await CreateLegalUnitAsync(context, activities, null, unitName);
                await CreateLegalUnitAsync(context, activities, null, duplicateName);

                var unitId = context.LegalUnits.Single(x => x.Name == unitName).RegId;

                await EditLegalUnitAsync(context, activities, unitId, unitNameEdit);

                Assert.IsType<LegalUnit>(
                    context.LegalUnits.Single(x => x.RegId == unitId && x.Name == unitNameEdit && !x.IsDeleted));
                Assert.IsType<LegalUnit>(
                    context.LegalUnits.Single(x => x.RegId != unitId && x.ParentId == unitId && x.Name == unitName));

                Type actual = null;
                try
                {
                    await EditLegalUnitAsync(context, activities, unitId, duplicateName);
                }
                catch (Exception e)
                {
                    actual = e.GetType();
                }
                Assert.Equal(typeof(BadRequestException), actual);
            }
        }

        [Fact]
        private async Task EditLocalUnit()
        {
            var unitName = Guid.NewGuid().ToString();
            var unitNameEdit = Guid.NewGuid().ToString();
            var dublicateName = Guid.NewGuid().ToString();

            using (var context = CreateDbContext())
            {
                context.Initialize();
                var activities = await CreateActivitiesAsync(context);
                var legalUnit = await CreateLegalUnitAsync(context, activities, null, Guid.NewGuid().ToString());

                await CreateLocalUnitAsync(context, activities, null, unitName, legalUnit.RegId);
                await CreateLocalUnitAsync(context, activities, null, dublicateName, legalUnit.RegId);

                var unitId = context.LocalUnits.Single(x => x.Name == unitName).RegId;

                await EditLocalUnitAsync(context, activities, unitId, unitNameEdit, legalUnit.RegId);

                Assert.IsType<LocalUnit>(
                    context.LocalUnits.Single(x => x.RegId == unitId && x.Name == unitNameEdit && !x.IsDeleted));
                Assert.IsType<LocalUnit>(
                    context.LocalUnits.Single(x => x.RegId != unitId && x.ParentId == unitId && x.Name == unitName));

                Type actual = null;
                try
                {
                    await EditLocalUnitAsync(context, activities, unitId, dublicateName, legalUnit.RegId);
                }
                catch (Exception e)
                {
                    actual = e.GetType();
                }
                Assert.Equal(typeof(BadRequestException), actual);
            }
        }

        [Fact]
        private async Task EditEnterpriseUnit()
        {
            var unitName = Guid.NewGuid().ToString();
            var unitNameEdit = Guid.NewGuid().ToString();
            var duplicateName = Guid.NewGuid().ToString();

            using (var context = CreateDbContext())
            {
                context.Initialize();

                var activities = await CreateActivitiesAsync(context);
                var legalUnit = await CreateLegalUnitAsync(context, activities, null, Guid.NewGuid().ToString());
                var legalUnitIds = new[] {legalUnit.RegId};
                var enterpriseGroup = await CreateEnterpriseGroupAsync(context, null, Guid.NewGuid().ToString(),
                    context.EnterpriseUnits.Select(eu => eu.RegId).ToArray(), legalUnitIds);

                await CreateEnterpriseUnitAsync(context, activities, null, unitName, legalUnitIds,
                    enterpriseGroup?.RegId);
                await CreateEnterpriseUnitAsync(context, activities, null, duplicateName, legalUnitIds,
                    enterpriseGroup?.RegId);

                var editUnitId = context.EnterpriseUnits.Single(x => x.Name == unitName).RegId;

                await EditEnterpriseUnitAsync(context, activities, legalUnitIds, editUnitId, unitNameEdit,
                    enterpriseGroup?.RegId);

                Assert.IsType<EnterpriseUnit>(
                    context.EnterpriseUnits.Single(
                        x => x.RegId == editUnitId && x.Name == unitNameEdit && !x.IsDeleted));
                Assert.IsType<EnterpriseUnit>(
                    context.EnterpriseUnits.Single(
                        x => x.RegId != editUnitId && x.ParentId == editUnitId && x.Name == unitName));
                Assert.Equal(1,
                    context.EnterpriseUnits.Single(x => x.Name == unitNameEdit && x.ParentId == null).LegalUnits.Count);

                Type actual = null;
                try
                {
                    await EditEnterpriseUnitAsync(context, activities, legalUnitIds, editUnitId, duplicateName,
                        enterpriseGroup?.RegId);
                }
                catch (Exception e)
                {
                    actual = e.GetType();
                }
                Assert.Equal(typeof(BadRequestException), actual);
            }
        }

        [Fact]
        public async Task EditEnterpriseGroup()
        {
            var unitName = Guid.NewGuid().ToString();
            var unitNameEdit = Guid.NewGuid().ToString();
            var duplicateName = Guid.NewGuid().ToString();

            using (var context = CreateDbContext())
            {
                context.Initialize();

                var activities = await CreateActivitiesAsync(context);
                var legalUnit = await CreateLegalUnitAsync(context, activities, null, Guid.NewGuid().ToString());
                var legalUnitsIds = new[] {legalUnit.RegId};
                var enterpriseGroup = await CreateEnterpriseGroupAsync(context, null, Guid.NewGuid().ToString(),
                    context.EnterpriseUnits.Select(eu => eu.RegId).ToArray(), legalUnitsIds);

                await CreateEnterpriseUnitAsync(context, activities, null, unitName, legalUnitsIds,
                    enterpriseGroup?.RegId);
                await CreateEnterpriseUnitAsync(context, activities, null, duplicateName, legalUnitsIds,
                    enterpriseGroup?.RegId);

                var enterpriseUnitsIds = context.EnterpriseUnits.Select(eu => eu.RegId).ToArray();

                await CreateEnterpriseGroupAsync(context, null, unitName, enterpriseUnitsIds, legalUnitsIds);
                await CreateEnterpriseGroupAsync(context, null, duplicateName, enterpriseUnitsIds, legalUnitsIds);

                var unitId = context.EnterpriseGroups.Single(x => x.Name == unitName).RegId;

                await EditEnterpriseGroupAsync(context, unitId, unitNameEdit, enterpriseUnitsIds, legalUnitsIds);

                Assert.IsType<EnterpriseGroup>(
                    context.EnterpriseGroups.Single(
                        x => x.RegId == unitId && x.Name == unitNameEdit && !x.IsDeleted));
                Assert.IsType<EnterpriseGroup>(
                    context.EnterpriseGroups.Single(
                        x => x.RegId != unitId && x.ParentId == unitId && x.Name == unitName));

                Type actual = null;
                try
                {
                    await EditEnterpriseGroupAsync(context, unitId, duplicateName, enterpriseUnitsIds, legalUnitsIds);
                }
                catch (Exception e)
                {
                    actual = e.GetType();
                }
                Assert.Equal(typeof(BadRequestException), actual);
            }
        }

        #endregion

        #region DeleteTest

        [Theory]
        [InlineData(StatUnitTypes.LegalUnit)]
        [InlineData(StatUnitTypes.LocalUnit)]
        [InlineData(StatUnitTypes.EnterpriseUnit)]
        [InlineData(StatUnitTypes.EnterpriseGroup)]
        public void DeleteTest(StatUnitTypes type)
        {
            var unitName = Guid.NewGuid().ToString();
            using (var context = CreateDbContext())
            {
                int unitId;
                switch (type)
                {
                    case StatUnitTypes.LegalUnit:
                        context.LegalUnits.Add(new LegalUnit
                        {
                            Name = unitName,
                            IsDeleted = false,
                            UserId = DbContextExtensions.UserId
                        });
                        context.SaveChanges();
                        unitId = context.LegalUnits.Single(x => x.Name == unitName && !x.IsDeleted).RegId;
                        new DeleteService(context).DeleteUndelete(type, unitId, true, DbContextExtensions.UserId);
                        Assert.IsType<LegalUnit>(context.LegalUnits.Single(x => x.Name == unitName && x.IsDeleted));
                        Assert.IsType<LegalUnit>(
                            context.LegalUnits.Single(x => x.Name == unitName && !x.IsDeleted && x.ParentId == unitId));
                        break;
                    case StatUnitTypes.LocalUnit:
                        context.LocalUnits.Add(new LocalUnit
                        {
                            Name = unitName,
                            IsDeleted = false,
                            UserId = DbContextExtensions.UserId
                        });
                        context.SaveChanges();
                        unitId = context.LocalUnits.Single(x => x.Name == unitName && !x.IsDeleted).RegId;
                        new DeleteService(context).DeleteUndelete(type, unitId, true, DbContextExtensions.UserId);
                        Assert.IsType<LocalUnit>(context.LocalUnits.Single(x => x.Name == unitName && x.IsDeleted));
                        Assert.IsType<LocalUnit>(
                            context.LocalUnits.Single(x => x.Name == unitName && !x.IsDeleted && x.ParentId == unitId));
                        break;
                    case StatUnitTypes.EnterpriseUnit:
                        context.EnterpriseUnits.Add(new EnterpriseUnit
                        {
                            Name = unitName,
                            IsDeleted = false,
                            UserId = DbContextExtensions.UserId
                        });
                        context.SaveChanges();
                        unitId = context.EnterpriseUnits.Single(x => x.Name == unitName && !x.IsDeleted).RegId;
                        new DeleteService(context).DeleteUndelete(type, unitId, true, DbContextExtensions.UserId);
                        Assert.IsType<EnterpriseUnit>(
                            context.EnterpriseUnits.Single(x => x.Name == unitName && x.IsDeleted));
                        Assert.IsType<EnterpriseUnit>(
                            context.EnterpriseUnits.Single(
                                x => x.Name == unitName && !x.IsDeleted && x.ParentId == unitId));
                        break;
                    case StatUnitTypes.EnterpriseGroup:
                        context.EnterpriseGroups.Add(new EnterpriseGroup
                        {
                            Name = unitName,
                            IsDeleted = false,
                            UserId = DbContextExtensions.UserId
                        });
                        context.SaveChanges();
                        unitId = context.EnterpriseGroups.Single(x => x.Name == unitName && !x.IsDeleted).RegId;
                        new DeleteService(context).DeleteUndelete(type, unitId, true, DbContextExtensions.UserId);
                        Assert.IsType<EnterpriseGroup>(
                            context.EnterpriseGroups.Single(x => x.Name == unitName && x.IsDeleted));
                        Assert.IsType<EnterpriseGroup>(
                            context.EnterpriseGroups.Single(
                                x => x.Name == unitName && !x.IsDeleted && x.ParentId == unitId));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }

        #endregion

        #region UndeleteTest

        [Theory]
        [InlineData(StatUnitTypes.LegalUnit)]
        [InlineData(StatUnitTypes.LocalUnit)]
        [InlineData(StatUnitTypes.EnterpriseUnit)]
        [InlineData(StatUnitTypes.EnterpriseGroup)]
        public void UndeleteTest(StatUnitTypes type)
        {

            var unitName = Guid.NewGuid().ToString();
            using (var context = CreateDbContext())
            {
                int unitId;
                switch (type)
                {
                    case StatUnitTypes.LegalUnit:
                        context.LegalUnits.Add(new LegalUnit
                        {
                            Name = unitName,
                            IsDeleted = true,
                            UserId = DbContextExtensions.UserId
                        });
                        context.SaveChanges();
                        unitId = context.LegalUnits.Single(x => x.Name == unitName && x.IsDeleted).RegId;
                        new DeleteService(context).DeleteUndelete(type, unitId, false, DbContextExtensions.UserId);
                        Assert.IsType<LegalUnit>(context.LegalUnits.Single(x => x.Name == unitName && !x.IsDeleted));
                        Assert.IsType<LegalUnit>(
                            context.LegalUnits.Single(x => x.Name == unitName && x.IsDeleted && x.ParentId == unitId));
                        break;
                    case StatUnitTypes.LocalUnit:
                        context.LocalUnits.Add(new LocalUnit
                        {
                            Name = unitName,
                            IsDeleted = true,
                            UserId = DbContextExtensions.UserId
                        });
                        context.SaveChanges();
                        unitId = context.LocalUnits.Single(x => x.Name == unitName && x.IsDeleted).RegId;
                        new DeleteService(context).DeleteUndelete(type, unitId, false, DbContextExtensions.UserId);
                        Assert.IsType<LocalUnit>(context.LocalUnits.Single(x => x.Name == unitName && !x.IsDeleted));
                        Assert.IsType<LocalUnit>(
                            context.LocalUnits.Single(x => x.Name == unitName && x.IsDeleted && x.ParentId == unitId));
                        break;
                    case StatUnitTypes.EnterpriseUnit:
                        context.EnterpriseUnits.Add(new EnterpriseUnit
                        {
                            Name = unitName,
                            IsDeleted = true,
                            UserId = DbContextExtensions.UserId
                        });
                        context.SaveChanges();
                        unitId = context.EnterpriseUnits.Single(x => x.Name == unitName && x.IsDeleted).RegId;
                        new DeleteService(context).DeleteUndelete(type, unitId, false, DbContextExtensions.UserId);
                        Assert.IsType<EnterpriseUnit>(
                            context.EnterpriseUnits.Single(x => x.Name == unitName && !x.IsDeleted));
                        Assert.IsType<EnterpriseUnit>(
                            context.EnterpriseUnits.Single(
                                x => x.Name == unitName && x.IsDeleted && x.ParentId == unitId));
                        break;
                    case StatUnitTypes.EnterpriseGroup:
                        context.EnterpriseGroups.Add(new EnterpriseGroup
                        {
                            Name = unitName,
                            IsDeleted = true,
                            UserId = DbContextExtensions.UserId
                        });
                        context.SaveChanges();
                        unitId = context.EnterpriseGroups.Single(x => x.Name == unitName && x.IsDeleted).RegId;
                        new DeleteService(context).DeleteUndelete(type, unitId, false, DbContextExtensions.UserId);
                        Assert.IsType<EnterpriseGroup>(
                            context.EnterpriseGroups.Single(x => x.Name == unitName && !x.IsDeleted));
                        Assert.IsType<EnterpriseGroup>(
                            context.EnterpriseGroups.Single(
                                x => x.Name == unitName && x.IsDeleted && x.ParentId == unitId));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }

        #endregion

        #region View OrgLinks

        [Fact]
        private async Task GetOrgLinksWithParent()
        {
            var expectedRoot = new LegalUnit {Name = "le0"};
            var childNode = new LocalUnit {Name = "lo1"};
            OrgLinksNode actualRoot;
            using (var ctx = CreateDbContext())
            {
                ctx.LegalUnits.Add(expectedRoot);
                await ctx.SaveChangesAsync();
                childNode.ParentOrgLink = expectedRoot.RegId;
                ctx.LocalUnits.Add(childNode);
                await ctx.SaveChangesAsync();

                actualRoot = await new ViewService(ctx).GetOrgLinksTree(childNode.RegId);
            }

            Assert.NotNull(actualRoot);
            Assert.NotNull(actualRoot.OrgLinksNodes);
            Assert.NotEmpty(actualRoot.OrgLinksNodes);
            Assert.Equal(childNode.RegId, actualRoot.OrgLinksNodes.First().RegId);
            Assert.Empty(actualRoot.OrgLinksNodes.First().OrgLinksNodes);
        }

        [Fact]
        private async Task GetOrgLinksWithChildNodes()
        {
            var expectedRoot = new LegalUnit {Name = "42", ParentOrgLink = null};
            OrgLinksNode actualRoot;
            using (var ctx = CreateDbContext())
            {
                ctx.LegalUnits.Add(expectedRoot);
                await ctx.SaveChangesAsync();
                ctx.LocalUnits.AddRange(
                    new LocalUnit {Name = "17", ParentOrgLink = expectedRoot.RegId},
                    new LocalUnit {Name = "3.14", ParentOrgLink = expectedRoot.RegId});
                await ctx.SaveChangesAsync();

                actualRoot = await new ViewService(ctx).GetOrgLinksTree(expectedRoot.RegId);
            }

            Assert.NotNull(actualRoot);
            Assert.True(expectedRoot.RegId > 0);
            Assert.Equal(expectedRoot.RegId, actualRoot.RegId);
            Assert.Null(actualRoot.ParentOrgLink);
            Assert.False(string.IsNullOrEmpty(expectedRoot.Name));
            Assert.Equal(expectedRoot.Name, actualRoot.Name);
            Assert.NotNull(actualRoot.OrgLinksNodes);
            Assert.NotEmpty(actualRoot.OrgLinksNodes);
            Assert.Equal(2, actualRoot.OrgLinksNodes.Count());
            Assert.Contains(actualRoot.OrgLinksNodes, x => x.Name == "17");
            Assert.Contains(actualRoot.OrgLinksNodes, x => x.Name == "3.14");
        }

        [Fact]
        private async Task GetOrgLinksWithNoChildNodes()
        {
            var expectedRoot = new LegalUnit {Name = "42", ParentOrgLink = null};
            OrgLinksNode actualRoot;
            using (var ctx = CreateDbContext())
            {
                ctx.LegalUnits.Add(expectedRoot);
                await ctx.SaveChangesAsync();

                actualRoot = await new ViewService(ctx).GetOrgLinksTree(expectedRoot.RegId);
            }

            Assert.NotNull(actualRoot);
            Assert.True(expectedRoot.RegId > 0);
            Assert.Equal(expectedRoot.RegId, actualRoot.RegId);
            Assert.Null(actualRoot.ParentOrgLink);
            Assert.False(string.IsNullOrEmpty(expectedRoot.Name));
            Assert.Equal(expectedRoot.Name, actualRoot.Name);
            Assert.NotNull(actualRoot.OrgLinksNodes);
            Assert.Empty(actualRoot.OrgLinksNodes);
        }

        #endregion
    }
}
