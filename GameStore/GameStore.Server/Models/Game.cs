using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Games_Store.Models
{
    public class Game
    {
        public Game()
        {
            GameGenres = new List<GameGenre>();
            GamePlatforms = new List<GamePlatform>();
            GameDevelopers = new List<GameDeveloper>();
            GamePublishers = new List<GamePublisher>();
            BundleGames = new List<BundleGame>();
            Wishlists = new List<Wishlist>();
            PurchasedGames = new List<PurchasedGame>();
            GameRatings = new List<GameRating>();
            Achievements = new List<Achievement>();
            PlaySessions = new List<PlaySession>();
            Screenshots = new List<Screenshot>();
        }



        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercentage { get; set; } = 0;

        [Required]
        public DateTime ReleaseDate { get; set; }

        public string? CoverImageUrl { get; set; }

        public string? BackgroundImageUrl { get; set; }

        public string? TrailerUrl { get; set; }

        public string? MinimumSystemRequirements { get; set; }

        public string? RecommendedSystemRequirements { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; }

        public int Sales { get; set; }

        public decimal? DiscountPrice { get; set; }

        public double? AverageRating { get; set; } 
        public int ReviewCount { get; set; }      

        [NotMapped]
        public Developer? Developer { get; set; }
        
        [NotMapped]
        public Publisher? Publisher { get; set; }
        
        [NotMapped]
        public string? ImageUrl 
        { 
            get => CoverImageUrl; 
            set => CoverImageUrl = value; 
        }
        
        [NotMapped]
        public string? VideoUrl 
        { 
            get => TrailerUrl; 
            set => TrailerUrl = value; 
        }
        


        [NotMapped]
        public ICollection<Genre> Genres { get; set; } = new List<Genre>();

        // Navigation properties
        public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();

        public ICollection<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();

        public ICollection<GameDeveloper> GameDevelopers { get; set; }

        public ICollection<GamePublisher> GamePublishers { get; set; }

        public ICollection<BundleGame> BundleGames { get; set; }

        public ICollection<Wishlist> Wishlists { get; set; }

        public ICollection<PurchasedGame> PurchasedGames { get; set; }

        public ICollection<GameRating> GameRatings { get; set; }

        public ICollection<Achievement> Achievements { get; set; }

        public ICollection<PlaySession> PlaySessions { get; set; }

        public ICollection<Screenshot> Screenshots { get; set; }
    }
} 