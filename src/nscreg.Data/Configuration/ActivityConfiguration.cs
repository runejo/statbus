using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using nscreg.Data.Core.EntityConfiguration;
using nscreg.Data.Entities;

namespace nscreg.Data.Configuration
{
    /// <summary>
    /// Класс конфигурации деятельности
    /// </summary>
    public class ActivityConfiguration : EntityTypeConfigurationBase<Activity>
    {
        /// <summary>
        /// Метод конфигурации деятельности
        /// </summary>
        public override void Configure(EntityTypeBuilder<Activity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(v => v.UpdatedBy).IsRequired();
            builder.HasOne(v => v.UpdatedByUser).WithMany().HasForeignKey(v => v.UpdatedBy);
            builder.HasOne(v => v.ActivityRevxCategory).WithMany().HasForeignKey(v => v.ActivityRevx);
            SetColumnNames(builder);
        }

        /// <summary>
        /// Метод установки имён столбцов
        /// </summary>
        private static void SetColumnNames(EntityTypeBuilder<Activity> builder)
        {
            builder.Property(p => p.Id).HasColumnName("Id");
            builder.Property(p => p.IdDate).HasColumnName("Id_Date");
            builder.Property(p => p.ActivityRevx).HasColumnName("Activity_Revx");
            builder.Property(p => p.ActivityRevy).HasColumnName("Activity_Revy");
            builder.Property(p => p.ActivityYear).HasColumnName("Activity_Year");
            builder.Property(p => p.ActivityType).HasColumnName("Activity_Type");
            builder.Property(p => p.Employees).HasColumnName("Employees");
            builder.Property(p => p.Turnover).HasColumnName("Turnover");
            builder.Property(p => p.UpdatedBy).HasColumnName("Updated_By");
            builder.Property(p => p.UpdatedDate).HasColumnName("Updated_Date");
        }
    }
}