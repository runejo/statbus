using nscreg.Data.Constants;
using nscreg.Data.Entities;
using nscreg.Server.Common.Services.DataSources;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nscreg.Data;
using Newtonsoft.Json;
using Xunit;
using static nscreg.TestUtils.InMemoryDb;

namespace nscreg.Services.Test.DataSources.QueueServiceTest
{
    public class QueueServiceGetStatUnitFromRawEntityTest
    {
        [Fact]
        private async Task ShouldCreateStatUnitWithoutComplexEntities()
        {
            const string expected = "42", sourceProp = "activities";
            var raw = new Dictionary<string, string> { [sourceProp] = expected };
            var mapping = new[] { (sourceProp, nameof(StatisticalUnit.StatId)) };
            LegalUnit actual;

            using (var ctx = CreateDbContext())
                actual = await new QueueService(ctx)
                    .GetStatUnitFromRawEntity(raw, StatUnitTypes.LegalUnit, mapping, DataSourceUploadTypes.StatUnits, DataSourceAllowedOperation.Create) as LegalUnit;

            Assert.NotNull(actual);
            Assert.Equal(expected, actual.StatId);
        }

        [Fact]
        private async Task ShouldGetExistingStatUnitWithoutComplexEntities()
        {
            var raw = new Dictionary<string, string> { ["sourceProp"] = "name42", ["sourceId"] = "42" };
            var mapping = new[] { ("sourceProp", "Name"), ("sourceId", nameof(StatisticalUnit.StatId)) };
            LocalUnit actual;

            using (var ctx = CreateDbContext())
            {
                var unit = new LocalUnit { StatId = "42" };
                ctx.LocalUnits.Add(unit);
                await ctx.SaveChangesAsync();
                actual = await new QueueService(ctx)
                    .GetStatUnitFromRawEntity(raw, StatUnitTypes.LocalUnit, mapping, DataSourceUploadTypes.StatUnits, DataSourceAllowedOperation.Alter) as LocalUnit;
            }

            Assert.NotNull(actual);
            Assert.Equal("42", actual.StatId);
            Assert.Equal("name42", actual.Name);
        }

        [Fact]
        private async Task ShouldCreateStatUnitAndCreateActivity()
        {
            const string expectedCode = "01.13.1", sourceProp = "activities";
            var raw = new Dictionary<string, string>{[sourceProp] = expectedCode };
            var propPath =
                $"{nameof(StatisticalUnit.Activities)}.{nameof(Activity.ActivityCategory)}.{nameof(ActivityCategory.Code)}";
            var mapping = new[] {(sourceProp, propPath)};
            LegalUnit actual;

            using (var ctx = CreateDbContext())
            {
                CreateActivityCategory(ctx, expectedCode);
                actual = await new QueueService(ctx)
                    .GetStatUnitFromRawEntity(raw, StatUnitTypes.LegalUnit, mapping, DataSourceUploadTypes.StatUnits, DataSourceAllowedOperation.Create) as LegalUnit;
            }

            Assert.NotNull(actual);
            Assert.NotEmpty(actual.Activities);
            Assert.NotNull(actual.Activities.First());
            Assert.NotNull(actual.Activities.First().ActivityCategory);
            Assert.Equal(expectedCode, actual.Activities.First().ActivityCategory.Code);
        }

        [Fact]
        private async Task ShouldCreateStatUnitAndGetExistingActivity()
        {
            int expectedId;
            const string expected = "42", sourceProp = "activities";
            var raw = new Dictionary<string, string>
            {
                [sourceProp] = expected
            };
            var propPath =
                $"{nameof(StatisticalUnit.Activities)}.{nameof(Activity.ActivityCategory)}.{nameof(ActivityCategory.Code)}";
            var mapping = new[]
                {(sourceProp, propPath)};
            LegalUnit actual;

            using (var ctx = CreateDbContext())
            {
                var activityCategory = new ActivityCategory {Code = expected};
                ctx.Activities.Add(new Activity {ActivityCategory = activityCategory});
                await ctx.SaveChangesAsync();
                expectedId = activityCategory.Id;
                actual = await new QueueService(ctx)
                    .GetStatUnitFromRawEntity(raw, StatUnitTypes.LegalUnit, mapping, DataSourceUploadTypes.StatUnits, DataSourceAllowedOperation.CreateAndAlter) as LegalUnit;
            }

            Assert.NotNull(actual);
            Assert.NotEmpty(actual.Activities);
            Assert.NotNull(actual.Activities.First());
            Assert.NotNull(actual.Activities.First().ActivityCategory);
            Assert.Equal(expected, actual.Activities.First().ActivityCategory.Code);
            Assert.Equal(expectedId, actual.Activities.First().ActivityCategory.Id);
        }

        private static void CreateActivityCategory(NSCRegDbContext context, string code)
        {
            context.ActivityCategories.Add(new ActivityCategory
            {
                Code = code,
                Name = "Activity category name",
                Section = "A"
            });
            context.SaveChanges();
        }
    }
}
