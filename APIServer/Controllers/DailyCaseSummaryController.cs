namespace APIServer.Controllers;
using APIServer.Models;
using APIServer.Data;
using Microsoft.AspNetCore.Mvc;



/// <summary>
/// DailyCaseSummaryController
/// </summary>
[Route("api/daily_case_summary")]
public class DailyCaseSummaryController : Controller
{
    private readonly ApplicationDbContext _context;
    public DailyCaseSummaryController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Get all daily case summaries
    /// </summary>
    /// <returns></returns>

    [HttpGet("")]
    [Consumes( "application/json" )]
    [ProducesResponseType(typeof(List<DailyCaseSummary>), 200)]
    public List<DailyCaseSummary> GetDailyCaseSummaries([FromQuery] string start, [FromQuery] string end, [FromQuery] string region)
    {
        if (start == null || end == null)
        {
            return new List<DailyCaseSummary>();
            // TODO: Figure out how to properly return a 400 error
            // throw new HttpResponseException(400, "start and end are required");
        }
        var cases = from data in _context.DailyCaseSummaries
                    where data.date.CompareTo(start) >= 0
                    where data.date.CompareTo(end) <= 0
                    select data;
        if (region != null) {
            cases = cases.Where(d => d.region == region);
        }
        return cases.ToList();
    }
}