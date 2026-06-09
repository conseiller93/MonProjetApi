using Microsoft.EntityFrameworkCore;
using ProjetApiNet.Models;

namespace ProjetApiNet.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<ZoneMiniere> ZoneMinieres => Set<ZoneMiniere>();
    public DbSet<GroupeTransport> GroupesTransport => Set<GroupeTransport>();
    public DbSet<Camion> Camions => Set<Camion>();
    public DbSet<Chargement> Chargements => Set<Chargement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasIndex(u => u.Identifiant).IsUnique();
            entity.Property(u => u.Identifiant).HasMaxLength(100);
            entity.Property(u => u.MotDePasseHash).HasMaxLength(256);
        });

        modelBuilder.Entity<ZoneMiniere>(entity =>
        {
            entity.HasIndex(z => z.Nom).IsUnique();
            entity.Property(z => z.Nom).HasMaxLength(150);
            entity.Property(z => z.DistanceDepotZone).HasPrecision(10, 2);
            entity.Property(z => z.DistanceAllerRetour).HasPrecision(10, 2);
            entity.Property(z => z.Tarification).HasPrecision(18, 2);
        });

        modelBuilder.Entity<GroupeTransport>(entity =>
        {
            entity.HasIndex(g => g.Nom).IsUnique();
            entity.Property(g => g.Nom).HasMaxLength(150);

            entity.HasOne(g => g.SuperviseurGroupe)
                .WithMany(u => u.GroupesSupervises)
                .HasForeignKey(g => g.SuperviseurGroupeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(g => g.ZoneMiniere)
                .WithMany(z => z.GroupesTransport)
                .HasForeignKey(g => g.ZoneMiniereId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Camion>(entity =>
        {
            entity.HasIndex(c => c.Immatriculation).IsUnique();
            entity.Property(c => c.Immatriculation).HasMaxLength(20);
            entity.Property(c => c.Modele).HasMaxLength(100);
            entity.Property(c => c.Kilometrage).HasPrecision(12, 2);

            entity.HasOne(c => c.Chauffeur)
                .WithMany(u => u.CamionsConduits)
                .HasForeignKey(c => c.ChauffeurId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.GroupeTransport)
                .WithMany(g => g.Camions)
                .HasForeignKey(c => c.GroupeTransportId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Chargement>(entity =>
        {
            entity.Property(c => c.CarburantCalcule).HasPrecision(10, 2);

            entity.HasOne(c => c.Camion)
                .WithMany(c => c.Chargements)
                .HasForeignKey(c => c.CamionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
