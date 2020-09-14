using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using nscreg.Business.DataSources;
using Xunit;

namespace nscreg.Business.Test.DataSources
{
    public class XmlParserTest
    {
        [Fact]
        private void ParseRawEntityTest()
        {
            var xdoc = XDocument.Parse(
                "<TaxPayer>"
                + "<NscCode>21878385</NscCode>"
                + "<Tin>21904196910047</Tin>"
                + "<AddressObl>Чуйская обл., Аламудунский р-н, Степное</AddressObl>"
                + "<AddressStreet>ул.Артезиан 23</AddressStreet>"
                + "<FullName>Балабеков Ибрахим Хакиевич</FullName>"
                + "<CoateCode>41708203859030</CoateCode>"
                + "<AwardingSolutionDate>2016-08-10T00:00:00+06:00</AwardingSolutionDate>"
                + "<LiquidationReasonCode>003</LiquidationReasonCode>"
                + "<OSNOVAN>Реш.суда -неплатежесп.(банкротства)</OSNOVAN>" +
                "</TaxPayer>"
            );

            var actual = XmlParser.ParseRawEntity(xdoc.Root, null);

            Assert.Equal(9, actual.Count);
            Assert.Equal("21878385", actual["NscCode"]);
            Assert.Equal("21904196910047", actual["Tin"]);
            Assert.Equal("Балабеков Ибрахим Хакиевич", actual["FullName"]);
        }

        [Fact]
        private void GetRawEntitiesTest()
        {
            var xdoc = XDocument.Parse(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<GetTaxPayersLiquidatedResult xmlns=\"http://sti.gov.kg/\">" +
                "<LegalUnit>"
                + "<NscCode>21878385</NscCode>"
                + "<Tin>21904196910047</Tin>"
                + "<AddressObl>Чуйская обл., Аламудунский р-н, Степное</AddressObl>"
                + "<AddressStreet>ул.Артезиан 23</AddressStreet>"
                + "<FullName>Балабеков Ибрахим Хакиевич</FullName>"
                + "<CoateCode>41708203859030</CoateCode>"
                + "<AwardingSolutionDate>2016-08-10T00:00:00+06:00</AwardingSolutionDate>"
                + "<LiquidationReasonCode>003</LiquidationReasonCode>"
                + "<OSNOVAN>Реш.суда -неплатежесп.(банкротства)</OSNOVAN>" +
                "</LegalUnit>" +
                "<LegalUnit>"
                + "<NscCode>22987099</NscCode>"
                + "<Tin>10510196100229</Tin>"
                + "<AddressObl>Таласская обл., Карабуринский р-н, Кызыл-адыр</AddressObl>"
                + "<AddressStreet>ул. Аларча 19</AddressStreet>"
                + "<FullName>Искендерова Алтынкул Убайдылдаевна</FullName>"
                + "<CoateCode>41707215818010</CoateCode>"
                + "<AwardingSolutionDate>2016-09-06T00:00:00+06:00</AwardingSolutionDate>"
                + "<LiquidationReasonCode>001</LiquidationReasonCode>"
                + "<OSNOVAN>Реш,собств-ка/органа, уполн-го собс-ком</OSNOVAN>" +
                "</LegalUnit>" +
                "</GetTaxPayersLiquidatedResult>"
            );

            var actual = XmlParser.GetRawEntities(xdoc).ToArray();

            Assert.Equal(2, actual.Length);
            Assert.Equal("LegalUnit", actual[0].Name.LocalName);
            Assert.Equal(9, actual[0].Descendants().Count());

            // two xml-related methods integration test - temporary solution, while other logic is unclear
            var entitiesDictionary = actual.Select(x => XmlParser.ParseRawEntity(x ,null)).ToArray();
            Assert.Equal(2, entitiesDictionary.Length);
            Assert.True(entitiesDictionary.All(dict => dict.Count == 9));
        }

        [Fact]
        private void ShouldParseEntityWithActivityTest()
        {
            var xdoc = XDocument.Parse(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<GetLegalUnits xmlns=\"http://sti.gov.kg/\">" +
                    "<LegalUnit>" +
                        "<StatId>920951287</StatId>" +
                        "<Name>LAST FRIDAY INVEST AS</Name>" +
                        "<Activities> " +
                            "<Activity>" +
                                "<ActivityYear>2019</ActivityYear>" +
                                "<CategoryCode>62.020</CategoryCode>" +
                                 "<Employees>100</Employees>" +
                            "</Activity>" +
                            "<Activity>" +
                                 "<ActivityYear>2018</ActivityYear>" +
                                 "<CategoryCode>70.220</CategoryCode>" +
                                 "<Employees>20</Employees>" +
                            "</Activity>" +
                            "<Activity>" +
                                "<CategoryCode>52.292</CategoryCode>" +
                                "<Employees>10</Employees>" +
                            "</Activity>" +
                            "<Activity>" +
                                "<CategoryCode>68.209</CategoryCode>" +
                            "</Activity>" +
                        "</Activities>" +
                    "</LegalUnit>" +
                "</GetLegalUnits>");
            var mappings =
                "StatId-StatId,Name-Name,Activities.Activity.ActivityYear-Activities.Activity.ActivityYear,Activities.Activity.CategoryCode- Activities.Activity.ActivityCategory.Code,Activities.Activity.Employees-Activities.Activity.Employees";
            var array = mappings.Split(',').Select(vm =>
            {
                var pair = vm.Split('-');
                return (pair[0], pair[1]);
            }).ToArray();

            var actual = XmlParser.GetRawEntities(xdoc);
            var entitiesDictionary = actual.Select(x => XmlParser.ParseRawEntity(x, array)).ToArray();
            entitiesDictionary.Should().BeEquivalentTo(new List<IReadOnlyDictionary<string, object>>
            {
                new Dictionary<string, object>()
                {
                    { "StatId", "920951287"},
                    { "Name", "LAST FRIDAY INVEST AS" },
                    { "Activities", new List<KeyValuePair<string, Dictionary<string, string>>>()
                    {
                        new KeyValuePair<string, Dictionary<string, string>>("Activity", new Dictionary<string, string>()
                        {
                            {"ActivityCategory.Code", "62.020"},
                            {"Employees","100"},
                            {"ActivityYear","2019"}
                        }),
                        new KeyValuePair<string, Dictionary<string, string>>("Activity", new Dictionary<string, string>()
                        {
                            {"CategoryCode", "70.220"},
                            {"Employees","20"},
                            {"ActivityYear","2018"}
                        }),
                        new KeyValuePair<string, Dictionary<string, string>>("Activity", new Dictionary<string, string>()
                        {
                            {"CategoryCode", "52.292"},
                            {"Employees","10"}
                        }),
                        new KeyValuePair<string, Dictionary<string, string>>("Activity", new Dictionary<string, string>()
                        {
                            {"CategoryCode", "68.209"},
                        })
                    }}
                }
            });
        }
    }
}
