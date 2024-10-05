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

    public virtual DbSet<ProductPrice> ProductPrices { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleDetail> RoleDetails { get; set; }

    public virtual DbSet<Shipment> Shipments { get; set; }

    public virtual DbSet<ShipmentDetail> ShipmentDetails { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-LCOF2LA; Initial Catalog=CMS;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Declaration>(entity =>
        {
            entity.HasKey(e => e.DeclarationId).HasName("PK__Declarat__B4AA37DF4DE8DAA8");

            entity.Property(e => e.DeclarationDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Declarations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserId_Declarations");
        });

        modelBuilder.Entity<Monitoring>(entity =>
        {
            entity.HasKey(e => e.MonitoringId).HasName("PK__Monitori__CAC3C057DC3123B6");

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
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A380AA141C0");

            entity.HasIndex(e => e.TransactionId, "UQ__Payments__55433A6A1ADCF7C5").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("BDT");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.ErrorCode).HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(255);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TransactionId).HasMaxLength(100);

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
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CD4ED9C19C");

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
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Weight).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Declaration).WithMany(p => p.Products)
                .HasForeignKey(d => d.DeclarationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeclarationId_Products");
        });

        modelBuilder.Entity<ProductPrice>(entity =>
        {
            entity.HasKey(e => e.PriceId).HasName("PK__ProductP__49575BAF6F01EDC7");

            entity.Property(e => e.Category).HasMaxLength(255);
            entity.Property(e => e.HsCode).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(100);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__D5BD4805A0A15E21");

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
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A1949A174");

            entity.Property(e => e.RoleId).ValueGeneratedNever();
            entity.Property(e => e.RoleName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RoleDetail>(entity =>
        {
            entity.HasKey(e => e.RoleDetailsId).HasName("PK__RoleDeta__10F774D5E2BF5788");

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
            entity.HasKey(e => e.ShipmentId).HasName("PK__Shipment__5CAD37ED3DBAE459");

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

        modelBuilder.Entity<ShipmentDetail>(entity =>
        {
            entity.HasKey(e => e.ShipmentDetailId).HasName("PK__Shipment__0471432049FEF821");

            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Port).HasMaxLength(100);
            entity.Property(e => e.Tax).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Vat)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("VAT");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C1D58471B");

            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.LoginCount).HasDefaultValue(0);
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
