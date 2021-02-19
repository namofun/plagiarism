﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Models;
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
    [Authorize(Roles = "Administrator")]
    [Route("[area]/plagiarism/[controller]")]
    [Produces("application/json")]
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
        /// <param name="name">The name of plagiarism set</param>
        /// <response code="201">Returns the created plagiarism set</response>
        [HttpPost]
        [ProducesResponseType(typeof(PlagiarismSet), 201)]
        public async Task<ActionResult<PlagiarismSet>> CreateOne(
            [FromForm, Required] string name)
        {
            var result = await Store.CreateSetAsync(name);
            return CreatedAtAction(nameof(GetOne), new { id = result.Id }, result);
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
        /// <param name="skip">The count to skip</param>
        /// <param name="limit">The count to take</param>
        /// <response code="200">Returns all the sets for the plagiarism system</response>
        [HttpGet]
        public async Task<ActionResult<PlagiarismSet[]>> GetAll(
            [FromQuery] int? skip = null,
            [FromQuery] int? limit = null)
        {
            var items = await Store.ListSetsAsync(skip, limit);
            return items.ToArray();
        }
    }
}
