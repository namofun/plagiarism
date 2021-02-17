using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IStoreService = Plag.Backend.Services.IPlagiarismDetectService;

namespace SatelliteSite.PlagModule.Apis
{
    /// <summary>
    /// The core API
    /// </summary>
    [Area("Api")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/plagiarism/[action]")]
    [Produces("application/json")]
    public class CoreController : ApiControllerBase
    {
        public IStoreService Store { get; }

        public CoreController(IStoreService store)
        {
            Store = store;
        }

        /// <summary>
        /// Send a rescue signal to the core service
        /// </summary>
        /// <response code="200">Returns the service version</response>
        [HttpPost]
        public async Task<IActionResult> Rescue()
        {
            await Store.RescueAsync();
            return Ok(Store.GetVersion());
        }
    }
}
