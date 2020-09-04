using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Entities;
using Plag.Backend.Services;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.PlagModule.Apis
{
    /// <summary>
    /// The set API
    /// </summary>
    [Area("Api")]
    [Route("[area]/plag/[controller]")]
    [Produces("application/json")]
    public class SetsController : ApiControllerBase
    {
        public IStoreService Store { get; }

        public SetsController(IStoreService store)
        {
            Store = store;
        }

        /// <summary>
        /// Create a set for the plagiarism system
        /// </summary>
        /// <param name="name">The name of plagiarism set</param>
        /// <response code="201">Returns the created plagiarism set</response>
        [HttpPost]
        public async Task<ActionResult<PlagiarismSet>> CreateSetAsync(
            [FromBody, Required] string name)
        {
            return await Store.CreateSetAsync(name);
        }

        /// <summary>
        /// Get the given set for the plagiarism system
        /// </summary>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given set for the plagiarism system</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<PlagiarismSet>> FindSetAsync(
            [FromRoute, Required] string id)
        {
            return await Store.FindSetAsync(id);
        }

        /// <summary>
        /// Get page view of all the sets for the plagiarism system
        /// </summary>
        /// <param name="page">The page</param>
        /// <response code="200">Returns page view of all the sets for the plagiarism system</response>
        [HttpGet]
        public async Task<ActionResult<PlagiarismSet[]>> ListSetsAsync(
            [FromQuery] int page = 1)
        {
            var items = await Store.ListSetsAsync(page);
            Response.Headers.Add("X-Pagination", $"{items.CountPerPage};{items.TotalCount}");
            return items.ToArray();
        }
    }
}
