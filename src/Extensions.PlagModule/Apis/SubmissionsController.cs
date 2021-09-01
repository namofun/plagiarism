using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.PlagModule.Apis
{
    /// <summary>
    /// The submission API
    /// </summary>
    [Area("Api")]
    [Authorize(Roles = "Administrator")]
    [Route("/api/plagiarism/sets/{sid}/submissions")]
    [Produces("application/json")]
    [CustomedExceptionFilter]
    public class SubmissionsController : ApiControllerBase
    {
        public IPlagiarismDetectService Store { get; }

        public SubmissionsController(IPlagiarismDetectService store)
        {
            Store = store;
        }


        /// <summary>
        /// Create a submission for the plagiarism system
        /// </summary>
        /// <param name="sid">The plagiarism set ID</param>
        /// <param name="submission">The entity to create</param>
        /// <response code="201">Returns the created submission for the plagiarism system</response>
        [HttpPost]
        [ProducesResponseType(typeof(Submission), 201)]
        public async Task<ActionResult<Submission>> CreateOne(
            [FromRoute, Required] string sid,
            [FromBody, Required] SubmissionCreation submission)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (submission.SetId != sid) return BadRequest();
            if (submission.Files == null || submission.Files.Count == 0) return BadRequest();
            var s = await Store.SubmitAsync(submission);

            if (s.Files == null)
            {
                s.Files = submission.Files
                    .Select((s, j) => new SubmissionFile
                    {
                        FileId = j + 1,
                        FileName = s.FileName,
                        FilePath = s.FilePath,
                    })
                    .ToList();
            }

            return CreatedAtAction(nameof(GetOne), new { id = s.Id }, s);
        }


        /// <summary>
        /// Get all the submissions for the plagiarism system
        /// </summary>
        /// <param name="sid">The plagiarism set ID</param>
        /// <param name="language">The language ID</param>
        /// <param name="exclusive_category">The exclusive category ID</param>
        /// <param name="inclusive_category">The non-exclusive category ID</param>
        /// <param name="min_percent">The minimal percent to show</param>
        /// <param name="skip">The count to skip</param>
        /// <param name="limit">The count to take</param>
        /// <param name="order">The order to enumerate the sets, "asc" or "desc"</param>
        /// <param name="by">The order rule column, "id" or "percent"</param>
        /// <response code="200">Returns all the submissions for the plagiarism system</response>
        [HttpGet]
        public async Task<ActionResult<Submission[]>> GetAll(
            [FromRoute, Required] string sid,
            [FromQuery] string language = null,
            [FromQuery] int? exclusive_category = null,
            [FromQuery] int? inclusive_category = null,
            [FromQuery] double? min_percent = null,
            [FromQuery] int? skip = null,
            [FromQuery] int? limit = null,
            [FromQuery] string order = "asc",
            [FromQuery] string by = "id")
        {
            by = (by ?? "id").ToLowerInvariant();
            if (order != "asc" && order != "desc") return BadRequest();
            if (by != "id" && by != "percent") return BadRequest();
            if (skip.HasValue && skip.Value < 0) return BadRequest();
            if (limit.HasValue && limit.Value < 0) return BadRequest();

            var result = await Store.ListSubmissionsAsync(sid, language, exclusive_category, inclusive_category, min_percent, skip, limit, by, order == "asc");
            return result.ToArray();
        }


        /// <summary>
        /// Get the given submission for the plagiarism system
        /// </summary>
        /// <param name="sid">The plagiarism set ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <param name="includeFiles">Whether to include submission files</param>
        /// <response code="200">Returns the given submission for the plagiarism system</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Submission>> GetOne(
            [FromRoute, Required] string sid,
            [FromRoute, Required] int id,
            [FromQuery] bool includeFiles = true)
        {
            return await Store.FindSubmissionAsync(sid, id, includeFiles);
        }


        /// <summary>
        /// Get the files of given submission for the plagiarism system
        /// </summary>
        /// <param name="sid">The plagiarism set ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the files of given submission for the plagiarism system</response>
        [HttpGet("{id}/files")]
        public async Task<ActionResult<SubmissionFile[]>> GetFiles(
            [FromRoute, Required] string sid,
            [FromRoute, Required] int id)
        {
            var result = await Store.GetFilesAsync(sid, id);
            return result?.ToArray();
        }


        /// <summary>
        /// Get the compilation result of given submission for the plagiarism system
        /// </summary>
        /// <param name="sid">The plagiarism set ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the compilation result of given submission for the plagiarism system</response>
        [HttpGet("{id}/compilation")]
        public async Task<ActionResult<Compilation>> GetCompilation(
            [FromRoute, Required] string sid,
            [FromRoute, Required] int id)
        {
            return await Store.GetCompilationAsync(sid, id);
        }


        /// <summary>
        /// Reset the compilation result of given submission for the plagiarism system
        /// </summary>
        /// <param name="sid">The plagiarism set ID</param>
        /// <param name="id">The ID of the entity to reset</param>
        /// <response code="200">Returns the service version</response>
        [HttpDelete("{id}/compilation")]
        public async Task<ActionResult<ServiceVersion>> ResetCompilation(
            [FromRoute, Required] string sid,
            [FromRoute, Required] int id)
        {
            await Store.ResetCompilationAsync(sid, id);
            return Store.GetVersion();
        }


        /// <summary>
        /// Get the comparison results of given submission for the plagiarism system
        /// </summary>
        /// <param name="sid">The plagiarism set ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <param name="includeFiles">Whether to include submission files</param>
        /// <response code="200">Returns the comparison results of given submission for the plagiarism system</response>
        [HttpGet("{id}/comparisons")]
        public async Task<ActionResult<Vertex>> GetComparisons(
            [FromRoute, Required] string sid,
            [FromRoute, Required] int id,
            [FromQuery] bool includeFiles = false)
        {
            return await Store.GetComparisonsBySubmissionAsync(sid, id, includeFiles);
        }
    }
}
