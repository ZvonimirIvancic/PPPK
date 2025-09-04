using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmPictureTester.Models
{
    [Table("AlbumSlike")]
    public class AlbumSlika
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Album")]
        public int AlbumId { get; set; }

        [ForeignKey("Slika")]
        public int SlikaId { get; set; }

        public DateTime DatumDodavanja { get; set; } = DateTime.Now;

        public int Redoslijed { get; set; } = 0;

        public virtual Album Album { get; set; } = null!;
        public virtual Slika Slika { get; set; } = null!;
    }
}
