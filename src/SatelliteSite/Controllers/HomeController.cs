using Microsoft.AspNetCore.Mvc;
using SatelliteSite.Models;
using System.Diagnostics;

namespace SatelliteSite.Controllers
{
    [Route("[action]")]
    public class HomeController : Controller2
    {
        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
