using System;

namespace Games_Store.DTOs
{
    public class DeveloperDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime? FoundedDate { get; set; }
    }
} 