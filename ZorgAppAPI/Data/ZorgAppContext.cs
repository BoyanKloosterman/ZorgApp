using Microsoft.EntityFrameworkCore;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Data
{
    public class ZorgAppContext : DbContext
    {
        public ZorgAppContext(DbContextOptions<ZorgAppContext> options) : base(options)
        {
        }

        public DbSet<Arts> Artsen { get; set; }
        public DbSet<OuderVoogd> OuderVoogden { get; set; }
        public DbSet<Patient> Patienten { get; set; }
        public DbSet<Traject> Trajecten { get; set; }
        public DbSet<ZorgMoment> ZorgMomenten { get; set; }
        public DbSet<Traject_ZorgMoment> Traject_ZorgMomenten { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure table names to match SQL schema
            modelBuilder.Entity<Arts>().ToTable("Arts");
            modelBuilder.Entity<OuderVoogd>().ToTable("OuderVoogd");
            modelBuilder.Entity<Patient>().ToTable("Patient");
            modelBuilder.Entity<Traject>().ToTable("Traject");
            modelBuilder.Entity<ZorgMoment>().ToTable("ZorgMoment");
            modelBuilder.Entity<Traject_ZorgMoment>().ToTable("Traject_ZorgMoment");

            // Configure primary keys
            modelBuilder.Entity<Arts>().HasKey(a => a.ID);
            modelBuilder.Entity<OuderVoogd>().HasKey(o => o.ID);
            modelBuilder.Entity<Patient>().HasKey(p => p.ID);
            modelBuilder.Entity<Traject>().HasKey(t => t.ID);
            modelBuilder.Entity<ZorgMoment>().HasKey(z => z.ID);
            modelBuilder.Entity<Traject_ZorgMoment>().HasKey(tz => new { tz.TrajectID, tz.ZorgMomentID });

            // Configure relationships
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.OuderVoogd)
                .WithMany(o => o.Patienten)
                .HasForeignKey(p => p.OuderVoogd_ID);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Traject)
                .WithMany(t => t.Patienten)
                .HasForeignKey(p => p.TrajectID);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Arts)
                .WithMany(a => a.Patienten)
                .HasForeignKey(p => p.ArtsID);

            modelBuilder.Entity<Traject_ZorgMoment>()
                .HasOne(tz => tz.Traject)
                .WithMany(t => t.TrajectZorgMomenten)
                .HasForeignKey(tz => tz.TrajectID);

            modelBuilder.Entity<Traject_ZorgMoment>()
                .HasOne(tz => tz.ZorgMoment)
                .WithMany(z => z.TrajectZorgMomenten)
                .HasForeignKey(tz => tz.ZorgMomentID);
        }
    }
}
