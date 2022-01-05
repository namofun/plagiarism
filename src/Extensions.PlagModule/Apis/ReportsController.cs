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
        /// <param name="justification">The status of report. -1 if claimed, 1 if ignored, 0 if unspecified</param>
        /// <param name="toggleShared">True to toggle shareness</param>
        /// <response code="200">Returns nothing</response>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchOne(
            [FromRoute, Required] string id,
            [FromForm] int? justification = null,
            [FromForm] bool? toggleShared = null)
        {
            Dictionary<string, string> updatedProps = new();
            if (justification == 0 || justification == 1 || justification == -1)
            {
                updatedProps.Add(nameof(justification), justification.ToString());
                await Store.JustificateAsync(id, justification == 0 ? default(bool?) : justification.Value < 0);
            }

            if (toggleShared == true)
            {
                updatedProps.Add(nameof(toggleShared), "true");
                await Store.ToggleReportSharenessAsync(id);
            }

            return Ok(updatedProps);
        }
    }
}
