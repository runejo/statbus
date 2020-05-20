using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using nscreg.Data.Core.EntityConfiguration;
using nscreg.Data.Entities;
using nscreg.Utilities.Enums;
using nscreg.Utilities.Extensions;

namespace nscreg.Data.Configuration
{
    /// <summary>
    ///  Enterprise Group Configuration Class
    /// </summary>
    public class EnterpriseGroupConfiguration : EntityTypeConfigurationBase<EnterpriseGroup>
    {
        public override void Configure(EntityTypeBuilder<EnterpriseGroup> builder)
        {
            builder.HasKey(x => x.RegId);
            builder.HasMany(x => x.EnterpriseUnits).WithOne(x => x.EnterpriseGroup).HasForeignKey(x => x.EntGroupId).IsRequired(false);
            builder.ToTable("EnterpriseGroups");
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.ChangeReason).IsRequired().HasDefaultValue(ChangeReasons.Create);
            builder.Property(x => x.EditComment).IsNullable();
            builder.Property(x => x.Name).HasMaxLength(400);
            builder.HasIndex(x => x.Name);
            builder.HasIndex(x => x.StartPeriod);

            builder.Ignore(x => x.LegalForm);
            builder.Ignore(x => x.InstSectorCode);
            builder.Ignore(x => x.ActivitiesUnits);
            builder.Ignore(x => x.ForeignParticipationCountriesUnits);
        }
    }
}
