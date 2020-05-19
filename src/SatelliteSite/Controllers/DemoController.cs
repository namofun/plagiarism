using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SatelliteSite.Data;
using SatelliteSite.Data.Demos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SatelliteSite.Controllers
{
    [Route("[controller]/[action]")]
    public class DemoController : Controller2
    {
        private DemoContext Context { get; }

        public DemoController(DemoContext context)
        {
            Context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var families = await Context.Families.ToListAsync();
            return View("List", families.PrettyJson());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Create a family object for the Andersen family
            var andersenFamily = new Family
            {
                Id = Guid.NewGuid().ToString(),
                LastName = "Andersen",
                Parents = new List<Parent>
                {
                    new Parent { FirstName = "Thomas" },
                    new Parent { FirstName = "Mary Kay" }
                },
                Children = new Collection<Child>
                {
                    new Child
                    {
                        FirstName = "Henriette Thaulow",
                        Gender = "female",
                        Grade = 5,
                        Pets = new HashSet<Pet>
                        {
                            new Pet { GivenName = "Fluffy" }
                        }
                    }
                },
                Address = new Address { State = "WA", County = "King", City = "Seattle" },
                IsRegistered = false
            };

            var item = Context.Families.Add(andersenFamily);
            await Context.SaveChangesAsync();
            StatusMessage = "Item added.";
            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        public async Task<IActionResult> Delete()
        {
            var families = await Context.Families.ToListAsync();
            
            if (families.Count > 0)
            {
                var item = families[new Random().Next(0, families.Count)];
                Context.Families.Remove(item);
                await Context.SaveChangesAsync();
                StatusMessage = "Item remove.";
            }
            else
            {
                StatusMessage = "Error Empty.";
            }

            return RedirectToAction(nameof(List));
        }
    }
}
