using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Games_Store.Models
{
    public class Screenshot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GameId { get; set; }

        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; } = null!;

        [Required]
        public string Url { get; set; } = null!;
    }
} 