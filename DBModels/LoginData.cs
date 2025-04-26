using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LinkedInAPI.DBModels
{
    public class LoginData
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Name { get; set; }

        [Required]
        public string? Email { get; set; }

        public string? ProviderName { get; set; }

      
    }
}
