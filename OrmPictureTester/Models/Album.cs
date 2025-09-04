using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmPictureTester.Models
{
    [Table("Albumi")]
    public class Album
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Naziv { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Opis { get; set; }

        public DateTime DatumKreiranja { get; set; } = DateTime.Now;

        public bool IsPublic { get; set; } = true;

        public virtual ICollection<AlbumSlika> AlbumSlike { get; set; } = new List<AlbumSlika>();

        [NotMapped]
        public int BrojSlika => AlbumSlike?.Count ?? 0;
    }
}
