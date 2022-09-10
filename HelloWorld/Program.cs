﻿using CsvHelper;
using System;
using System.Text.RegularExpressions;

IEnumerable<string> files = Directory.EnumerateFiles("../data_source/csse_covid_19_data/csse_covid_19_daily_reports", "*.csv");
Regex dateMatcher = new Regex("(?<month>\\d{2})-(?<date>\\d{2})-(?<year>\\d{4})");

foreach (var file in files)
{
    
    string fileName = Path.GetFileNameWithoutExtension(file);
    List<SourceCovidData> records = ReadDataFromCsv(file);;
    MatchCollection matches = dateMatcher.Matches(fileName);
    if (matches.Count == 1)
    {
        string month = matches[0].Groups["month"].Value;
        string date = matches[0].Groups["date"].Value;
        string year = matches[0].Groups["year"].Value;
        string dateStr = $"{year}-{month}-{date}";
        DateTime dateValue = DateTime.Parse(dateStr);
        foreach (var record in records)
        {
            record.date = dateValue;
        }
    }
    else
    {
        Console.WriteLine($"Unable to parse date from file name {fileName}");
    }
    Console.WriteLine($"Successfully parsed {records.Count} records from {fileName}");
}


List<SourceCovidData> ReadDataFromCsv(string path)
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
    return records;
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

    public DateTime date { get; set; }
}