using System;
using System.Collections.Generic;
using Customs_Management_System.DBContexts.Models;
using Microsoft.EntityFrameworkCore;

namespace Customs_Management_System.DbContexts;

public partial class CMSDbContext : DbContext
{
    public CMSDbContext()
    {
    }

    public CMSDbContext(DbContextOptions<CMSDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Declaration> Declarations { get; set; }

    public virtual DbSet<Monitoring> Monitorings { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleDetail> RoleDetails { get; set; }

    public virtual DbSet<Shipment> Shipments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-PM9DUD3;Initial Catalog=CMS;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Declaration>(entity =>
        {
            entity.HasKey(e => e.DeclarationId).HasName("PK__Declarat__B4AA37DF38A887C1");

            entity.HasOne(d => d.User).WithMany(p => p.Declarations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserId_Declarations");
        });

        modelBuilder.Entity<Monitoring>(entity =>
        {
            entity.HasKey(e => e.MonitoringId).HasName("PK__Monitori__CAC3C05750C34837");

            entity.HasOne(d => d.Declaration).WithMany(p => p.Monitorings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeclarationId_Monitoring");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A386AFEDEF9");

            entity.HasOne(d => d.Declaration).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeclarationId_Payments");

            entity.HasOne(d => d.Product).WithMany(p => p.Payments).HasConstraintName("FK_ProductId_Payments");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserId_Payments");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CD6514EEA2");

            entity.HasOne(d => d.Declaration).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeclarationId_Products");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__D5BD48052383C6E8");

            entity.HasOne(d => d.User).WithMany(p => p.Reports)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserId_Reports");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A10776246");

            entity.Property(e => e.RoleId).ValueGeneratedNever();
        });

        modelBuilder.Entity<RoleDetail>(entity =>
        {
            entity.HasKey(e => e.RoleDetailsId).HasName("PK__RoleDeta__10F774D560C3C423");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoleId_RoleDetails");
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(e => e.ShipmentId).HasName("PK__Shipment__5CAD37ED3B5B02AF");

            entity.HasOne(d => d.Declaration).WithMany(p => p.Shipments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeclarationId_Shipment");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C10D344C6");

            entity.HasOne(d => d.UserRole).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoleId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
