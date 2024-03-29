﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Models;
using Xylab.PlagiarismDetect.Backend.Services;

namespace SatelliteSite.PlagModule.Apis
{
    /// <summary>
    /// The core API
    /// </summary>
    [Area("Api")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/plagiarism/[action]")]
    [Produces("application/json")]
    [CustomedExceptionFilter]
    public class CoreController : ApiControllerBase
    {
        public IPlagiarismDetectService Store { get; }

        public CoreController(IPlagiarismDetectService store)
        {
            Store = store;
        }


        /// <summary>
        /// Send a rescue signal to the core service
        /// </summary>
        /// <response code="200">Returns the service version</response>
        [HttpPost]
        public async Task<ActionResult<ServiceVersion>> Rescue()
        {
            await Store.RescueAsync();
            return Store.GetVersion();
        }
    }
}
