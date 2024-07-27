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
        => optionsBuilder.UseSqlServer("Data Source=localhost\\sqlexpress;Initial Catalog=CMS;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Declaration>(entity =>
        {
            entity.HasKey(e => e.DeclarationId).HasName("PK__Declarat__B4AA37DFD80B84E5");

            entity.Property(e => e.DeclarationDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Declarations)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoleId_Declarations");

            entity.HasOne(d => d.User).WithMany(p => p.Declarations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserId_Declarations");
        });

        modelBuilder.Entity<Monitoring>(entity =>
        {
            entity.HasKey(e => e.MonitoringId).HasName("PK__Monitori__CAC3C0578827507A");

            entity.ToTable("Monitoring");

            entity.Property(e => e.ArrivalDate).HasColumnType("datetime");
            entity.Property(e => e.DepartureDate).HasColumnType("datetime");
            entity.Property(e => e.MethodOfShipment)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PortOfDeparture)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PortOfDestination)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Declaration).WithMany(p => p.Monitorings)
                .HasForeignKey(d => d.DeclarationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeclarationId_Monitoring");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A3896ADB98F");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Declaration).WithMany(p => p.Payments)
                .HasForeignKey(d => d.DeclarationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeclarationId_Payments");

            entity.HasOne(d => d.Product).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ProductId_Payments");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserId_Payments");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CD45AD605F");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CountryOfOrigin)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Hscode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("HSCode");
            entity.Property(e => e.ProductName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Weight).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Declaration).WithMany(p => p.Products)
                .HasForeignKey(d => d.DeclarationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeclarationId_Products");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__D5BD4805A2E6DE6E");

            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.ReportType)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Reports)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserId_Reports");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AB1E6EE87");

            entity.Property(e => e.RoleId).ValueGeneratedNever();
            entity.Property(e => e.RoleName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RoleDetail>(entity =>
        {
            entity.HasKey(e => e.RoleDetailsId).HasName("PK__RoleDeta__10F774D5CDDC7915");

            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ContractNumber)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.RoleDetails)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoleId_RoleDetails");
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(e => e.ShipmentId).HasName("PK__Shipment__5CAD37EDCC779865");

            entity.ToTable("Shipment");

            entity.Property(e => e.ArrivalDate).HasColumnType("datetime");
            entity.Property(e => e.DepartureDate).HasColumnType("datetime");
            entity.Property(e => e.MethodOfShipment)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PortOfDeparture)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PortOfDestination)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Declaration).WithMany(p => p.Shipments)
                .HasForeignKey(d => d.DeclarationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeclarationId_Shipment");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C1F494ABD");

            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.UserRole).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoleId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
