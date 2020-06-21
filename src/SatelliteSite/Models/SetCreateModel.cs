using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SatelliteSite.Models
{
    public class SetCreateModel
    {
        [Required]
        [StringLength(30, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public IFormFileCollection Files { get; set; }
    }
}
