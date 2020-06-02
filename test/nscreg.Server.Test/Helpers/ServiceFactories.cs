using nscreg.Data;
using nscreg.Server.Common.Services;
using nscreg.Server.Common.Services.StatUnit;
using nscreg.Utilities.Configuration;
using nscreg.Utilities.Configuration.DBMandatoryFields;
using nscreg.Utilities.Configuration.StatUnitAnalysis;
using nscreg.Utilities.Enums;

namespace nscreg.Server.Test.Helpers
{
    internal static class ServiceFactories
    {
        public static DataSourcesQueueService CreateEmptyConfiguredDataSourceQueueService(NSCRegDbContext ctx)
        {
            var analysisRules = new StatUnitAnalysisRules();
            var dbMandatoryFields = new DbMandatoryFields();
            var validationSettings = new ValidationSettings();
            var createSvc = new CreateService(ctx, analysisRules, dbMandatoryFields, validationSettings, StatUnitTypeOfSave.WebApplication);
            var editSvc = new EditService(ctx, analysisRules, dbMandatoryFields, validationSettings);
            var servicesConfig = new ServicesSettings();
            return new DataSourcesQueueService(ctx, createSvc, editSvc, servicesConfig, dbMandatoryFields);
        }
    }
}
