using CsvHelper;
using System;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;


public class Program
{
    public static async Task<int> Main(params string[] args)
    {
        if (args.Length == 2)
        {
            return await ConvertCsvToSqlite(args[0], args[1]);
        }

        Console.WriteLine("Usage: DataParser <input csv directory> <output sqlite file>");
        return 1;
    }
    private static Regex dateMatcher = new Regex("(?<month>\\d{2})-(?<date>\\d{2})-(?<year>\\d{4})");
    /// <summary>
    /// Converts directory of csv files to sqlite database
    /// </summary>
    /// <param name="input">The path to the csv files that is to be converted.</param>
    /// <param name="output">The target name of the output file after conversion.</param>
    private static async Task<int> ConvertCsvToSqlite(string input, string output)
    {
        Console.WriteLine($"Converting CSV at {input} to {output}");
        var data = await ReadDataDirectory(input);
        await WriteDataToSqlite(data, output);
        return 0;
    }

    public static Task<SortedDictionary<DateTime, List<SourceCovidData>>> ReadDataDirectory(string path)
    {
        IEnumerable<string> files = Directory.EnumerateFiles(path, "*.csv");
        SortedDictionary<DateTime, List<SourceCovidData>> data = new SortedDictionary<DateTime, List<SourceCovidData>>();
        Parallel.ForEach(files, async file =>
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            List<SourceCovidData> records = await ReadDataFromCsvFile(file); ;
            MatchCollection matches = dateMatcher.Matches(fileName);
            if (matches.Count == 1)
            {
                string month = matches[0].Groups["month"].Value;
                string date = matches[0].Groups["date"].Value;
                string year = matches[0].Groups["year"].Value;
                string dateStr = $"{year}-{month}-{date}";
                DateTime dateValue = DateTime.Parse(dateStr);
                data.Add(dateValue, records);
            }
            else
            {
                Console.WriteLine($"Unable to parse date from file name {fileName}");
            }
            Console.WriteLine($"Successfully parsed {records.Count} records from {fileName}");
        });
        return Task.FromResult(data);
    }

    public static Task<int> WriteDataToSqlite(SortedDictionary<DateTime, List<SourceCovidData>> data, string path)
    {
        Console.WriteLine($"Writing data to {path}");
        using (var connection = new SqliteConnection($"Data Source={path}"))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS covid_data (
                        date TEXT,
                        region TEXT,
                        confirmed INTEGER,
                        deaths INTEGER,
                        recovered INTEGER,
                        active INTEGER,
                        incident_rate REAL,
                        case_fatality_ratio REAL,
                        PRIMARY KEY (date, region))";
                    command.ExecuteNonQuery();
                }
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM covid_data";
                    command.ExecuteNonQuery();
                }
                using (var command = connection.CreateCommand())
                {

                    command.CommandText = @"INSERT INTO covid_data
                        (date, region, confirmed, deaths, recovered, active, incident_rate, case_fatality_ratio) VALUES
                        (@date, @region, @confirmed, @deaths, @recovered, @active, @incident_rate, @case_fatality_ratio)";
                    command.Parameters.Add("@date", SqliteType.Text);
                    command.Parameters.Add("@region", SqliteType.Text);
                    command.Parameters.Add("@cases", SqliteType.Integer);
                    command.Parameters.Add("@deaths", SqliteType.Integer);
                    command.Parameters.Add("@confirmed", SqliteType.Integer);
                    command.Parameters.Add("@recovered", SqliteType.Integer);
                    command.Parameters.Add("@active", SqliteType.Integer);
                    command.Parameters.Add("@incident_rate", SqliteType.Real);
                    command.Parameters.Add("@case_fatality_ratio", SqliteType.Real);
                    foreach (var date in data.Keys)
                    {
                        foreach (var record in data[date])
                        {
                            command.Parameters["@date"].Value = date.ToString("yyyy-MM-dd");
                            command.Parameters["@region"].Value = record.region;
                            command.Parameters["@confirmed"].Value = record.confirmed;
                            command.Parameters["@deaths"].Value = record.deaths;
                            command.Parameters["@recovered"].Value = record.recovered;
                            command.Parameters["@active"].Value = record.active;
                            command.Parameters["@incident_rate"].Value = record.incident_rate;
                            command.Parameters["@case_fatality_ratio"].Value = record.case_fatality_ratio;
                            command.ExecuteNonQuery();
                        }
                    }
                }
                transaction.Commit();
            }
        }
        return Task.FromResult(0);
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
                } else if (record.confirmed > 0) {
                    // Estimated formula
                    record.case_fatality_ratio = record.deaths / (double)record.confirmed * 100.0;
                } else {
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
            record.case_fatality_ratio = group.Average(d => d.case_fatality_ratio);
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

public class SourceCovidData
{
    public string region { get; set; } = "";
    public int confirmed { get; set; }
    public int deaths { get; set; }
    public int recovered { get; set; }
    public int active { get; set; }
    public double incident_rate { get; set; }
    public double case_fatality_ratio {get; set; }
}