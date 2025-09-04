using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrmPictureTester.Data;
using OrmPictureTester.Models;

namespace OrmPictureTester
{
    class Program
    {
        private static SlikeDbContext? _context;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== ORM Slike Tester Application ===");
            Console.WriteLine("Testing Entity Framework Code First with Images");
            Console.WriteLine();

            try
            {
                _context = new SlikeDbContext();
                await InitializeDatabase();
                await RunMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
            finally
            {
                _context?.Dispose();
            }
        }

        static async Task InitializeDatabase()
        {
            Console.WriteLine("🔄 Initializing database...");
            await _context!.Database.EnsureCreatedAsync();
            Console.WriteLine("✅ Database initialized successfully!");
            Console.WriteLine($"📊 Connection: {_context.Database.GetConnectionString()}");
            Console.WriteLine();
        }

        static async Task RunMainMenu()
        {
            while (true)
            {
                Console.WriteLine("=== MAIN MENU ===");
                Console.WriteLine("1. View all images");
                Console.WriteLine("2. View all albums");
                Console.WriteLine("3. Add new image");
                Console.WriteLine("4. Add new album");
                Console.WriteLine("5. Add image to album");
                Console.WriteLine("6. Search images");
                Console.WriteLine("7. Show database statistics");
                Console.WriteLine("8. Delete image");
                Console.WriteLine("9. Exit");
                Console.Write("Choose option (1-9): ");
                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        await ViewAllImages();
                        break;
                    case "2":
                        await ViewAllAlbums();
                        break;
                    case "3":
                        await AddNewImage();
                        break;
                    case "4":
                        await AddNewAlbum();
                        break;
                    case "5":
                        await AddImageToAlbum();
                        break;
                    case "6":
                        await SearchImages();
                        break;
                    case "7":
                        await ShowDatabaseStatistics();
                        break;
                    case "8":
                        await DeleteImage();
                        break;
                    case "9":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static async Task ViewAllImages()
        {
            Console.WriteLine("📷 All images in database:");
            Console.WriteLine(new string('=', 80));

            try
            {
                var slike = await _context!.Slike
                    .Where(s => s.IsActive)
                    .OrderByDescending(s => s.DatumUpload)
                    .ToListAsync();

                if (!slike.Any())
                {
                    Console.WriteLine("No images found in database.");
                    return;
                }

                foreach (var slika in slike)
                {
                    Console.WriteLine($"🖼️  {slika.NazivDatoteke}");
                    Console.WriteLine($"   ID: {slika.Id}");
                    Console.WriteLine($"   Size: {slika.VelicinaFormatirana}");
                    Console.WriteLine($"   Type: {slika.TipDatoteke}");
                    Console.WriteLine($"   Uploaded: {slika.DatumUpload:yyyy-MM-dd HH:mm}");
                    Console.WriteLine($"   Path: {slika.Putanja}");
                    if (!string.IsNullOrEmpty(slika.Opis))
                        Console.WriteLine($"   Description: {slika.Opis}");
                    if (!string.IsNullOrEmpty(slika.Tag))
                        Console.WriteLine($"   Tag: {slika.Tag}");
                    Console.WriteLine($"   Is Image: {(slika.IsImageFile ? "Yes" : "No")}");
                    Console.WriteLine();
                }

                Console.WriteLine($"Total: {slike.Count} images");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving images: {ex.Message}");
            }
        }

        static async Task ViewAllAlbums()
        {
            Console.WriteLine("📁 All albums in database:");
            Console.WriteLine(new string('=', 80));

            try
            {
                var albumi = await _context!.Albumi
                    .Include(a => a.AlbumSlike)
                        .ThenInclude(rel => rel.Slika)
                    .OrderByDescending(a => a.DatumKreiranja)
                    .ToListAsync();

                if (!albumi.Any())
                {
                    Console.WriteLine("No albums found in database.");
                    return;
                }

                foreach (var album in albumi)
                {
                    Console.WriteLine($"📁 {album.Naziv}");
                    Console.WriteLine($"   ID: {album.Id}");
                    Console.WriteLine($"   Created: {album.DatumKreiranja:yyyy-MM-dd HH:mm}");
                    Console.WriteLine($"   Public: {(album.IsPublic ? "Yes" : "No")}");
                    Console.WriteLine($"   Images: {album.BrojSlika}");
                    if (!string.IsNullOrEmpty(album.Opis))
                        Console.WriteLine($"   Description: {album.Opis}");
                    if (album.AlbumSlike.Any())
                    {
                        Console.WriteLine("   📷 Images in album:");
                        foreach (var rel in album.AlbumSlike.OrderBy(r => r.Redoslijed))
                        {
                            Console.WriteLine($"      - {rel.Slika.NazivDatoteke} ({rel.Slika.VelicinaFormatirana})");
                        }
                    }
                    Console.WriteLine();
                }

                Console.WriteLine($"Total: {albumi.Count} albums");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving albums: {ex.Message}");
            }
        }

        static async Task AddNewImage()
        {
            Console.WriteLine("➕ Adding new image:");
            Console.WriteLine(new string('-', 40));

            try
            {
                Console.Write("Enter file name: ");
                var fileName = Console.ReadLine();
                if (string.IsNullOrEmpty(fileName))
                {
                    Console.WriteLine("❌ File name cannot be empty");
                    return;
                }

                Console.Write("Enter file path: ");
                var filePath = Console.ReadLine();
                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("❌ File path cannot be empty");
                    return;
                }

                Console.Write("Enter description (optional): ");
                var description = Console.ReadLine();
                Console.Write("Enter tag (optional): ");
                var tag = Console.ReadLine();

                var random = new Random();
                var fileSize = random.Next(512000, 5000000); // 512KB to 5MB

                var newSlika = new Slika
                {
                    NazivDatoteke = fileName,
                    Putanja = filePath,
                    TipDatoteke = GetMimeType(fileName),
                    VelicinaDatoteke = fileSize,
                    Opis = description,
                    Tag = tag,
                    DatumUpload = DateTime.Now
                };

                _context!.Slike.Add(newSlika);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Image '{fileName}' added successfully!");
                Console.WriteLine($"   ID: {newSlika.Id}");
                Console.WriteLine($"   Size: {newSlika.VelicinaFormatirana}");
                Console.WriteLine($"   Type: {newSlika.TipDatoteke}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding image: {ex.Message}");
            }
        }

        static async Task AddNewAlbum()
        {
            Console.WriteLine("➕ Adding new album:");
            Console.WriteLine(new string('-', 40));

            try
            {
                Console.Write("Enter album name: ");
                var name = Console.ReadLine();
                if (string.IsNullOrEmpty(name))
                {
                    Console.WriteLine("❌ Album name cannot be empty");
                    return;
                }

                Console.Write("Enter description (optional): ");
                var description = Console.ReadLine();

                Console.Write("Is public? (y/n): ");
                var isPublicInput = Console.ReadLine();
                var isPublic = isPublicInput?.ToLower() == "y";

                var newAlbum = new Album
                {
                    Naziv = name,
                    Opis = description,
                    IsPublic = isPublic,
                    DatumKreiranja = DateTime.Now
                };

                _context!.Albumi.Add(newAlbum);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Album '{name}' added successfully!");
                Console.WriteLine($"   ID: {newAlbum.Id}");
                Console.WriteLine($"   Public: {(isPublic ? "Yes" : "No")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding album: {ex.Message}");
            }
        }

        static async Task AddImageToAlbum()
        {
            Console.WriteLine("🔗 Adding image to album:");
            Console.WriteLine(new string('-', 40));

            try
            {
                var albums = await _context!.Albumi.ToListAsync();
                if (!albums.Any())
                {
                    Console.WriteLine("❌ No albums available. Create an album first.");
                    return;
                }

                Console.WriteLine("Available albums:");
                foreach (var a in albums)
                    Console.WriteLine($"  {a.Id}: {a.Naziv}");

                Console.Write("Enter album ID: ");
                if (!int.TryParse(Console.ReadLine(), out int albumId))
                {
                    Console.WriteLine("❌ Invalid album ID");
                    return;
                }

                var images = await _context.Slike.Where(s => s.IsActive).ToListAsync();
                if (!images.Any())
                {
                    Console.WriteLine("❌ No images available. Add an image first.");
                    return;
                }

                Console.WriteLine("Available images:");
                foreach (var img in images)
                    Console.WriteLine($"  {img.Id}: {img.NazivDatoteke}");

                Console.Write("Enter image ID: ");
                if (!int.TryParse(Console.ReadLine(), out int imageId))
                {
                    Console.WriteLine("❌ Invalid image ID");
                    return;
                }

                var exists = await _context.AlbumSlike
                    .FirstOrDefaultAsync(rel => rel.AlbumId == albumId && rel.SlikaId == imageId);

                if (exists != null)
                {
                    Console.WriteLine("❌ This image is already in the album");
                    return;
                }

                var maxOrder = await _context.AlbumSlike
                    .Where(rel => rel.AlbumId == albumId)
                    .MaxAsync(rel => (int?)rel.Redoslijed) ?? 0;

                var albumSlika = new AlbumSlika
                {
                    AlbumId = albumId,
                    SlikaId = imageId,
                    DatumDodavanja = DateTime.Now,
                    Redoslijed = maxOrder + 1
                };

                _context.AlbumSlike.Add(albumSlika);
                await _context.SaveChangesAsync();

                Console.WriteLine("✅ Image added to album successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding image to album: {ex.Message}");
            }
        }

        static async Task SearchImages()
        {
            Console.WriteLine("🔍 Search images:");
            Console.WriteLine(new string('-', 40));

            Console.Write("Enter search term (filename, description, or tag): ");
            var term = Console.ReadLine();
            if (string.IsNullOrEmpty(term))
            {
                Console.WriteLine("❌ Search term cannot be empty");
                return;
            }

            try
            {
                var results = await _context!.Slike
                    .Where(s => s.IsActive &&
                               (s.NazivDatoteke.Contains(term) ||
                                (s.Opis != null && s.Opis.Contains(term)) ||
                                (s.Tag != null && s.Tag.Contains(term))))
                    .OrderByDescending(s => s.DatumUpload)
                    .ToListAsync();

                if (!results.Any())
                {
                    Console.WriteLine($"❌ No images found matching '{term}'");
                    return;
                }

                Console.WriteLine($"🔍 Found {results.Count} images matching '{term}':");
                Console.WriteLine();
                foreach (var slika in results)
                {
                    Console.WriteLine($"📷 {slika.NazivDatoteke}");
                    Console.WriteLine($"   ID: {slika.Id} | Size: {slika.VelicinaFormatirana} | Uploaded: {slika.DatumUpload:yyyy-MM-dd}");
                    if (!string.IsNullOrEmpty(slika.Opis))
                        Console.WriteLine($"   Description: {slika.Opis}");
                    if (!string.IsNullOrEmpty(slika.Tag))
                        Console.WriteLine($"   Tag: {slika.Tag}");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error searching images: {ex.Message}");
            }
        }

        static async Task ShowDatabaseStatistics()
        {
            Console.WriteLine("📊 Database Statistics:");
            Console.WriteLine(new string('=', 50));

            try
            {
                var totalImages = await _context!.Slike.CountAsync(s => s.IsActive);
                var totalAlbums = await _context.Albumi.CountAsync();
                var totalConnections = await _context.AlbumSlike.CountAsync();
                var totalSize = await _context.Slike
                    .Where(s => s.IsActive && s.VelicinaDatoteke.HasValue)
                    .SumAsync(s => s.VelicinaDatoteke!.Value);

                Console.WriteLine($"📷 Total Images: {totalImages}");
                Console.WriteLine($"📁 Total Albums: {totalAlbums}");
                Console.WriteLine($"🔗 Connections: {totalConnections}");
                Console.WriteLine($"💾 Total Storage: {FormatFileSize(totalSize)}");
                Console.WriteLine();

                var byType = await _context.Slike
                    .Where(s => s.IsActive)
                    .GroupBy(s => s.TipDatoteke)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToListAsync();

                if (byType.Any())
                {
                    Console.WriteLine("📊 Images by Type:");
                    foreach (var t in byType)
                        Console.WriteLine($"   {t.Type ?? "Unknown"}: {t.Count}");
                    Console.WriteLine();
                }

                var byTag = await _context.Slike
                    .Where(s => s.IsActive && s.Tag != null)
                    .GroupBy(s => s.Tag)
                    .Select(g => new { Tag = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                if (byTag.Any())
                {
                    Console.WriteLine("🏷️  Images by Tag:");
                    foreach (var t in byTag)
                        Console.WriteLine($"   {t.Tag}: {t.Count}");
                    Console.WriteLine();
                }

                var recent = await _context.Slike
                    .Where(s => s.IsActive)
                    .OrderByDescending(s => s.DatumUpload)
                    .Take(3)
                    .Select(s => new { s.NazivDatoteke, s.DatumUpload })
                    .ToListAsync();

                if (recent.Any())
                {
                    Console.WriteLine("🕒 Recently Added:");
                    foreach (var r in recent)
                        Console.WriteLine($"   {r.NazivDatoteke} - {r.DatumUpload:yyyy-MM-dd HH:mm}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting statistics: {ex.Message}");
            }
        }

        static async Task DeleteImage()
        {
            Console.WriteLine("🗑️  Delete image:");
            Console.WriteLine(new string('-', 40));

            try
            {
                var images = await _context!.Slike.Where(s => s.IsActive).ToListAsync();
                if (!images.Any())
                {
                    Console.WriteLine("❌ No images available to delete");
                    return;
                }

                Console.WriteLine("Available images:");
                foreach (var img in images.Take(10))
                    Console.WriteLine($"  {img.Id}: {img.NazivDatoteke}");

                Console.Write("Enter image ID to delete: ");
                if (!int.TryParse(Console.ReadLine(), out int id))
                {
                    Console.WriteLine("❌ Invalid ID");
                    return;
                }

                var toDelete = await _context.Slike.FindAsync(id);
                if (toDelete == null)
                {
                    Console.WriteLine("❌ Image not found");
                    return;
                }

                Console.Write($"Are you sure you want to delete '{toDelete.NazivDatoteke}'? (yes/no): ");
                if (Console.ReadLine()?.ToLower() != "yes")
                {
                    Console.WriteLine("❌ Operation cancelled");
                    return;
                }

                toDelete.IsActive = false;
                toDelete.DatumPromjene = DateTime.Now;
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Image '{toDelete.NazivDatoteke}' deleted successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting image: {ex.Message}");
            }
        }

        static string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}