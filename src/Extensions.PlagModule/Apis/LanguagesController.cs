﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Models;
using Xylab.PlagiarismDetect.Backend.Services;

namespace SatelliteSite.PlagModule.Apis
{
    /// <summary>
    /// The language API
    /// </summary>
    [Area("Api")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/plagiarism/[controller]")]
    [Produces("application/json")]
    [CustomedExceptionFilter]
    public class LanguagesController : ApiControllerBase
    {
        public IPlagiarismDetectService Store { get; }

        public LanguagesController(IPlagiarismDetectService store)
        {
            Store = store;
        }


        /// <summary>
        /// Get all the languages for the plagiarism system
        /// </summary>
        /// <response code="200">Returns all the languages for the plagiarism system</response>
        [HttpGet]
        public async Task<ActionResult<List<LanguageInfo>>> GetAll()
        {
            return await Store.ListLanguageAsync();
        }


        /// <summary>
        /// Get the given language for the plagiarism system
        /// </summary>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given language for the plagiarism system</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<LanguageInfo>> GetOne(
            [FromRoute, Required] string id)
        {
            return await Store.FindLanguageAsync(id);
        }
    }
}
