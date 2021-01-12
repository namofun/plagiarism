using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Entities;
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
    public class ReportsController : ApiControllerBase
    {
        public IStoreService Store { get; }

        public ReportsController(IStoreService store)
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
    }
}
