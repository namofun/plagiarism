using System.ComponentModel.DataAnnotations;

namespace SatelliteSite.PlagModule.Models
{
    public class SetCreateModel
    {
        [Required]
        [StringLength(30, MinimumLength = 1)]
        public string Name { get; set; }
    }
}
