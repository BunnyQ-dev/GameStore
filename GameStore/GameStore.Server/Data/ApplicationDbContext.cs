using Games_Store.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Games_Store.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Game> Games { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Developer> Developers { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Bundle> Bundles { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        
        // Join entities
        public DbSet<GameGenre> GameGenres { get; set; }
        public DbSet<GamePlatform> GamePlatforms { get; set; }
        public DbSet<GameDeveloper> GameDevelopers { get; set; }
        public DbSet<GamePublisher> GamePublishers { get; set; }
        public DbSet<BundleGame> BundleGames { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<PurchasedGame> PurchasedGames { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<GameRating> GameRatings { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<PlaySession> PlaySessions { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<CartBundle> CartBundles { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Screenshot> Screenshots { get; set; }
        public DbSet<OrderBundle> OrderBundles { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Game-Genre M:N relationship
            modelBuilder.Entity<GameGenre>()
                .HasKey(gg => new { gg.GameId, gg.GenreId });
                
            modelBuilder.Entity<GameGenre>()
                .HasOne(gg => gg.Game)
                .WithMany(g => g.GameGenres)
                .HasForeignKey(gg => gg.GameId);
                
            modelBuilder.Entity<GameGenre>()
                .HasOne(gg => gg.Genre)
                .WithMany(g => g.GameGenres)
                .HasForeignKey(gg => gg.GenreId);
            
            // Game-Platform M:N relationship
            modelBuilder.Entity<GamePlatform>()
                .HasKey(gp => new { gp.GameId, gp.PlatformId });
                
            modelBuilder.Entity<GamePlatform>()
                .HasOne(gp => gp.Game)
                .WithMany(g => g.GamePlatforms)
                .HasForeignKey(gp => gp.GameId);
                
            modelBuilder.Entity<GamePlatform>()
                .HasOne(gp => gp.Platform)
                .WithMany(p => p.GamePlatforms)
                .HasForeignKey(gp => gp.PlatformId);
            
            // Game-Developer M:N relationship
            modelBuilder.Entity<GameDeveloper>()
                .HasKey(gd => new { gd.GameId, gd.DeveloperId });
                
            modelBuilder.Entity<GameDeveloper>()
                .HasOne(gd => gd.Game)
                .WithMany(g => g.GameDevelopers)
                .HasForeignKey(gd => gd.GameId);
                
            modelBuilder.Entity<GameDeveloper>()
                .HasOne(gd => gd.Developer)
                .WithMany(d => d.GameDevelopers)
                .HasForeignKey(gd => gd.DeveloperId);
            
            // Game-Publisher M:N relationship
            modelBuilder.Entity<GamePublisher>()
                .HasKey(gp => new { gp.GameId, gp.PublisherId });
                
            modelBuilder.Entity<GamePublisher>()
                .HasOne(gp => gp.Game)
                .WithMany(g => g.GamePublishers)
                .HasForeignKey(gp => gp.GameId);
                
            modelBuilder.Entity<GamePublisher>()
                .HasOne(gp => gp.Publisher)
                .WithMany(p => p.GamePublishers)
                .HasForeignKey(gp => gp.PublisherId);
            
            // Bundle-Game M:N relationship
            modelBuilder.Entity<BundleGame>()
                .HasKey(bg => new { bg.BundleId, bg.GameId });
                
            modelBuilder.Entity<BundleGame>()
                .HasOne(bg => bg.Bundle)
                .WithMany(b => b.BundleGames)
                .HasForeignKey(bg => bg.BundleId);
                
            modelBuilder.Entity<BundleGame>()
                .HasOne(bg => bg.Game)
                .WithMany(g => g.BundleGames)
                .HasForeignKey(bg => bg.GameId);
            
            // User-Game Wishlist M:N relationship
            modelBuilder.Entity<Wishlist>()
                .HasKey(w => new { w.UserId, w.GameId });
                
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId);
                
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Game)
                .WithMany(g => g.Wishlists)
                .HasForeignKey(w => w.GameId);
            
            // User-Game Purchased M:N relationship
            modelBuilder.Entity<PurchasedGame>()
                .HasKey(pg => new { pg.UserId, pg.GameId });
                
            modelBuilder.Entity<PurchasedGame>()
                .HasOne(pg => pg.User)
                .WithMany(u => u.PurchasedGames)
                .HasForeignKey(pg => pg.UserId);
                
            modelBuilder.Entity<PurchasedGame>()
                .HasOne(pg => pg.Game)
                .WithMany(g => g.PurchasedGames)
                .HasForeignKey(pg => pg.GameId);
            
            // User-User Friends M:N relationship
            modelBuilder.Entity<Friend>()
                .HasKey(f => new { f.UserId, f.FriendId });
                
            modelBuilder.Entity<Friend>()
                .HasOne(f => f.User)
                .WithMany(u => u.FriendsInitiated)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Friend>()
                .HasOne(f => f.FriendUser)
                .WithMany(u => u.FriendsReceived)
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // FriendRequest relationship
            modelBuilder.Entity<FriendRequest>()
                .HasKey(fr => fr.Id);
                
            modelBuilder.Entity<FriendRequest>()
                .HasIndex(fr => new { fr.SenderId, fr.ReceiverId }).IsUnique();
                
            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany(u => u.FriendRequestsSent)
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany(u => u.FriendRequestsReceived)
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // User-Game Rating M:N relationship
            modelBuilder.Entity<GameRating>()
                .HasKey(gr => new { gr.UserId, gr.GameId });
                
            modelBuilder.Entity<GameRating>()
                .HasOne(gr => gr.User)
                .WithMany(u => u.GameRatings)
                .HasForeignKey(gr => gr.UserId);
                
            modelBuilder.Entity<GameRating>()
                .HasOne(gr => gr.Game)
                .WithMany(g => g.GameRatings)
                .HasForeignKey(gr => gr.GameId);
            
            // User-Achievement M:N relationship
            modelBuilder.Entity<UserAchievement>()
                .HasKey(ua => new { ua.UserId, ua.AchievementId });
                
            modelBuilder.Entity<UserAchievement>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserAchievements)
                .HasForeignKey(ua => ua.UserId);
                
            modelBuilder.Entity<UserAchievement>()
                .HasOne(ua => ua.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(ua => ua.AchievementId);
            
            // User-Game PlaySession relationship
            modelBuilder.Entity<PlaySession>()
                .HasKey(ps => ps.Id);
                
            modelBuilder.Entity<PlaySession>()
                .HasOne(ps => ps.User)
                .WithMany(u => u.PlaySessions)
                .HasForeignKey(ps => ps.UserId);
                
            modelBuilder.Entity<PlaySession>()
                .HasOne(ps => ps.Game)
                .WithMany(g => g.PlaySessions)
                .HasForeignKey(ps => ps.GameId);
            
            // Configure decimals precision
            modelBuilder.Entity<Game>()
                .Property(g => g.Price)
                .HasPrecision(10, 2);
                
            modelBuilder.Entity<Game>()
                .Property(g => g.DiscountPrice)
                .HasPrecision(10, 2);
                
            modelBuilder.Entity<Bundle>()
                .Property(b => b.Price)
                .HasPrecision(10, 2);
                
            modelBuilder.Entity<Bundle>()
                .Property(b => b.DiscountPrice)
                .HasPrecision(10, 2);
                
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.Price)
                .HasPrecision(10, 2);
                
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(12, 2);
                
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(10, 2);
                
            modelBuilder.Entity<FriendRequest>()
                .Property(fr => fr.CreatedAt)
                .HasDefaultValueSql("getdate()");

            // Additional configuration for PurchasedGame.PricePaid
            modelBuilder.Entity<PurchasedGame>()
                .Property(pg => pg.PricePaid)
                .HasPrecision(10, 2);

            // Order-OrderItem 1:N relationship
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Game -> Achievements (one-to-many)
            modelBuilder.Entity<Achievement>()
                .HasOne(a => a.Game);

            // CartBundle M:N (Cart-Bundle relationship)
            modelBuilder.Entity<CartBundle>()
                .HasKey(cb => cb.Id); // single key
            modelBuilder.Entity<CartBundle>()
                .HasOne(cb => cb.Cart)
                .WithMany(c => c.CartBundles)
                .HasForeignKey(cb => cb.CartId);
            modelBuilder.Entity<CartBundle>()
                .HasOne(cb => cb.Bundle)
                .WithMany(b => b.CartBundles)
                .HasForeignKey(cb => cb.BundleId);

            // Configure OrderBundle relationship
            modelBuilder.Entity<OrderBundle>()
                .HasKey(ob => ob.Id); // Primary key for OrderBundle

            modelBuilder.Entity<OrderBundle>()
                .HasOne(ob => ob.Order)
                .WithMany(o => o.Bundles) // Navigation property in Order
                .HasForeignKey(ob => ob.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Delete OrderBundles if Order is deleted

            modelBuilder.Entity<OrderBundle>()
                .HasOne(ob => ob.Bundle)
                .WithMany() // Bundle might not need a direct navigation back to OrderBundles
                .HasForeignKey(ob => ob.BundleId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Bundle if it's in an order
        }
    }
} 