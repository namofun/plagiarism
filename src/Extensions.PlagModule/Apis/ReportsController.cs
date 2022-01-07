using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System.Collections.Generic;
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
        /// Patch the given report for the plagiarism system
        /// </summary>
        /// <param name="id">The ID of the entity to patch</param>
        /// <param name="justification">The status of report</param>
        /// <param name="shared">Whether report is shared</param>
        /// <response code="200">Returns nothing</response>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchOne(
            [FromRoute, Required] string id,
            [FromForm] ReportJustification? justification = null,
            [FromForm] bool? shared = null)
        {
            Dictionary<string, string> updatedProps = new();
            if (justification.HasValue)
            {
                updatedProps.Add(nameof(justification), justification.ToString());
                await Store.JustificateAsync(id, justification.Value);
            }

            if (shared.HasValue)
            {
                updatedProps.Add(nameof(shared), shared.Value.ToString().ToLower());
                await Store.ShareReportAsync(id, shared.Value);
            }

            return Ok(updatedProps);
        }
    }
}
