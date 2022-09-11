namespace APIServer.Controllers;
using APIServer.Models;
using Microsoft.AspNetCore.Mvc;


/// <summary>
/// DailyCaseSummaryController
/// </summary>
[Route("api/daily_case_summary")]
public class DailyCaseSummaryController : Controller
{
    [HttpGet("")]
    [Consumes( "application/json" )]
    [ProducesResponseType(typeof(DailyCaseSummary), 200)]
    public DailyCaseSummary index([FromQuery] string start, [FromQuery] string end, [FromQuery] string region)
    {
        var result = new DailyCaseSummary();
        result.date = "2020-01-01";
        result.region = "US";
        result.confirmed = 2;
        result.deaths = 1;
        result.active = 1;
        result.incident_rate = 1.0;
        return result;
    }
}