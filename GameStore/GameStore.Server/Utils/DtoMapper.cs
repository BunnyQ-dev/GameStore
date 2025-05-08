using Games_Store.DTOs;
using Games_Store.Models;
using System.Linq;
using System.Collections.Generic; // For HashSet

namespace Games_Store.Utils // Or Services, Helpers, etc.
{
    public static class DtoMapper
    {
        public static GameDto MapGameToDto(Game game, HashSet<int>? ownedGameIds = null)
        {
            if (game == null) return null;

            var dto = new GameDto
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description?.Length > 100 ? game.Description.Substring(0, 100) + "..." : game.Description,
                Price = game.Price,
                DiscountPercentage = game.DiscountPercentage,
                ReleaseDate = game.ReleaseDate,
                ImageUrl = game.CoverImageUrl ?? "",
                Developer = game.GameDevelopers != null ? string.Join(", ", game.GameDevelopers.Select(gd => gd.Developer?.Name)) : "",
                Publisher = game.GamePublishers != null ? string.Join(", ", game.GamePublishers.Select(gp => gp.Publisher?.Name)) : "",
                Genres = game.GameGenres != null ? game.GameGenres.Select(gg => gg.Genre?.Name).Where(n => n != null).ToList() : new List<string>(),
                Platforms = game.GamePlatforms != null ? game.GamePlatforms.Select(gp => gp.Platform?.Name).Where(n => n != null).ToList() : new List<string>(),
                AverageRating = game.GameRatings != null && game.GameRatings.Any() ? Math.Round(game.GameRatings.Average(r => r.Score), 1) : null,
                RatingsCount = game.GameRatings?.Count ?? 0,
                IsOwned = ownedGameIds?.Contains(game.Id) ?? false,
                IsInWishlist = false,
                ReviewCount = game.GameRatings?.Count ?? 0
            };

            if (game.DiscountPercentage.HasValue && game.DiscountPercentage.Value > 0)
            {
                if (game.DiscountPercentage.Value < 100)
                {
                    decimal originalPrice = game.Price / (1 - game.DiscountPercentage.Value / 100m);
                    dto.OriginalPrice = Math.Round(originalPrice, 2);
                }
                else if (game.DiscountPercentage.Value == 100)
                {
                    dto.OriginalPrice = game.Price;
                }
            }

            return dto;
        }

        public static GameDetailsDto MapGameDetailsToDto(Game game, bool isOwnedByCurrentUser, bool isInWishlist)
        {
            if (game == null) return null;

            decimal currentPrice = game.Price;
            decimal? originalPrice = null;
            if (game.DiscountPercentage.HasValue && game.DiscountPercentage.Value > 0)
            {
                if (game.DiscountPercentage.Value < 100)
                {
                    currentPrice = game.Price * (1 - game.DiscountPercentage.Value / 100m);
                }
                else
                {
                    currentPrice = 0;
                }
                originalPrice = game.Price;
            }

            var dto = new GameDetailsDto
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                Price = Math.Round(currentPrice, 2),
                DiscountPercentage = game.DiscountPercentage,
                OriginalPrice = originalPrice.HasValue ? Math.Round(originalPrice.Value, 2) : null,
                ReleaseDate = game.ReleaseDate,
                CoverImageUrl = game.CoverImageUrl,
                BackgroundImageUrl = game.BackgroundImageUrl,
                TrailerUrl = game.TrailerUrl,
                Developer = game.GameDevelopers != null ? string.Join(", ", game.GameDevelopers.Select(gd => gd.Developer?.Name)) : "",
                Publisher = game.GamePublishers != null ? string.Join(", ", game.GamePublishers.Select(gp => gp.Publisher?.Name)) : "",
                Genres = game.GameGenres != null ? game.GameGenres.Select(gg => gg.Genre?.Name).Where(n => n != null).ToList() : new List<string>(),
                Platforms = game.GamePlatforms != null ? game.GamePlatforms.Select(gp => gp.Platform?.Name).Where(n => n != null).ToList() : new List<string>(),
                MinimumRequirements = game.MinimumSystemRequirements,
                RecommendedRequirements = game.RecommendedSystemRequirements,
                Screenshots = game.Screenshots?.Select(s => new ScreenshotDto { Id = s.Id, Url = s.Url }).ToList() ?? new List<ScreenshotDto>(),
                Ratings = game.GameRatings?.Select(r => new RatingDto
                {
                    UserId = r.UserId,
                    UserName = r.User?.UserName ?? "Anonymous",
                    Rating = r.Score,
                    Review = r.Comment,
                    DatePosted = r.RatingDate
                }).ToList() ?? new List<RatingDto>(),
                AverageRating = game.GameRatings != null && game.GameRatings.Any() ? Math.Round(game.GameRatings.Average(r => r.Score), 1) : null,
                RatingsCount = game.GameRatings?.Count ?? 0,
                IsInWishlist = isInWishlist,
                IsOwned = isOwnedByCurrentUser,
                IsPurchased = isOwnedByCurrentUser,
                ReviewCount = game.GameRatings?.Count ?? 0
            };

            return dto;
        }

        // Add other mappers here if needed
    }
} 