namespace DataParser;

public class SourceCovidData
{
    public string region { get; set; } = "";
    public int confirmed { get; set; }
    public int deaths { get; set; }
    public int recovered { get; set; }
    public int active { get; set; }
    public double incident_rate { get; set; }
    public double case_fatality_ratio { get; set; }
}