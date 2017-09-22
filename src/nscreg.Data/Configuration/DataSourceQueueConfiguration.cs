using Microsoft.EntityFrameworkCore.Metadata.Builders;
using nscreg.Data.Core.EntityConfiguration;
using nscreg.Data.Entities;

namespace nscreg.Data.Configuration
{
    /// <summary>
    ///  Класс конфигурации очереди источника данных
    /// </summary>
    public class DataSourceQueueConfiguration: EntityTypeConfigurationBase<DataSourceQueue>
    {
        /// <summary>
        ///  Метод конфигурации очереди источника данных
        /// </summary>
        public override void Configure(EntityTypeBuilder<DataSourceQueue> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DataSourcePath).IsRequired();
            builder.Property(x => x.DataSourceFileName).IsRequired();
            builder.HasOne(x => x.DataSource).WithMany(x => x.DataSourceQueuedUploads).HasForeignKey(x => x.DataSourceId);
            builder.HasOne(x => x.User).WithMany(x => x.DataSourceQueues).HasForeignKey(x => x.UserId);
        }
    }
}
