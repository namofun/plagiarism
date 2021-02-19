using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SatelliteSite.PlagModule.Models
{
    public class SetUploadModel
    {
        [Required]
        public string Language { get; set; }

        [Required]
        public IFormFileCollection Files { get; set; }

        [DisplayName("Non-exclusive category")]
        public int Inclusive { get; set; }

        [DisplayName("Exclusive category")]
        public int? Exclusive { get; set; }
    }
}
