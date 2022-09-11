namespace APIServer.Models;
/// <summary>
/// DailyCaseSummary
/// </summary>
public class DailyCaseSummary
{
    /// <summary>
    /// Date YYYY-MM-DD
    /// </summary>
    public string date { get; set; } = "";
    /// <summary>
    /// Total number of confirmed cases
    /// </summary>
    public string region { get; set; } = "";
    /// <summary>
    /// Total number of confirmed cases
    /// </summary>
    public int confirmed { get; set; }
    /// <summary>
    /// Total number of deaths
    /// </summary>
    public int deaths { get; set; }
    /// <summary>
    /// Total number of recovered cases
    /// </summary>
    public int recovered { get; set; }
    /// <summary>
    /// Total number of active cases
    /// </summary>
    public int active { get; set; }

    /// <summary>
    /// incident rate per 100000 people
    /// </summary>
    public double incident_rate { get; set; }
    /// <summary>
    /// Case Fatality Rate
    /// </summary>
    public double case_fatality_ratio { get; set; }
}