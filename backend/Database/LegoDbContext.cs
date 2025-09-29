using Database.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database {
    internal class LegoDbContext : DbContext {
        internal static string ConnectionString { set; private get; }

        public DbSet<LegoSet> Sets { get; set; }
        public DbSet<LegoPiece> Pieces { get; set; }
        public DbSet<LegoSetPiece> SetPieces { get; set; }
        public DbSet<LegoColor> Colors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            try {
                optionsBuilder.UseMySql(connectionString: ConnectionString, ServerVersion.AutoDetect(ConnectionString));
            } catch (Exception ex) {
                MyLogger.Log.Fatal($"Connection to DB error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            /// Primary keys

            modelBuilder.Entity<LegoSet>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<LegoPiece>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<LegoSetPiece>()
                .HasKey(sp => new { sp.SetId, sp.PieceId });

            modelBuilder.Entity<LegoColor>()
                .HasKey(c => c.Id);


            /// Index
            modelBuilder.Entity<LegoSet>()
                .HasIndex(s => s.LegoCode)
                .IsUnique();

            modelBuilder.Entity<LegoPiece>()
                .HasIndex(p => p.Id)
                .IsUnique();

            modelBuilder.Entity<LegoSetPiece>()
                .HasIndex(sp => new { sp.SetId, sp.PieceId })
                .IsUnique();

            modelBuilder.Entity<LegoColor>()
                .HasIndex(c => c.Id)
                .IsUnique();

            /// Foreign key
            modelBuilder.Entity<LegoSetPiece>()
                .HasOne(sp => sp.Set)
                .WithMany(s => s.Pieces)
                .HasForeignKey(sp => sp.SetId);

            modelBuilder.Entity<LegoSetPiece>()
                .HasOne(sp => sp.Piece)
                .WithMany(s => s.Sets)
                .HasForeignKey(sp => sp.PieceId);

            modelBuilder.Entity<LegoPiece>()
                .HasOne(p => p.Color)
                .WithMany(c => c.Pieces)
                .HasForeignKey(p => p.ColorId);
        }
    }
}
