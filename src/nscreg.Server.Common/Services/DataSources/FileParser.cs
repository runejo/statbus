using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using nscreg.Business.DataSources;

namespace nscreg.Server.Common.Services.DataSources
{
    public static class FileParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns></returns>
        public static async Task<IEnumerable<IReadOnlyDictionary<string, object>>> GetRawEntitiesFromXml(
            string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return XmlParser.GetRawEntities(await XDocument.LoadAsync(fileStream, LoadOptions.None, CancellationToken.None)).Select(XmlParser.ParseRawEntity);
            }
        }

        public static async Task<IEnumerable<IReadOnlyDictionary<string, object>>> GetRawEntitiesFromCsv(
            string filePath, int skipCount, string delimiter)
        {
            var i = skipCount;
            string rawLines;
            using (var stream = File.OpenRead(filePath))
            using (var reader = new StreamReader(stream))
            {
                while (--i == 0) await reader.ReadLineAsync();
                rawLines = await reader.ReadToEndAsync();
            }

            return CsvParser.GetParsedEntities(rawLines, delimiter);
        }
    }
}
