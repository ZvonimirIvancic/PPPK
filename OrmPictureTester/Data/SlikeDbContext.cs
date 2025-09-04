using Microsoft.EntityFrameworkCore;
using OrmPictureTester.Models;
using Microsoft.EntityFrameworkCore;
using OrmPictureTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmPictureTester.Data
{
    public class SlikeDbContext : DbContext
    {
        public DbSet<Slika> Slike { get; set; }
        public DbSet<Album> Albumi { get; set; }
        public DbSet<AlbumSlika> AlbumSlike { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-AVECIVB;Database=SlikeTesterDb;User Id=sa;Password=SQL;TrustServerCertificate=True;MultipleActiveResultSets=true;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AlbumSlika>()
                .HasOne(albumSlika => albumSlika.Album)
                .WithMany(album => album.AlbumSlike)
                .HasForeignKey(albumSlika => albumSlika.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AlbumSlika>()
                .HasOne(albumSlika => albumSlika.Slika)
                .WithMany()
                .HasForeignKey(albumSlika => albumSlika.SlikaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Slika>()
                .HasIndex(slika => slika.NazivDatoteke);

            modelBuilder.Entity<Slika>()
                .HasIndex(slika => slika.DatumUpload);

            modelBuilder.Entity<Album>()
                .HasIndex(album => album.Naziv);

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Album>().HasData(
                new Album { Id = 1, Naziv = "Priroda", Opis = "Fotografije prirode i krajolika", DatumKreiranja = DateTime.Now.AddDays(-30) },
                new Album { Id = 2, Naziv = "Portretiranje", Opis = "Portreti ljudi i životinja", DatumKreiranja = DateTime.Now.AddDays(-20) },
                new Album { Id = 3, Naziv = "Arhitektura", Opis = "Fotografije građevina i spomenika", DatumKreiranja = DateTime.Now.AddDays(-10) }
            );
            var slike = new[]
            {
                new Slika { Id = 1, NazivDatoteke = "planine_zalazak.jpg", Putanja = "/uploads/planine_zalazak.jpg", TipDatoteke = "image/jpeg", VelicinaDatoteke = 2048576, Opis = "Zalazak sunca u planinama", DatumUpload = DateTime.Now.AddDays(-25), Tag = "priroda" },
                new Slika { Id = 2, NazivDatoteke = "portret_covek.jpg", Putanja = "/uploads/portret_covek.jpg", TipDatoteke = "image/jpeg", VelicinaDatoteke = 1536000, Opis = "Portret muškarca", DatumUpload = DateTime.Now.AddDays(-18), Tag = "portret" },
                new Slika { Id = 3, NazivDatoteke = "katedrala_zagreb.jpg", Putanja = "/uploads/katedrala_zagreb.jpg", TipDatoteke = "image/jpeg", VelicinaDatoteke = 3072000, Opis = "Zagrebačka katedrala", DatumUpload = DateTime.Now.AddDays(-15), Tag = "arhitektura" },
                new Slika { Id = 4, NazivDatoteke = "more_plaža.jpg", Putanja = "/uploads/more_plaža.jpg", TipDatoteke = "image/jpeg", VelicinaDatoteke = 2560000, Opis = "Kristalno čisto more", DatumUpload = DateTime.Now.AddDays(-12), Tag = "priroda" },
                new Slika { Id = 5, NazivDatoteke = "dijete_smijeh.jpg", Putanja = "/uploads/dijete_smijeh.jpg", TipDatoteke = "image/jpeg", VelicinaDatoteke = 1280000, Opis = "Dijete se smije", DatumUpload = DateTime.Now.AddDays(-8), Tag = "portret" },
                new Slika { Id = 6, NazivDatoteke = "most_noću.jpg", Putanja = "/uploads/most_noću.jpg", TipDatoteke = "image/jpeg", VelicinaDatoteke = 1792000, Opis = "Most osvijetljen noću", DatumUpload = DateTime.Now.AddDays(-5), Tag = "arhitektura" }
            };

            modelBuilder.Entity<Slika>().HasData(slike);

            modelBuilder.Entity<AlbumSlika>().HasData(
                new AlbumSlika { Id = 1, AlbumId = 1, SlikaId = 1, DatumDodavanja = DateTime.Now.AddDays(-24), Redoslijed = 1 },
                new AlbumSlika { Id = 2, AlbumId = 1, SlikaId = 4, DatumDodavanja = DateTime.Now.AddDays(-11), Redoslijed = 2 },
                new AlbumSlika { Id = 3, AlbumId = 2, SlikaId = 2, DatumDodavanja = DateTime.Now.AddDays(-17), Redoslijed = 1 },
                new AlbumSlika { Id = 4, AlbumId = 2, SlikaId = 5, DatumDodavanja = DateTime.Now.AddDays(-7), Redoslijed = 2 },
                new AlbumSlika { Id = 5, AlbumId = 3, SlikaId = 3, DatumDodavanja = DateTime.Now.AddDays(-14), Redoslijed = 1 },
                new AlbumSlika { Id = 6, AlbumId = 3, SlikaId = 6, DatumDodavanja = DateTime.Now.AddDays(-4), Redoslijed = 2 }
            );
        }
    }
}