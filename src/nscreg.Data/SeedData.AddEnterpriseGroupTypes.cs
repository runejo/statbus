using Microsoft.EntityFrameworkCore.Internal;
using nscreg.Data.Entities;

namespace nscreg.Data
{
    internal static partial class SeedData
    {
        public static void AddEnterpriseGroupTypes(NSCRegDbContext context)
        {
            if (!context.EnterpriseGroupTypes.Any())
            {
                context.EnterpriseGroupTypes.AddRange(
                    new EnterpriseGroupType() { Name = "All-residents", NameLanguage1 = "Все резиденты", NameLanguage2 = "Бардык резидент" },
                    new EnterpriseGroupType() { Name = "Multinational domestically controlled ", NameLanguage1 = "Многонациональный внутренний контроль", NameLanguage2 = "Көп улуттуу өлкө башкарылат" },
                    new EnterpriseGroupType() { Name = "Multinational foreign controlled", NameLanguage1 = "Многонациональный иностранный контроль", NameLanguage2 = "Көп улуттуу чет элдик көзөмөлдө" });
                context.SaveChanges();
            }
        }
    }
}
