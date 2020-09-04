using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Entities;
using Plag.Backend.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SatelliteSite.PlagModule.Apis
{
    /// <summary>
    /// The submission API
    /// </summary>
    [Area("Api")]
    [Route("[area]/plag/[controller]")]
    [Produces("application/json")]
    public class SubmissionsController : ApiControllerBase
    {
        public IStoreService Store { get; }

        public SubmissionsController(IStoreService store)
        {
            Store = store;
        }

        /// <summary>
        /// Create a submission for the plagiarism system
        /// </summary>
        /// <param name="submission">The entity to create</param>
        /// <response code="201">Returns the created submission for the plagiarism system</response>
        [HttpPost]
        public async Task<ActionResult<Submission>> CreateOne(
            [FromBody, Required] Submission submission)
        {
            await Store.SubmitAsync(submission);
            return submission;
        }

        /// <summary>
        /// Get all the submissions for the plagiarism system
        /// </summary>
        /// <param name="set">The plagiarism set ID</param>
        /// <response code="200">Returns all the submissions for the plagiarism system</response>
        [HttpGet]
        public async Task<ActionResult<Submission[]>> GetAll(
            [FromQuery, Required] string set)
        {
            var result = await Store.ListSubmissionsAsync(set);
            return result.ToArray();
        }

        /// <summary>
        /// Get the given submission for the plagiarism system
        /// </summary>
        /// <param name="id">The ID of the entity to get</param>
        /// <param name="includeFiles">Whether to include submission files</param>
        /// <response code="200">Returns the given submission for the plagiarism system</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Submission>> GetOne(
            [FromRoute, Required] string id,
            [FromQuery] bool includeFiles = true)
        {
            return await Store.FindSubmissionAsync(id, includeFiles);
        }

        /// <summary>
        /// Get the compilation result of given submission for the plagiarism system
        /// </summary>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the compilation result of given submission for the plagiarism system</response>
        [HttpGet("{id}/compilation")]
        public async Task<ActionResult<Compilation>> GetCompilation(
            [FromRoute, Required] string id)
        {
            return await Store.GetCompilationAsync(id);
        }

        /// <summary>
        /// Get the comparison results of given submission for the plagiarism system
        /// </summary>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the comparison results of given submission for the plagiarism system</response>
        [HttpGet("{id}/comparisons")]
        public async Task<ActionResult<Comparison[]>> GetComparisons(
            [FromRoute, Required] string id)
        {
            var result = await Store.GetComparisonsBySubmissionAsync(id);
            return result.ToArray();
        }
    }
}
