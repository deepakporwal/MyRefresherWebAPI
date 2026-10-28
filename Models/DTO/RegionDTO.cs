using System.ComponentModel.DataAnnotations;
using MyRefresherWebAPI.Validation;

namespace MyRefresherWebAPI.Models.DTO
{
    public class RegionDTO
    {
        [GreaterThanZero]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Code { get; set; }
    }
}
