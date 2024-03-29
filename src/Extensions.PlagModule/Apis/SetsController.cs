﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Models;
using Xylab.PlagiarismDetect.Backend.Services;

namespace SatelliteSite.PlagModule.Apis
{
    /// <summary>
    /// The set API
    /// </summary>
    [Area("Api")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/plagiarism/[controller]")]
    [Produces("application/json")]
    [CustomedExceptionFilter]
    public class SetsController : ApiControllerBase
    {
        public IPlagiarismDetectService Store { get; }

        public SetsController(IPlagiarismDetectService store)
        {
            Store = store;
        }

        /// <summary>
        /// Create a set for the plagiarism system
        /// </summary>
        /// <param name="model">The metadata of plagiarism set</param>
        /// <response code="201">Returns the created plagiarism set</response>
        [HttpPost]
        [ProducesResponseType(typeof(PlagiarismSet), 201)]
        public async Task<ActionResult<PlagiarismSet>> CreateOne(
            [FromBody, Required] SetCreation model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var result = await Store.CreateSetAsync(model);
            return CreatedAtAction(nameof(GetOne), new { sid = result.Id }, result);
        }

        /// <summary>
        /// Get the given set for the plagiarism system
        /// </summary>
        /// <param name="sid">The ID of the entity to get</param>
        /// <response code="200">Returns the given set for the plagiarism system</response>
        [HttpGet("{sid}")]
        public async Task<ActionResult<PlagiarismSet>> GetOne(
            [FromRoute, Required] string sid)
        {
            return await Store.FindSetAsync(sid);
        }

        /// <summary>
        /// Get all the sets for the plagiarism system
        /// </summary>
        /// <param name="creator">The creator of entity</param>
        /// <param name="related">The related of entity</param>
        /// <param name="skip">The count to skip</param>
        /// <param name="limit">The count to take</param>
        /// <param name="order">The order to enumerate the sets, "asc" or "desc"</param>
        /// <response code="200">Returns all the sets for the plagiarism system</response>
        [HttpGet]
        public async Task<ActionResult<PlagiarismSet[]>> GetAll(
            [FromQuery] int? creator = null,
            [FromQuery] int? related = null,
            [FromQuery] int? skip = null,
            [FromQuery] int? limit = null,
            [FromQuery] string order = "desc")
        {
            if (order != "asc" && order != "desc") return BadRequest();
            if (skip.HasValue && skip.Value < 0) return BadRequest();
            if (limit.HasValue && limit.Value < 0) return BadRequest();

            var items = await Store.ListSetsAsync(related, creator, skip, limit, order == "asc");
            return items.ToArray();
        }
    }
}
