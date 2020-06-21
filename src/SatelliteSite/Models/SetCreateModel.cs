using System.ComponentModel.DataAnnotations;

namespace SatelliteSite.Models
{
    public class SetCreateModel
    {
        [Required]
        [StringLength(30, MinimumLength = 1)]
        public string Name { get; set; }
    }
}
