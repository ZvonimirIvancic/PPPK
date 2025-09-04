using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmPictureTester.Models
{
    [Table("Slike")]
    public class Slika
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string NazivDatoteke { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Putanja { get; set; } = string.Empty;

        [StringLength(100)]
        public string? TipDatoteke { get; set; }

        public long? VelicinaDatoteke { get; set; }

        [StringLength(1000)]
        public string? Opis { get; set; }

        public DateTime DatumUpload { get; set; } = DateTime.Now;

        public DateTime? DatumPromjene { get; set; }

        [StringLength(50)]
        public string? Tag { get; set; }

        public bool IsActive { get; set; } = true;

        [NotMapped]
        public string VelicinaFormatirana => VelicinaDatoteke.HasValue ? FormatFileSize(VelicinaDatoteke.Value) : "Nepoznato";

        [NotMapped]
        public string FileExtension => Path.GetExtension(NazivDatoteke).ToLowerInvariant();

        [NotMapped]
        public bool IsImageFile => new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" }.Contains(FileExtension);

        private static string FormatFileSize(long bytes)
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
