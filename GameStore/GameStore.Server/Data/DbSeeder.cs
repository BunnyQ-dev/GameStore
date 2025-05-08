using Games_Store.Data;
using Games_Store.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Games_Store.Data
{
    public static class DbSeeder
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await SeedRoles(roleManager);
            
            await SeedGenres(db);
            
            await SeedPlatforms(db);
            
            await SeedDevelopers(db);
            
            await SeedPublishers(db);
            
            await SeedGames(db);
        }
        
        public static async Task MakeUserAdmin(IServiceProvider serviceProvider, string userEmail)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            var user = await userManager.FindByEmailAsync(userEmail);
            if (user != null)
            {
                await userManager.AddToRoleAsync(user, "Admin");
                Console.WriteLine($"User {userEmail} successfully assigned as admin.");
            }
            else
            {
                Console.WriteLine($"User with email {userEmail} not found.");
            }
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "Player" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"Role {role} created.");
                }
            }
        }

        private static async Task SeedGenres(ApplicationDbContext db)
        {
            if (await db.Genres.AnyAsync())
                return;

            var genres = new List<Genre>
            {
                new Genre { Name = "RPG", Description = "Role-playing games focus on immersive storytelling and character development." },
                new Genre { Name = "Action", Description = "Fast-paced games requiring quick reflexes and precision." },
                new Genre { Name = "Adventure", Description = "Games that emphasize exploration and puzzle-solving." },
                new Genre { Name = "Strategy", Description = "Games that prioritize thinking and planning." },
                new Genre { Name = "Shooter", Description = "Games focused on weapon-based combat with first or third-person perspectives." },
                new Genre { Name = "Simulation", Description = "Games designed to simulate real-world activities or systems." },
                new Genre { Name = "Sports", Description = "Games based on real-world sports or athletics." },
                new Genre { Name = "Puzzle", Description = "Games focused on problem-solving challenges." },
                new Genre { Name = "Horror", Description = "Games designed to scare or frighten the player." },
                new Genre { Name = "Indie", Description = "Games developed by individuals or small teams without publisher support." },
                new Genre { Name = "Platformer", Description = "Games involving navigating characters across platforms and obstacles." },
                new Genre { Name = "Racing", Description = "Games that involve competing in races with various vehicles." },
                new Genre { Name = "MMO", Description = "Massively multiplayer online games with large persistent worlds." },
                new Genre { Name = "Fighting", Description = "Games focused on combat between characters." },
                new Genre { Name = "Stealth", Description = "Games emphasizing sneaking and avoiding detection." },
                new Genre { Name = "Survival", Description = "Games focused on resource management and staying alive in harsh environments." },
                new Genre { Name = "Open World", Description = "Games with large, freely explorable environments." },
                new Genre { Name = "Roguelike", Description = "Games featuring procedurally generated levels and permadeath." },
                new Genre { Name = "MOBA", Description = "Multiplayer online battle arena games." },
                new Genre { Name = "Visual Novel", Description = "Story-focused games with minimal gameplay interactions." }
            };

            await db.Genres.AddRangeAsync(genres);
            await db.SaveChangesAsync();
            Console.WriteLine($"Added {genres.Count} genres.");
        }

        private static async Task SeedPlatforms(ApplicationDbContext db)
        {
            if (await db.Platforms.AnyAsync())
                return;

            var platforms = new List<Platform>
            {
                new Platform { Name = "PC", Description = "Personal Computer gaming platform", LogoUrl = "https://www.freepnglogos.com/uploads/windows-logo-png/windows-logo-logok-0.png" },
                new Platform { Name = "PlayStation 5", Description = "Sony's latest gaming console", LogoUrl = "https://www.freepnglogos.com/uploads/playstation-png-logo/playstation-png-logo-0.png" },
                new Platform { Name = "Xbox Series X", Description = "Microsoft's latest gaming console", LogoUrl = "https://www.freepnglogos.com/uploads/xbox-png-logo/xbox-png-logo-2.png" },
                new Platform { Name = "Nintendo Switch", Description = "Nintendo's hybrid gaming console", LogoUrl = "https://www.freepnglogos.com/uploads/nintendo-switch-png-logo/nintendo-switch-png-logo-0.png" },
                new Platform { Name = "PlayStation 4", Description = "Sony's previous generation console", LogoUrl = "https://www.freepnglogos.com/uploads/playstation-png-logo/playstation-png-logo-0.png" },
                new Platform { Name = "Xbox One", Description = "Microsoft's previous generation console", LogoUrl = "https://www.freepnglogos.com/uploads/xbox-png-logo/xbox-png-logo-2.png" },
                new Platform { Name = "Mobile", Description = "iOS and Android mobile platforms", LogoUrl = "https://cdn-icons-png.flaticon.com/512/545/545245.png" }
            };

            await db.Platforms.AddRangeAsync(platforms);
            await db.SaveChangesAsync();
            Console.WriteLine($"Added {platforms.Count} platforms.");
        }

        private static async Task SeedDevelopers(ApplicationDbContext db)
        {
            if (await db.Developers.AnyAsync())
                return;

            var developers = new List<Developer>
            {
                new Developer { 
                    Name = "CD Projekt Red", 
                    Description = "Polish video game developer known for The Witcher series and Cyberpunk 2077",
                    Website = "https://www.cdprojektred.com/", 
                    LogoUrl = "https://www.cdprojekt.com/en/wp-content/uploads-en/2016/12/logo_cdp-6.png",
                    FoundedDate = new DateTime(1994, 5, 1)
                },
                new Developer { 
                    Name = "Rockstar Games", 
                    Description = "American video game publisher known for Grand Theft Auto and Red Dead Redemption",
                    Website = "https://www.rockstargames.com/", 
                    LogoUrl = "https://www.rockstargames.com/images/screen/rockstarlogo.png",
                    FoundedDate = new DateTime(1998, 12, 1)
                },
                new Developer { 
                    Name = "Ubisoft", 
                    Description = "French video game company known for Assassin's Creed and Far Cry series",
                    Website = "https://www.ubisoft.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/7/78/Ubisoft_logo.svg",
                    FoundedDate = new DateTime(1986, 3, 28)
                },
                new Developer { 
                    Name = "Electronic Arts", 
                    Description = "American video game company known for FIFA, The Sims, and Battlefield",
                    Website = "https://www.ea.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/d/d9/EA_Sports.png",
                    FoundedDate = new DateTime(1982, 5, 27)
                },
                new Developer { 
                    Name = "Valve Corporation", 
                    Description = "American video game developer known for Half-Life, Portal, and Steam",
                    Website = "https://www.valvesoftware.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/a/ab/Valve_logo.svg",
                    FoundedDate = new DateTime(1996, 8, 24)
                },
                new Developer { 
                    Name = "Bethesda Game Studios", 
                    Description = "American video game developer known for The Elder Scrolls and Fallout series",
                    Website = "https://bethesdagamestudios.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/2/22/Bethesda_Game_Studios_logo.png",
                    FoundedDate = new DateTime(2001, 1, 1)
                },
                new Developer { 
                    Name = "Nintendo", 
                    Description = "Japanese video game company known for Mario, Zelda, and Pokémon franchises",
                    Website = "https://www.nintendo.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/0/0d/Nintendo.svg",
                    FoundedDate = new DateTime(1889, 9, 23)
                },
                new Developer { 
                    Name = "Blizzard Entertainment", 
                    Description = "American video game developer known for World of Warcraft, Diablo, and Overwatch",
                    Website = "https://www.blizzard.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/2/23/Blizzard_Entertainment_Logo_2015.svg",
                    FoundedDate = new DateTime(1991, 2, 8)
                },
                new Developer { 
                    Name = "FromSoftware", 
                    Description = "Japanese video game developer known for Dark Souls series and Elden Ring",
                    Website = "https://www.fromsoftware.jp/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/0/00/Fromsoftware_logo.svg",
                    FoundedDate = new DateTime(1986, 11, 1)
                },
                new Developer { 
                    Name = "Naughty Dog", 
                    Description = "American video game developer known for The Last of Us and Uncharted series",
                    Website = "https://www.naughtydog.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/9/9e/Naughty_Dog_logo.svg",
                    FoundedDate = new DateTime(1984, 9, 27)
                }
            };

            await db.Developers.AddRangeAsync(developers);
            await db.SaveChangesAsync();
            Console.WriteLine($"Added {developers.Count} developers.");
        }

        private static async Task SeedPublishers(ApplicationDbContext db)
        {
            if (await db.Publishers.AnyAsync())
                return;

            var publishers = new List<Publisher>
            {
                new Publisher { 
                    Name = "CD Projekt", 
                    Description = "Polish video game publisher and distributor",
                    Website = "https://www.cdprojekt.com/", 
                    LogoUrl = "https://www.cdprojekt.com/en/wp-content/uploads-en/2016/12/logo_cdp-6.png",
                    FoundedDate = new DateTime(1994, 5, 1)
                },
                new Publisher { 
                    Name = "Rockstar Games", 
                    Description = "American video game publisher",
                    Website = "https://www.rockstargames.com/", 
                    LogoUrl = "https://www.rockstargames.com/images/screen/rockstarlogo.png",
                    FoundedDate = new DateTime(1998, 12, 1)
                },
                new Publisher { 
                    Name = "Ubisoft", 
                    Description = "French video game publisher",
                    Website = "https://www.ubisoft.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/7/78/Ubisoft_logo.svg",
                    FoundedDate = new DateTime(1986, 3, 28)
                },
                new Publisher { 
                    Name = "Electronic Arts", 
                    Description = "American video game publisher",
                    Website = "https://www.ea.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/d/d9/EA_Sports.png",
                    FoundedDate = new DateTime(1982, 5, 27)
                },
                new Publisher { 
                    Name = "Valve", 
                    Description = "American video game publisher",
                    Website = "https://www.valvesoftware.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/a/ab/Valve_logo.svg",
                    FoundedDate = new DateTime(1996, 8, 24)
                },
                new Publisher { 
                    Name = "Bethesda Softworks", 
                    Description = "American video game publisher",
                    Website = "https://bethesda.net/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/6/6f/Bethesda_Softworks_Logo.svg",
                    FoundedDate = new DateTime(1986, 6, 28)
                },
                new Publisher { 
                    Name = "Nintendo", 
                    Description = "Japanese video game company",
                    Website = "https://www.nintendo.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/0/0d/Nintendo.svg",
                    FoundedDate = new DateTime(1889, 9, 23)
                },
                new Publisher { 
                    Name = "Activision Blizzard", 
                    Description = "American video game publisher",
                    Website = "https://www.activisionblizzard.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/0/0d/Activision_Blizzard.svg",
                    FoundedDate = new DateTime(2008, 7, 9)
                },
                new Publisher { 
                    Name = "Sony Interactive Entertainment", 
                    Description = "American video game publisher owned by Sony",
                    Website = "https://www.sie.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c1/PlayStation_Studios_logo.svg",
                    FoundedDate = new DateTime(1993, 11, 16)
                },
                new Publisher { 
                    Name = "Xbox Game Studios", 
                    Description = "American video game publisher owned by Microsoft",
                    Website = "https://www.xbox.com/", 
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/d/d7/Xbox_logo_%282019%29.svg",
                    FoundedDate = new DateTime(2000, 3, 21)
                }
            };

            await db.Publishers.AddRangeAsync(publishers);
            await db.SaveChangesAsync();
            Console.WriteLine($"Added {publishers.Count} publishers.");
        }

        private static async Task SeedGames(ApplicationDbContext db)
        {
            if (await db.Games.AnyAsync())
                return;

            var games = new List<Game>
            {
                new Game {
                    Title = "The Witcher 3: Wild Hunt",
                    Description = "An epic RPG with a mature, non-linear story set in an open world. You play as Geralt of Rivia, a monster hunter known as a Witcher.",
                    ReleaseDate = new DateTime(2015, 5, 19),
                    Price = 39.99m,
                    CoverImageUrl = "https://cdn.cloudflare.steamstatic.com/steam/apps/292030/header.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=c0i88t0Kacs",
                    IsActive = true
                },
                new Game {
                    Title = "Cyberpunk 2077",
                    Description = "An open-world, action-adventure RPG set in Night City, a megalopolis obsessed with power, glamour and body modification.",
                    ReleaseDate = new DateTime(2020, 12, 10),
                    Price = 59.99m,
                    CoverImageUrl = "https://cdn.cloudflare.steamstatic.com/steam/apps/1091500/header.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=8X2kIfS6fb8",
                    IsActive = true
                },
                new Game {
                    Title = "Red Dead Redemption 2",
                    Description = "An epic tale of life in America's unforgiving heartland. The game's vast and atmospheric world also provides the foundation for a brand new online multiplayer experience.",
                    ReleaseDate = new DateTime(2019, 11, 5),
                    Price = 59.99m,
                    CoverImageUrl = "https://cdn.cloudflare.steamstatic.com/steam/apps/1174180/header.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=eaW0tYpxyp0",
                    IsActive = true
                },
                new Game {
                    Title = "Elden Ring",
                    Description = "An action RPG developed by FromSoftware and written by George R. R. Martin. Rise, Tarnished, and be guided by grace to brandish the power of the Elden Ring and become an Elden Lord in the Lands Between.",
                    ReleaseDate = new DateTime(2022, 2, 25),
                    Price = 59.99m,
                    CoverImageUrl = "https://cdn.cloudflare.steamstatic.com/steam/apps/1245620/header.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=AKXiKBnzpBQ",
                    IsActive = true
                },
                new Game {
                    Title = "Grand Theft Auto V",
                    Description = "When a young street hustler, a retired bank robber, and a terrifying psychopath find themselves entangled with some of the most frightening and deranged elements of the criminal underworld, the U.S. government, and the entertainment industry, they must pull off a series of dangerous heists to survive.",
                    ReleaseDate = new DateTime(2013, 9, 17),
                    Price = 29.99m,
                    CoverImageUrl = "https://cdn.cloudflare.steamstatic.com/steam/apps/271590/header.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=hvoD7ehZPcM",
                    IsActive = true
                },
                new Game {
                    Title = "The Last of Us Part II",
                    Description = "Five years after their dangerous journey across the post-pandemic United States, Ellie and Joel have settled down in Jackson, Wyoming. When a violent event disrupts that peace, Ellie embarks on a relentless journey to carry out justice and find closure.",
                    ReleaseDate = new DateTime(2020, 6, 19),
                    Price = 59.99m,
                    CoverImageUrl = "https://image.api.playstation.com/vulcan/img/rnd/202010/2618/Y02l5ClOGXkwD1pqNwEqDUl0.png",
                    TrailerUrl = "https://www.youtube.com/watch?v=16RlfA39vhM",
                    IsActive = true
                },
                new Game {
                    Title = "God of War Ragnarök",
                    Description = "Embark on an epic journey as Kratos and Atreus struggle with holding on and letting go. Witness the changing dynamics of their relationship as they prepare for war.",
                    ReleaseDate = new DateTime(2022, 11, 9),
                    Price = 69.99m,
                    CoverImageUrl = "",
                    TrailerUrl = "https://www.youtube.com/watch?v=EE-4GvjKcfs",
                    IsActive = true
                },
                new Game {
                    Title = "The Legend of Zelda: Tears of the Kingdom",
                    Description = "An epic adventure across the land and skies of Hyrule. In this sequel to The Legend of Zelda: Breath of the Wild, you'll decide your own path through the sprawling landscapes of Hyrule and the mysterious islands floating in the vast skies above.",
                    ReleaseDate = new DateTime(2023, 5, 12),
                    Price = 69.99m,
                    CoverImageUrl = "https://assets.nintendo.com/image/upload/c_fill,w_1200/q_auto:best/f_auto/dpr_2.0/ncom/en_US/games/switch/t/the-legend-of-zelda-tears-of-the-kingdom-switch/hero",
                    TrailerUrl = "https://www.youtube.com/watch?v=2SNF4M_v7wc",
                    IsActive = true
                },
                new Game {
                    Title = "Horizon Forbidden West",
                    Description = "Join Aloy as she braves the Forbidden West, a majestic but dangerous frontier that conceals mysterious new threats.",
                    ReleaseDate = new DateTime(2022, 2, 18),
                    Price = 59.99m,
                    CoverImageUrl = "https://image.api.playstation.com/vulcan/ap/rnd/202107/3100/HO8vkO9pfXhwbHi5WHECQJdN.png",
                    TrailerUrl = "https://www.youtube.com/watch?v=Lq594XmpPBg",
                    IsActive = true
                },
                new Game {
                    Title = "Starfield",
                    Description = "The first new universe in 25 years from Bethesda Game Studios. In this next generation role-playing game set amongst the stars, create any character you want and explore with unparalleled freedom.",
                    ReleaseDate = new DateTime(2023, 9, 6),
                    Price = 69.99m,
                    CoverImageUrl = "https://cdn.cloudflare.steamstatic.com/steam/apps/1716740/header.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=Hs-6X2vKBkA",
                    IsActive = true
                },
                new Game {
                    Title = "Assassin's Creed Valhalla",
                    Description = "Become Eivor, a legendary Viking raider on a quest for glory. Explore England's Dark Ages as you raid your enemies, grow your settlement, and build your political power.",
                    ReleaseDate = new DateTime(2020, 11, 10),
                    Price = 59.99m,
                    CoverImageUrl = "https://cdn.cloudflare.steamstatic.com/steam/apps/2208920/header.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=ssrNcwxALS4",
                    IsActive = true
                },
                new Game {
                    Title = "Marvel's Spider-Man 2",
                    Description = "Spider-Men Peter Parker and Miles Morales face the ultimate test of strength inside and outside the mask as they fight to save the city, each other, and the ones they love, from Venom and the dangerous new symbiote threat.",
                    ReleaseDate = new DateTime(2023, 10, 20),
                    Price = 69.99m,
                    CoverImageUrl = "",
                    TrailerUrl = "https://www.youtube.com/watch?v=qIQ3xNqkVC4",
                    IsActive = true
                },
                new Game {
                    Title = "Hades",
                    Description = "Defy the god of the dead as you hack and slash out of the Underworld in this rogue-like dungeon crawler from the creators of Bastion, Transistor, and Pyre.",
                    ReleaseDate = new DateTime(2020, 9, 17),
                    Price = 24.99m,
                    CoverImageUrl = "https://cdn.cloudflare.steamstatic.com/steam/apps/1145360/header.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=Bz8l935Bv0Y",
                    IsActive = true
                },
                new Game {
                    Title = "Baldur's Gate 3",
                    Description = "Gather your party and return to the Forgotten Realms in a tale of fellowship and betrayal, sacrifice and survival, and the lure of absolute power.",
                    ReleaseDate = new DateTime(2023, 8, 3),
                    Price = 59.99m,
                    CoverImageUrl = "https://cdn.cloudflare.steamstatic.com/steam/apps/1086940/header.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=4itOTYup9_M",
                    IsActive = true
                },
                new Game {
                    Title = "Hollow Knight",
                    Description = "Forge your own path in Hollow Knight! An epic action adventure through a vast ruined kingdom of insects and heroes. Explore twisting caverns, battle tainted creatures and befriend bizarre bugs, all in a classic, hand-drawn 2D style.",
                    ReleaseDate = new DateTime(2017, 2, 24),
                    Price = 14.99m,
                    CoverImageUrl = "https://cdn.cloudflare.steamstatic.com/steam/apps/367520/header.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=UAO2urG23S4",
                    IsActive = true
                }
            };

            await db.Games.AddRangeAsync(games);
            await db.SaveChangesAsync();
            Console.WriteLine($"Added {games.Count} games.");

            var developers = await db.Developers.ToListAsync();
            var publishers = await db.Publishers.ToListAsync();
            var genres = await db.Genres.ToListAsync();
            var platforms = await db.Platforms.ToListAsync();

            var gameConnections = new List<object>();

            // The Witcher 3
            var witcher3 = games[0];
            var cdpr = developers.FirstOrDefault(d => d.Name == "CD Projekt Red");
            var cdprojekt = publishers.FirstOrDefault(p => p.Name == "CD Projekt");
            
            if (cdpr != null && cdprojekt != null)
            {
                gameConnections.Add(new GameDeveloper { GameId = witcher3.Id, DeveloperId = cdpr.Id });
                gameConnections.Add(new GamePublisher { GameId = witcher3.Id, PublisherId = cdprojekt.Id });
            }

            gameConnections.Add(new GameGenre { 
                GameId = witcher3.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "RPG")?.Id ?? 0 
            });
            gameConnections.Add(new GameGenre { 
                GameId = witcher3.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "Adventure")?.Id ?? 0 
            });
            gameConnections.Add(new GameGenre { 
                GameId = witcher3.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "Open World")?.Id ?? 0 
            });

            var pc = platforms.FirstOrDefault(p => p.Name == "PC");
            var ps4 = platforms.FirstOrDefault(p => p.Name == "PlayStation 4");
            var xboxOne = platforms.FirstOrDefault(p => p.Name == "Xbox One");
            var switch_ = platforms.FirstOrDefault(p => p.Name == "Nintendo Switch");
            var ps5 = platforms.FirstOrDefault(p => p.Name == "PlayStation 5");
            var xboxX = platforms.FirstOrDefault(p => p.Name == "Xbox Series X");

            if (pc != null) gameConnections.Add(new GamePlatform { GameId = witcher3.Id, PlatformId = pc.Id });
            if (ps4 != null) gameConnections.Add(new GamePlatform { GameId = witcher3.Id, PlatformId = ps4.Id });
            if (xboxOne != null) gameConnections.Add(new GamePlatform { GameId = witcher3.Id, PlatformId = xboxOne.Id });
            if (switch_ != null) gameConnections.Add(new GamePlatform { GameId = witcher3.Id, PlatformId = switch_.Id });
            if (ps5 != null) gameConnections.Add(new GamePlatform { GameId = witcher3.Id, PlatformId = ps5.Id });
            if (xboxX != null) gameConnections.Add(new GamePlatform { GameId = witcher3.Id, PlatformId = xboxX.Id });

            // Cyberpunk 2077
            var cyberpunk = games[1];
            
            if (cdpr != null && cdprojekt != null)
            {
                gameConnections.Add(new GameDeveloper { GameId = cyberpunk.Id, DeveloperId = cdpr.Id });
                gameConnections.Add(new GamePublisher { GameId = cyberpunk.Id, PublisherId = cdprojekt.Id });
            }

            gameConnections.Add(new GameGenre { 
                GameId = cyberpunk.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "RPG")?.Id ?? 0 
            });
            gameConnections.Add(new GameGenre { 
                GameId = cyberpunk.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "Action")?.Id ?? 0 
            });
            gameConnections.Add(new GameGenre { 
                GameId = cyberpunk.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "Open World")?.Id ?? 0 
            });
            gameConnections.Add(new GameGenre { 
                GameId = cyberpunk.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "Shooter")?.Id ?? 0 
            });

            if (pc != null) gameConnections.Add(new GamePlatform { GameId = cyberpunk.Id, PlatformId = pc.Id });
            if (ps4 != null) gameConnections.Add(new GamePlatform { GameId = cyberpunk.Id, PlatformId = ps4.Id });
            if (xboxOne != null) gameConnections.Add(new GamePlatform { GameId = cyberpunk.Id, PlatformId = xboxOne.Id });
            if (ps5 != null) gameConnections.Add(new GamePlatform { GameId = cyberpunk.Id, PlatformId = ps5.Id });
            if (xboxX != null) gameConnections.Add(new GamePlatform { GameId = cyberpunk.Id, PlatformId = xboxX.Id });

            // Red Dead Redemption 2
            var rdr2 = games[2];
            var rockstar = developers.FirstOrDefault(d => d.Name == "Rockstar Games");
            var rockstarPub = publishers.FirstOrDefault(p => p.Name == "Rockstar Games");
            
            if (rockstar != null && rockstarPub != null)
            {
                gameConnections.Add(new GameDeveloper { GameId = rdr2.Id, DeveloperId = rockstar.Id });
                gameConnections.Add(new GamePublisher { GameId = rdr2.Id, PublisherId = rockstarPub.Id });
            }

            gameConnections.Add(new GameGenre { 
                GameId = rdr2.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "Action")?.Id ?? 0 
            });
            gameConnections.Add(new GameGenre { 
                GameId = rdr2.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "Adventure")?.Id ?? 0 
            });
            gameConnections.Add(new GameGenre { 
                GameId = rdr2.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "Open World")?.Id ?? 0 
            });

            if (pc != null) gameConnections.Add(new GamePlatform { GameId = rdr2.Id, PlatformId = pc.Id });
            if (ps4 != null) gameConnections.Add(new GamePlatform { GameId = rdr2.Id, PlatformId = ps4.Id });
            if (xboxOne != null) gameConnections.Add(new GamePlatform { GameId = rdr2.Id, PlatformId = xboxOne.Id });

            // Elden Ring
            var eldenRing = games[3];
            var fromSoftware = developers.FirstOrDefault(d => d.Name == "FromSoftware");
            var bandaiNamco = publishers.FirstOrDefault(p => p.Name == "Bandai Namco");
            
            if (fromSoftware != null)
            {
                gameConnections.Add(new GameDeveloper { GameId = eldenRing.Id, DeveloperId = fromSoftware.Id });
            }

            gameConnections.Add(new GameGenre { 
                GameId = eldenRing.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "RPG")?.Id ?? 0 
            });
            gameConnections.Add(new GameGenre { 
                GameId = eldenRing.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "Action")?.Id ?? 0 
            });
            gameConnections.Add(new GameGenre { 
                GameId = eldenRing.Id, 
                GenreId = genres.FirstOrDefault(g => g.Name == "Open World")?.Id ?? 0 
            });

            if (pc != null) gameConnections.Add(new GamePlatform { GameId = eldenRing.Id, PlatformId = pc.Id });
            if (ps5 != null) gameConnections.Add(new GamePlatform { GameId = eldenRing.Id, PlatformId = ps5.Id });
            if (ps4 != null) gameConnections.Add(new GamePlatform { GameId = eldenRing.Id, PlatformId = ps4.Id });
            if (xboxOne != null) gameConnections.Add(new GamePlatform { GameId = eldenRing.Id, PlatformId = xboxOne.Id });
            if (xboxX != null) gameConnections.Add(new GamePlatform { GameId = eldenRing.Id, PlatformId = xboxX.Id });

            foreach (var connection in gameConnections)
            {
                if (connection is GameDeveloper gd)
                    db.GameDevelopers.Add(gd);
                else if (connection is GamePublisher gp)
                    db.GamePublishers.Add(gp);
                else if (connection is GameGenre gg)
                    db.GameGenres.Add(gg);
                else if (connection is GamePlatform gpl)
                    db.GamePlatforms.Add(gpl);
            }

            await db.SaveChangesAsync();
            Console.WriteLine($"Added connections between games and other entities.");
        }
    }
} 