using Newtonsoft.Json;
using nscreg.Data.Entities;
using nscreg.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using nscreg.Data.Constants;
using static nscreg.Utilities.JsonPathHelper;

namespace nscreg.Business.DataSources
{
    public static class StatUnitKeyValueParser
    {
        public static readonly string[] StatisticalUnitArrayPropertyNames = { nameof(StatisticalUnit.Activities), nameof(StatisticalUnit.Persons), nameof(StatisticalUnit.ForeignParticipationCountriesUnits) };

        public static string GetStatIdSourceKey(IEnumerable<(string source, string target)> mapping)
            => mapping.FirstOrDefault(vm => vm.target == nameof(StatisticalUnit.StatId)).target;

        public static void ParseAndMutateStatUnit(
            IReadOnlyDictionary<string, object> nextProps,
            StatisticalUnit unit)
        {
            foreach (var kv in nextProps)
            {

                if (kv.Value is string)
                {
                    try
                    {
                        UpdateObject(kv.Key, kv.Value);
                    }
                    catch (Exception ex)
                    {
                        ex.Data.Add("target property", kv.Key);
                        ex.Data.Add("value", kv.Value);
                        ex.Data.Add("unit", unit);
                        throw;
                    }
                }
                else if (kv.Value is List<KeyValuePair<string, Dictionary<string, string>>> arrayProperty)
                {
                    var targetArrKeys = arrayProperty.SelectMany(x=>x.Value.Select(d=>d.Key)).Distinct();
                    var mapping = targetArrKeys.ToDictionary(x => x, x => new string[] { x });
                    try
                    {
                        UpdateObject(kv.Key, kv.Value, mapping);
                    }
                    catch (Exception ex)
                    {
                        ex.Data.Add("source property", kv.Key);
                        ex.Data.Add("target property", targetArrKeys);
                        ex.Data.Add("value", kv.Value);
                        ex.Data.Add("unit", unit);
                        throw;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Fail("Bad sector of code. NextProps: " + JsonConvert.SerializeObject(nextProps));
                }
            }

            void UpdateObject(string propPath, object inputValue,
                Dictionary<string, string[]> mappingsArr = null)
            {
                var propHead = PathHead(propPath);
                var propTail = PathTail(propPath);
                var unitType = unit.GetType();
                var propInfo = unitType.GetProperty(propHead);
                if (propInfo == null)
                {
                    throw new Exception(
                        $"Property `{propHead}` not found in type `{unitType}`,"
                        + $" property path: `{propPath}`, value: `{inputValue}`");
                }
                object propValue;
                string value = "";
                List<KeyValuePair<string, Dictionary<string, string>>> valueArr = null;
                if (inputValue is string s)
                {
                    value = s;
                }
                else
                {
                    valueArr = inputValue as List<KeyValuePair<string, Dictionary<string, string>>>;
                }
                switch (propHead)
                {
                    case nameof(StatisticalUnit.Activities):
                        propInfo = unit.GetType().GetProperty(nameof(StatisticalUnit.ActivitiesUnits));
                        var unitActivities = unit.ActivitiesUnits ?? new List<ActivityStatisticalUnit>();
                        if (valueArr != null)
                            foreach (var activityFromArray in valueArr)
                            {
                                UpdateCollectionProperty(unitActivities, activityFromArray.Value, mappingsArr);
                            }
                        propValue = unitActivities;
                        break;
                    case nameof(StatisticalUnit.Persons):
                        propInfo = unit.GetType().GetProperty(nameof(StatisticalUnit.PersonsUnits));
                        var persons = unit.PersonsUnits ?? new List<PersonStatisticalUnit>();
                        var tmpPropValue = new List<PersonStatisticalUnit>();
                        if (valueArr != null)
                            foreach (var personFromArray in valueArr)
                            {
                                foreach (var personValue in personFromArray.Value)
                                {
                                    //TODO NEED UPDATE
                                    if (!mappingsArr.TryGetValue(personValue.Key, out string[] targetKeys)) continue;
                                    foreach (var targetKey in targetKeys)
                                    {
                                        UpdateCollectionProperty(persons, targetKey, personValue.Value);

                                    }
                                }
                                tmpPropValue.AddRange(persons);
                                persons.Clear();
                            }
                        propValue = persons;
                        break;
                    case nameof(StatisticalUnit.ForeignParticipationCountriesUnits):
                        var fpcPropValue = new List<CountryStatisticalUnit>();
                        var foreignParticipationCountries = unit.ForeignParticipationCountriesUnits ?? new List<CountryStatisticalUnit>();
                        if (valueArr != null)
                            foreach (var countryFromArray in valueArr)
                            {
                                Country prev = new Country();
                                foreach (var countryValue in countryFromArray.Value)
                                {
                                    if (!mappingsArr.TryGetValue(countryValue.Key, out string[] targetKeys)) continue;
                                    foreach (var targetKey in targetKeys)
                                    {
                                        PropertyParser.ParseCountry(targetKey, countryValue.Value, prev);
                                    }
                                }
                                foreignParticipationCountries.Add(new CountryStatisticalUnit()
                                {
                                    CountryId = prev.Id,
                                    Country = prev
                                });
                                fpcPropValue.AddRange(foreignParticipationCountries);
                            }
                        propValue = fpcPropValue;
                        break;
                    case nameof(StatisticalUnit.Address):
                        propValue = PropertyParser.ParseAddress(propTail, value, unit.Address);
                        break;
                    case nameof(StatisticalUnit.ActualAddress):
                        propValue = PropertyParser.ParseAddress(propTail, value, unit.ActualAddress);
                        break;
                    case nameof(StatisticalUnit.PostalAddress):
                        propValue = PropertyParser.ParseAddress(propTail, value, unit.PostalAddress);
                        break;
                    case nameof(StatisticalUnit.LegalForm):
                        propValue = PropertyParser.ParseLegalForm(propTail, value, unit.LegalForm);
                        break;
                    case nameof(StatisticalUnit.InstSectorCode):
                        propValue = PropertyParser.ParseSectorCode(propTail, value, unit.InstSectorCode);
                        break;
                    case nameof(StatisticalUnit.DataSourceClassification):
                        propValue = PropertyParser.ParseDataSourceClassification(propTail, value, unit.DataSourceClassification);
                        break;
                    case nameof(StatisticalUnit.Size):
                        propValue = PropertyParser.ParseSize(propTail, value, unit.Size);
                        break;
                    case nameof(StatisticalUnit.UnitStatus):
                        propValue = PropertyParser.ParseUnitStatus(propTail, value, unit.UnitStatus);
                        break;
                    case nameof(StatisticalUnit.ReorgType):
                        propValue = PropertyParser.ParseReorgType(propTail, value, unit.ReorgType);
                        break;
                    case nameof(StatisticalUnit.RegistrationReason):
                        propValue = PropertyParser.ParseRegistrationReason(propTail, value, unit.RegistrationReason);
                        break;
                    case nameof(StatisticalUnit.ForeignParticipation):
                        propValue = PropertyParser.ParseForeignParticipation(propTail, value, unit.ForeignParticipation);
                        break;
                    default:
                        var type = propInfo.PropertyType;
                        var underlyingType = Nullable.GetUnderlyingType(type);
                        propValue = value.HasValue() || underlyingType == null
                            ? Type.GetTypeCode(type) == TypeCode.String
                                ? value
                                : PropertyParser.ConvertOrDefault(underlyingType ?? type, value)
                            : null;
                        break;
                }

                propInfo?.SetValue(unit, propValue);
            }
        }
        private static void UpdateCollectionProperty(ICollection<ActivityStatisticalUnit> activities, Dictionary<string, string> targetKeys, Dictionary<string, string[]> mappingsArr)
        {
            // Todo Change Update
            var categoryCode = targetKeys.GetValueOrDefault(string.Join('.', nameof(ActivityCategory), nameof(ActivityCategory.Code)));
            var activityYear = targetKeys.GetValueOrDefault(nameof(Activity.ActivityYear));

            ActivityStatisticalUnit newJoin = null;

            if (categoryCode.HasValue() && activityYear.HasValue() && int.TryParse(activityYear, out var year))
            {
                newJoin = activities.FirstOrDefault(x =>
                    x.Activity?.ActivityCategory?.Code == categoryCode && x.Activity?.ActivityYear == year);
            }
            if (newJoin != null)
            {
                ParseActivity(newJoin.Activity, targetKeys, mappingsArr);
            }
            else
            {
                newJoin = new ActivityStatisticalUnit();
                ParseActivity(newJoin.Activity, targetKeys, mappingsArr);
                activities.Add(newJoin);
            }
        }

        private static void ParseActivity(Activity activity, Dictionary<string, string> targetKeys,
            Dictionary<string, string[]> mappingsArr)
        {
            foreach (var (key, val) in targetKeys)
            {
                if (!mappingsArr.TryGetValue(key, out var targetValues)) continue;
                foreach (var targetKey in targetValues)
                {
                    activity = PropertyParser.ParseActivity(targetKey, val, activity);
                }
            }
        }

        private static void UpdateCollectionProperty(ICollection<PersonStatisticalUnit> persons, string targetKey, string value)
        {
            var newJoin = persons.LastOrDefault(x => x.PersonId.Value == 0 && x.Person != null) ?? new PersonStatisticalUnit();

            var isOwnProperty = PropertyParser.SetPersonStatUnitOwnProperties(targetKey, newJoin, value);

            if (!isOwnProperty)
                PropertyParser.ParsePerson(targetKey, value, newJoin.Person);

            var newPersons = new List<PersonStatisticalUnit>();
            foreach (var existsPerson in persons)
            {
                if (existsPerson.Person.Role == newJoin.Person.Role)
                {
                    existsPerson.Person = newJoin.Person;
                }
                else
                {
                    newPersons.Add(newJoin);
                }
            }
            persons.AddRange(newPersons);
        }

    }
}
