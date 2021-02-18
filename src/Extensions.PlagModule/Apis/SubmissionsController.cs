using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using IStoreService = Plag.Backend.Services.IPlagiarismDetectService;

namespace SatelliteSite.PlagModule.Apis
{
    /// <summary>
    /// The submission API
    /// </summary>
    [Area("Api")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/plagiarism/[controller]")]
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
        [ProducesResponseType(typeof(Submission), 201)]
        public async Task<ActionResult<Submission>> CreateOne(
            [FromBody, Required] SubmissionCreation submission)
        {
            var s = await Store.SubmitAsync(submission);
            return CreatedAtAction(nameof(GetOne), new { id = s.Id }, s);
        }

        /// <summary>
        /// Get all the submissions for the plagiarism system
        /// </summary>
        /// <param name="id">The plagiarism set ID</param>
        /// <response code="200">Returns all the submissions for the plagiarism system</response>
        [HttpGet("/[area]/plagiarism/sets/{id}/submissions")]
        public async Task<ActionResult<Submission[]>> GetSubmissions(
            [FromRoute, Required] string id)
        {
            var result = await Store.ListSubmissionsAsync(id);
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
