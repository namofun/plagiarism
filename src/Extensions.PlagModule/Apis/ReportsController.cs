using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SatelliteSite.PlagModule.Apis
{
    /// <summary>
    /// The report API
    /// </summary>
    [Area("Api")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/plagiarism/[controller]")]
    [Produces("application/json")]
    [CustomedExceptionFilter]
    public class ReportsController : ApiControllerBase
    {
        public IPlagiarismDetectService Store { get; }

        public ReportsController(IPlagiarismDetectService store)
        {
            Store = store;
        }


        /// <summary>
        /// Get the given report for the plagiarism system
        /// </summary>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given report for the plagiarism system</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetOne(
            [FromRoute, Required] string id)
        {
            return await Store.FindReportAsync(id);
        }


        /// <summary>
        /// Get the given report for the plagiarism system
        /// </summary>
        /// <param name="id">The ID of the entity to patch</param>
        /// <param name="justification">The status of report. True if claimed, false if ignored, null if unspecified</param>
        /// <response code="200">Returns nothing</response>
        [HttpPatch("{id}")]
        public async Task<IActionResult> JustificateOne(
            [FromRoute, Required] string id,
            [FromForm] bool? justification = null)
        {
            await Store.JustificateAsync(id, justification);
            return Ok(Store.GetVersion());
        }
    }
}
