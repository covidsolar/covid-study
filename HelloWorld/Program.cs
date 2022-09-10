using CsvHelper;
using System;
using System.Text.RegularExpressions;

IEnumerable<string> files = Directory.EnumerateFiles("../data_source/csse_covid_19_data/csse_covid_19_daily_reports", "*.csv");
Regex dateMatcher = new Regex("(?<month>\\d{2})-(?<date>\\d{2})-(?<year>\\d{4})");
Dictionary<DateTime, List<SourceCovidData>> data = new Dictionary<DateTime, List<SourceCovidData>>();
Parallel.ForEach(files,async file => {
    string fileName = Path.GetFileNameWithoutExtension(file);
    Console.WriteLine($"Parsing {fileName}");
    List<SourceCovidData> records = await ReadDataFromCsv(file);;
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



async Task<List<SourceCovidData>> ReadDataFromCsv(string path)
{
    var records = new List<SourceCovidData>();
    using (var reader = new StreamReader(path))
    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
    {
        csv.Read();
        csv.ReadHeader();

        bool isOldFormat = Array.Exists(csv.HeaderRecord, element => element == "Country/Region");
        string incidentRateHeader = Array.Find(csv.HeaderRecord, str => str.StartsWith("Incid") && str.EndsWith("Rate"));
        while (csv.Read())
        {
            var record = new SourceCovidData();
            record.confirmed = ReadInt(csv.GetField("Confirmed"));
            record.deaths = ReadInt(csv.GetField("Deaths"));
            record.recovered = ReadInt(csv.GetField("Recovered"));
            if (incidentRateHeader != null) {
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
            records.Add(record);
        }
    }
    return aggregateData(records);
}

List<SourceCovidData> aggregateData(List<SourceCovidData> data)
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
        aggregatedData.Add(record);
    }
    return aggregatedData;
}

// Read an integer from a string, returning 0 if the string is empty
int ReadInt(string value) {
    int result = 0;
    try {
        result = Int32.Parse(value);
    } catch (Exception e) {
        result = 0;
    }
    return result;
}

double ReadDouble(string value) {
    double result = 0;
    try {
        result = Double.Parse(value);
    } catch (Exception e) {
        result = 0;
    }
    return result;
}

public class SourceCovidData
{
    public string region { get; set; }
    public int confirmed { get; set; }
    public int deaths { get; set; }
    public int recovered { get; set; }
    public int active { get; set; }
    public double incident_rate { get; set; }
}