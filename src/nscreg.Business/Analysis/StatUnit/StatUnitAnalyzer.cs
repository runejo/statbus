using System;
using System.Collections.Generic;
using System.Linq;
using nscreg.Business.Analysis.Contracts;
using nscreg.Business.Analysis.StatUnit.Managers.Duplicates;
using nscreg.Business.Analysis.StatUnit.Managers.MandatoryFields;
using nscreg.Data;
using nscreg.Data.Entities;
using nscreg.Utilities.Configuration.DBMandatoryFields;
using nscreg.Utilities.Configuration.StatUnitAnalysis;
using nscreg.Utilities.Extensions;
using Microsoft.EntityFrameworkCore;
using nscreg.Business.Analysis.StatUnit.Managers.AnalysisChecks;
using nscreg.Business.PredicateBuilders;
using nscreg.Resources.Languages;
using nscreg.Utilities.Configuration;
using EnterpriseGroup = nscreg.Data.Entities.EnterpriseGroup;
using LocalUnit = nscreg.Data.Entities.LocalUnit;

namespace nscreg.Business.Analysis.StatUnit
{
    /// <inheritdoc />
    /// <summary>
    /// Statistical unit analyzer
    /// </summary>
    public class StatUnitAnalyzer : IStatUnitAnalyzer
    {
        private readonly StatUnitAnalysisRules _analysisRules;
        private readonly DbMandatoryFields _mandatoryFields;
        private readonly NSCRegDbContext _context;
        private readonly ValidationSettings _validationSettings;

        public StatUnitAnalyzer(StatUnitAnalysisRules analysisRules, DbMandatoryFields mandatoryFields,
            NSCRegDbContext context, ValidationSettings validationSettings)
        {
            _analysisRules = analysisRules;
            _mandatoryFields = mandatoryFields;
            _context = context;
            _validationSettings = validationSettings;
        }

        /// <inheritdoc />
        /// <summary>
        /// Check statistical unit connections
        /// </summary>
        /// <param name="unit">Stat unit</param>
        /// <returns>Dictionary of messages</returns>
        public Dictionary<string, string[]> CheckConnections(IStatisticalUnit unit)
        {
            var messages = new Dictionary<string, string[]>();

            if (_analysisRules.Connections.CheckRelatedLegalUnit)
            {
                var hasRelatedLegalUnit = unit is LocalUnit localUnit
                    ? localUnit.LegalUnitId != null
                    : !(unit is EnterpriseUnit) || ((EnterpriseUnit) unit).LegalUnits.Any();

                if (!hasRelatedLegalUnit)
                    messages.Add(unit is LocalUnit ? nameof(LocalUnit.LegalUnitId) : nameof(EnterpriseUnit.LegalUnits),
                        new[] { nameof(Resource.AnalysisRelatedLegalUnit)});
            }

            if (_analysisRules.Connections.CheckRelatedActivities && _mandatoryFields.StatUnit.Activities)
            {
                var hasRelatedActivities = !(unit is LocalUnit) && !(unit is EnterpriseUnit) ||
                                           ((StatisticalUnit) unit).ActivitiesUnits.Any();
                if (!hasRelatedActivities)
                    messages.Add(nameof(StatisticalUnit.Activities), new[] { nameof(Resource.AnalysisRelatedActivity) });
            }

            if (_analysisRules.Connections.CheckAddress && unit.Address == null && _mandatoryFields.StatUnit.Address)
                messages.Add(nameof(StatisticalUnit.Address), new[] {nameof(Resource.AnalysisRelatedAddress)});

            return messages;
        }

        /// <inheritdoc />
        /// <summary>
        /// Check statistical unit mandatory fields
        /// </summary>
        /// <param name="unit">Stat unit</param>
        /// <returns>Dictionary of messages</returns>
        public Dictionary<string, string[]> CheckMandatoryFields(IStatisticalUnit unit)
        {
            var manager = unit is StatisticalUnit statisticalUnit
                ? new StatisticalUnitMandatoryFieldsManager(statisticalUnit, _mandatoryFields) as IAnalysisManager
                : new EnterpriseGroupMandatoryFieldsManager(unit as EnterpriseGroup, _mandatoryFields);

            return manager.CheckFields();
        }

        /// <inheritdoc />
        /// <summary>
        /// Check statistical unit calculation fields
        /// </summary>
        /// <param name="unit">Stat unit</param>
        /// <returns>Dictionary of messages</returns>
        public Dictionary<string, string[]> CheckCalculationFields(IStatisticalUnit unit)
        {
            var messages = new Dictionary<string, string[]>();

            var okpo = unit is StatisticalUnit statisticalUnit
                ? statisticalUnit.StatId
                : ((EnterpriseGroup) unit).StatId;

            if (string.IsNullOrEmpty(okpo)) return messages;

            if (okpo.Any(x => !char.IsDigit(x)))
            {
                messages.Add(nameof(unit.StatId), new[] {nameof(Resource.AnalysisCalculationsStatIdOnlyNumber)});
                return messages;
            }

            if (_validationSettings.ValidateStatIdChecksum)
            {
                if (okpo.Length < 8)
                {
                    okpo = okpo.PadLeft(8, '0');
                }

                var okpoWithoutCheck = okpo.Substring(0, okpo.Length - 1);
                var checkNumber = Convert.ToInt32(okpo.Last().ToString());

                var sum = okpoWithoutCheck.Select((s, i) => Convert.ToInt32(s.ToString()) * (i % 10 + 1)).Sum();
                var remainder = sum % 11;
                if (remainder >= 10)
                {
                    sum = okpoWithoutCheck.Select((s, i) => Convert.ToInt32(s.ToString()) * (i % 10 + 3)).Sum();
                    remainder = sum % 11;
                }

                if (!(remainder == checkNumber || remainder == 10))
                    messages.Add(nameof(unit.StatId), new[] { nameof(Resource.AnalysisCalculationsStatId) });
            }
            
            return messages;
        }

        /// <inheritdoc />
        /// <summary>
        /// Check statistical unit duplicates
        /// </summary>
        /// <param name="unit">Stat unit</param>
        /// <param name="units">Duplicate units</param>
        /// <returns>Dictionary of messages</returns>
        public Dictionary<string, string[]> CheckDuplicates(IStatisticalUnit unit, List<IStatisticalUnit> units)
        {
            var manager = unit is StatisticalUnit statisticalUnit
                ? new StatisticalUnitDuplicatesManager(statisticalUnit, _analysisRules, units) as IAnalysisManager
                : new EnterpriseGroupDuplicatesManager(unit as EnterpriseGroup, _analysisRules, units);

            return manager.CheckFields();
        }
        
        /// <summary>
        /// Check statistical unit for orphanness
        /// </summary>
        /// <param name="unit">Stat unit</param>
        /// <returns>Dictionary of messages</returns>
        public Dictionary<string, string[]> CheckOrphanUnits(EnterpriseUnit unit)
        {
            var messages = new Dictionary<string, string[]>();

            if (_analysisRules.Orphan.CheckRelatedEnterpriseGroup && unit.EntGroupId == null)
                messages.Add(nameof(EnterpriseUnit.EntGroupId),
                    new[] { nameof(Resource.AnalysisOrphanEnterprise)});

            return messages;
        }

        /// <inheritdoc />
        /// <summary>
        /// Analyze statistical unit
        /// </summary>
        /// <param name="unit">Stat unit</param>
        /// <returns>Dictionary of messages</returns>
        public virtual AnalysisResult CheckAll(IStatisticalUnit unit)
        {
            var messages = new Dictionary<string, string[]>();
            var summaryMessages = new List<string>();

            var connectionsResult = CheckConnections(unit);
            if (connectionsResult.Any())
            {
                summaryMessages.Add(nameof(Resource.ConnectionRulesWarnings));
                messages.AddRange(connectionsResult);
            }

            var mandatoryFieldsResult = CheckMandatoryFields(unit);
            if (mandatoryFieldsResult.Any())
            {
                summaryMessages.Add(nameof(Resource.MandatoryFieldsRulesWarnings));
                messages.AddRange(mandatoryFieldsResult);
            }

            var calculationFieldsResult = CheckCalculationFields(unit);
            if (calculationFieldsResult.Any())
            {
                summaryMessages.Add(nameof(Resource.CalculationFieldsRulesWarnings));
                calculationFieldsResult.ForEach(d =>
                {
                    if (messages.ContainsKey(d.Key))
                    {
                        var existed = messages[d.Key];
                        messages[d.Key] = existed.Concat(d.Value).ToArray();
                    }
                    else
                        messages.Add(d.Key, d.Value);
                });
            }

            var potentialDuplicateUnits = GetDuplicateUnits(unit);
            if (potentialDuplicateUnits.Any())
            {
                var duplicatesResult = CheckDuplicates(unit, potentialDuplicateUnits);
                if (duplicatesResult.Any())
                {
                    summaryMessages.Add(nameof(Resource.DuplicateFieldsRulesWarnings));

                    duplicatesResult.ForEach(d =>
                    {
                        if (messages.ContainsKey(d.Key))
                        {
                            var existed = messages[d.Key];
                            messages[d.Key] = existed.Concat(d.Value).ToArray();
                        }
                        else
                            messages.Add(d.Key, d.Value);
                    });
                }
            }

            var additionalAnalysisCheckResult = CheckCustomAnalysisChecks(unit);
            if (additionalAnalysisCheckResult.Any())
            {
                summaryMessages.Add(nameof(Resource.CustomAnalysisChecks));
                additionalAnalysisCheckResult.ForEach(d =>
                {
                    if (messages.ContainsKey(d.Key))
                    {
                        var existed = messages[d.Key];
                        messages[d.Key] = existed.Concat(d.Value).ToArray();
                    }
                    else
                        messages.Add(d.Key, d.Value);
                });
            }

            if (unit is EnterpriseUnit)
            {
                var ophanUnitsResult = CheckOrphanUnits((EnterpriseUnit) unit);
                if (ophanUnitsResult.Any())
                {
                    summaryMessages.Add(nameof(Resource.OrphanUnitsRulesWarnings));
                    messages.AddRange(ophanUnitsResult);
                }
            }

            return new AnalysisResult
            {
                Name = unit.Name,
                Type = unit.UnitType,
                Messages = messages,
                SummaryMessages = summaryMessages
            };
        }

        /// <summary>
        /// Checks all custom analysis rules
        /// </summary>
        /// <param name="unit">unit to analyze</param>
        /// <returns></returns>
        public Dictionary<string, string[]> CheckCustomAnalysisChecks(IStatisticalUnit unit)
        {
            return _analysisRules.CustomAnalysisChecks
                ? new StatUnitCustomCheckManager(unit, _context).CheckFields()
                : new Dictionary<string, string[]>();
        }


        /// <summary>
        /// Get statistical unit duplicates
        /// </summary>
        /// <param name="unit">Stat unit</param>
        /// <returns>List of duplicates</returns>
        private List<IStatisticalUnit> GetDuplicateUnits(IStatisticalUnit unit)
        {
            List<IStatisticalUnit> result;
            if (unit is EnterpriseGroup enterpriseGroup)
            {
                var egPredicateBuilder = new AnalysisPredicateBuilder<EnterpriseGroup>();
                var egPredicate = egPredicateBuilder.GetPredicate(enterpriseGroup);

                var enterpriseGroups = _context.EnterpriseGroups.Where(egPredicate).Select(x => (IStatisticalUnit) x).ToList();
                result = enterpriseGroups;
            }
            else
            {
                var suPredicateBuilder = new AnalysisPredicateBuilder<StatisticalUnit>();
                var suPredicate = suPredicateBuilder.GetPredicate((StatisticalUnit)unit);

                var units = _context.StatisticalUnits.Include(x => x.PersonsUnits).Where(suPredicate).Select(x => (IStatisticalUnit) x).ToList();
                result = units;
            }

            return result;
        }
    }
}
