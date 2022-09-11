using System;
using DataParser;


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

    /// <summary>
    /// Converts directory of csv files to sqlite database
    /// </summary>
    /// <param name="input">The path to the csv files that is to be converted.</param>
    /// <param name="output">The target name of the output file after conversion.</param>
    private static async Task<int> ConvertCsvToSqlite(string input, string output)
    {
        Console.WriteLine($"Converting CSV at {input} to {output}");
        var data = await Parser.ReadDataDirectory(input);
        await SqliteWriter.WriteDataToSqlite(data, output);
        return 0;
    }
}