using Database.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database {
    public class LegoDbContext : DbContext {
        internal static string ConnectionString { set; private get; }

        public DbSet<LegoSetDb> Sets { get; set; }
        public DbSet<LegoPieceDb> Pieces { get; set; }
        public DbSet<LegoSetPieceDb> SetPieces { get; set; }
        public DbSet<LegoColorDb> Colors { get; set; }

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

            modelBuilder.Entity<LegoSetDb>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<LegoPieceDb>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<LegoSetPieceDb>()
                .HasKey(sp => new { sp.SetId, sp.PieceId });

            modelBuilder.Entity<LegoColorDb>()
                .HasKey(c => c.Id);


            /// Index
            modelBuilder.Entity<LegoSetDb>()
                .HasIndex(s => s.LegoCode)
                .IsUnique();

            modelBuilder.Entity<LegoPieceDb>()
                .HasIndex(p => p.Id)
                .IsUnique();

            modelBuilder.Entity<LegoSetPieceDb>()
                .HasIndex(sp => new { sp.SetId, sp.PieceId })
                .IsUnique();

            modelBuilder.Entity<LegoColorDb>()
                .HasIndex(c => c.Id)
                .IsUnique();

            /// Foreign key
            modelBuilder.Entity<LegoSetPieceDb>()
                .HasOne(sp => sp.Set)
                .WithMany(s => s.Pieces)
                .HasForeignKey(sp => sp.SetId);

            modelBuilder.Entity<LegoSetPieceDb>()
                .HasOne(sp => sp.Piece)
                .WithMany(s => s.Sets)
                .HasForeignKey(sp => sp.PieceId);

            modelBuilder.Entity<LegoPieceDb>()
                .HasOne(p => p.Color)
                .WithMany(c => c.Pieces)
                .HasForeignKey(p => p.ColorId);
        }
    }
}
