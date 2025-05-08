using System.ComponentModel.DataAnnotations;

namespace Games_Store.DTOs
{
    public class CreatePlatformDto
    {
        [Required(ErrorMessage = "Platform name is required.")]
        [StringLength(100, ErrorMessage = "Platform name cannot be longer than 100 characters.")]
        public string Name { get; set; } = string.Empty;
    }
} 