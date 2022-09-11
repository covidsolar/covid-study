using Microsoft.Data.Sqlite;

namespace DataParser
{
    public class SqliteWriter
    {
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
    }
}
