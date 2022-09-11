using CsvHelper;
using System;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace DataParser
{
    public class Parser
    {
        private static Regex dateMatcher = new Regex("^(?<month>\\d{2})-(?<date>\\d{2})-(?<year>\\d{4})$");
        
        public static DateTime ParseDateFromFileName(string fileName)
        {
            var match = dateMatcher.Match(fileName);
            if (match.Success)
            {
                return new DateTime(
                    int.Parse(match.Groups["year"].Value),
                    int.Parse(match.Groups["month"].Value),
                    int.Parse(match.Groups["date"].Value));
            }
            else
            {
                throw new ArgumentException("Invalid file name");
            }
        }

        public static Task<SortedDictionary<DateTime, List<SourceCovidData>>> ReadDataDirectory(string path)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(path, "*.csv");
            SortedDictionary<DateTime, List<SourceCovidData>> data = new SortedDictionary<DateTime, List<SourceCovidData>>();
            Parallel.ForEach(files, async file =>
            {

                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var records = await ReadDataFromCsvFile(file); ;
                    var date = ParseDateFromFileName(fileName);
                    data.Add(date, records);
                    Console.WriteLine($"Successfully parsed {records.Count} records from {fileName}");
                }
                catch
                {
                    Console.Error.WriteLine($"Unable to parse date from file name {file}");
                }
            });
            return Task.FromResult(data);
        }

        public static Task<List<SourceCovidData>> ReadDataFromCsvFile(string path)
        {
            var records = new List<SourceCovidData>();
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();

                if (csv.HeaderRecord is null)
                {
                    throw new Exception("Header record is null");
                }

                bool isOldFormat = Array.Exists(csv.HeaderRecord, element => element == "Country/Region");
                string? incidentRateHeader = Array.Find(csv.HeaderRecord, str => str.StartsWith("Incid") && str.EndsWith("Rate"));
                string? caseFatalityRatioHeader = Array.Find(csv.HeaderRecord, str => str.Contains("Fatality_Ratio"));
                string? provinceHeader = Array.Find(csv.HeaderRecord, str => str.StartsWith("Province"));

                while (csv.Read())
                {
                    var record = new SourceCovidData();
                    record.confirmed = ReadInt(csv.GetField("Confirmed"));
                    record.deaths = ReadInt(csv.GetField("Deaths"));
                    record.recovered = ReadInt(csv.GetField("Recovered"));
                    if (incidentRateHeader != null)
                    {
                        record.incident_rate = ReadDouble(csv.GetField(incidentRateHeader));
                    }
                    if (isOldFormat)
                    {
                        // Handle the 2020 format
                        record.region = csv.GetField("Country/Region");
                        record.active = record.confirmed - record.recovered - record.deaths;
                    }
                    else
                    {
                        // Handle the 2021 format
                        record.region = csv.GetField("Country_Region");
                        record.active = ReadInt(csv.GetField("Active"));
                    }
                    // Unify the reference to China
                    if (record.region == "Mainland China")
                    {
                        record.region = "China";
                    }
                    // Unify the reference to Hong Kong
                    if (provinceHeader != null && csv.GetField(provinceHeader) == "Hong Kong")
                    {
                        record.region = "Hong Kong";
                    }

                    // Fill the fatality ratio
                    if (caseFatalityRatioHeader != null)
                    {
                        record.case_fatality_ratio = ReadDouble(csv.GetField(caseFatalityRatioHeader));
                    }
                    else if (record.confirmed > 0)
                    {
                        // Estimated formula
                        record.case_fatality_ratio = record.deaths / (double)record.confirmed * 100.0;
                    }
                    else
                    {
                        record.case_fatality_ratio = 0;
                    }

                    records.Add(record);
                }
            }
            return Task.FromResult(aggregateData(records));
        }
        
        public static List<SourceCovidData> aggregateData(List<SourceCovidData> data)
        {
            var aggregatedData = new List<SourceCovidData>();
            var groupedData = data.GroupBy(d => d.region);
            foreach (var group in groupedData)
            {
                var record = new SourceCovidData();
                record.region = group.Key;
                record.confirmed = group.Sum(d => d.confirmed);
                record.deaths = group.Sum(d => d.deaths);
                record.recovered = group.Sum(d => d.recovered);
                record.active = group.Sum(d => d.active);
                record.incident_rate = group.Average(d => d.incident_rate);
                if (record.confirmed > 0)
                {
                    record.case_fatality_ratio = record.deaths / (double)record.confirmed * 100.0;
                }
                else
                {
                    record.case_fatality_ratio = 0;
                }
                aggregatedData.Add(record);
            }
            return aggregatedData;
        }

        private static int ReadInt(string value)
        {
            int result = 0;
            try
            {
                result = Int32.Parse(value);
            }
            catch
            {
                result = 0;
            }
            return result;
        }

        private static double ReadDouble(string value)
        {
            double result = 0;
            try
            {
                result = Double.Parse(value);
            }
            catch
            {
                result = 0;
            }
            return result;
        }
    }


};
