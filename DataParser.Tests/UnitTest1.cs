namespace DataParser.Tests;
using DataParser;
using System;

public class UnitTest1
{
    [Fact]
    public void DateParsingCorrectly()
    {
        var date = DataParser.Parser.ParseDateFromFileName("03-01-2020");
        Assert.Equal(new DateTime(2020, 3, 1), date);
    }

    [Fact]
    public void DateParsingThrowsException()
    {
        Assert.Throws<ArgumentException>(() => DataParser.Parser.ParseDateFromFileName("1-03-01-2020"));
    }

    [Fact]
    public void ParseHongKongFrom2020Format()
    {
        Console.Error.WriteLine(Environment.CurrentDirectory);
        var records = DataParser.Parser.ReadDataFromCsvFile("../../../fixtures/format-2020-china.csv").Result;
        Assert.Equal(2, records.Count);
        Assert.Equal("China", records[0].region);
        Assert.Equal(62662 + 1333, records[0].confirmed);
        Assert.Equal(3.3580748495976245, records[0].case_fatality_ratio);
        Assert.Equal("Hong Kong", records[1].region);
        Assert.Equal(68, records[1].confirmed);
    }

    [Fact]
    public void ParseHongKongFrom2022Format()
    {
        Console.Error.WriteLine(Environment.CurrentDirectory);
        var records = DataParser.Parser.ReadDataFromCsvFile("../../../fixtures/format-2022-china.csv").Result;
        Assert.Equal(2, records.Count);
        Assert.Equal("Hong Kong", records[1].region);
        Assert.Equal(1.6815346964553564, records[1].case_fatality_ratio);
        Assert.Equal(168.961188146493, records[1].incident_rate);
        Assert.Equal(213, records[1].deaths);
        Assert.Equal(12667, records[1].confirmed);
        Assert.Equal(0, records[1].recovered);
        Assert.Equal(0, records[1].active);
    }

    [Fact]
    public async void ParseDirectoryToData()
    {
        var data = await DataParser.Parser.ReadDataDirectory("../../../fixtures/data-directory");
        Assert.Equal(3, data.Count);
    }
}