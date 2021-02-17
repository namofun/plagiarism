using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plag.Backend;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using IStoreService = Plag.Backend.Services.IPlagiarismDetectService;

namespace SatelliteSite.PlagModule.Apis
{
    /// <summary>
    /// The language API
    /// </summary>
    [Area("Api")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/plagiarism/[controller]")]
    [Produces("application/json")]
    public class LanguagesController : ApiControllerBase
    {
        public IStoreService Store { get; }

        public LanguagesController(IStoreService store)
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
