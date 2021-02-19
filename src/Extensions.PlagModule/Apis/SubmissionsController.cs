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
        /// <param name="exclusive_category">The exclusive category ID</param>
        /// <param name="inclusive_category">The non-exclusive category ID</param>
        /// <param name="min_percent">The minimal percent to show</param>
        /// <response code="200">Returns all the submissions for the plagiarism system</response>
        [HttpGet]
        public async Task<ActionResult<Submission[]>> GetAll(
            [FromRoute, Required] string sid,
            [FromQuery] int? exclusive_category = null,
            [FromQuery] int? inclusive_category = null,
            [FromQuery] double? min_percent = null)
        {
            var result = await Store.ListSubmissionsAsync(sid, exclusive_category, inclusive_category, min_percent);
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
            return result.ToArray();
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
        /// Get the comparison results of given submission for the plagiarism system
        /// </summary>
        /// <param name="sid">The plagiarism set ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the comparison results of given submission for the plagiarism system</response>
        [HttpGet("{id}/comparisons")]
        public async Task<ActionResult<Comparison[]>> GetComparisons(
            [FromRoute, Required] string sid,
            [FromRoute, Required] int id)
        {
            var result = await Store.GetComparisonsBySubmissionAsync(sid, id);
            return result.ToArray();
        }
    }
}
