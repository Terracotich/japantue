using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace japantune.Models;

public partial class JapanTuneContext : DbContext
{
    public JapanTuneContext()
    {
    }

    public JapanTuneContext(DbContextOptions<JapanTuneContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Car> Cars { get; set; }
    public virtual DbSet<Material> Materials { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Supplier> Suppliers { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-AIBSOGB\\SQLEXPRESS01;Initial Catalog=japan_tune;Integrated Security=True;Trust Server Certificate=True");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__car__3214EC077B4D4AFD");
            entity.ToTable("car");
            entity.Property(e => e.LicensePlate).HasMaxLength(10);
            entity.Property(e => e.Mark).HasMaxLength(30);
            entity.Property(e => e.Model).HasMaxLength(30);
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Cars)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade) // Изменено на каскадное удаление
                .HasConstraintName("FK__car__user_id__5535A963");
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__material__3214EC0721EFE2EF");
            entity.ToTable("material");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.Title).HasMaxLength(30);

            entity.HasOne(d => d.Supplier).WithMany(p => p.Materials)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__material__suppli__5FB337D6");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orders__3214EC0750B6EFA3");
            entity.ToTable("orders");
            entity.Property(e => e.MaterialId).HasColumnName("material_id");
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.Status).HasMaxLength(12);
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Material).WithMany(p => p.Orders)
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orders__material__70DDC3D8");

            entity.HasOne(d => d.Payment).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.Cascade) // Изменено на каскадное удаление
                .HasConstraintName("FK__orders__payment___6EF57B66");

            entity.HasOne(d => d.Review).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ReviewId)
                .HasConstraintName("FK__orders__review_i__71D1E811");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade) // Изменено на каскадное удаление
                .HasConstraintName("FK__orders__user_id__6FE99F9F");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payment__3214EC07ECF1F8A2");
            entity.ToTable("payment");
            entity.Property(e => e.PayMethod).HasMaxLength(15);
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade) // Изменено на каскадное удаление
                .HasConstraintName("FK__payment__user_id__59FA5E80");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__review__3214EC07348C4A01");
            entity.ToTable("review");
            entity.Property(e => e.ReviewDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade) // Изменено на каскадное удаление
                .HasConstraintName("FK__review__user_id__52593CB8");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__roles__3214EC074307D6E2");
            entity.ToTable("roles");
            entity.Property(e => e.Title).HasMaxLength(15);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__supplier__3214EC07C2CF6901");
            entity.ToTable("supplier");
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3214EC0730D27DE4");
            entity.ToTable("users");
            entity.HasIndex(e => e.ClientLogin, "UQ__users__5BF1A3114C522800").IsUnique();
            entity.HasIndex(e => e.PhoneNumber, "UQ__users__85FB4E3891EB82BA").IsUnique();
            entity.Property(e => e.ClientLogin).HasMaxLength(20);
            entity.Property(e => e.FirstName).HasMaxLength(30);
            entity.Property(e => e.LastName).HasMaxLength(30);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.SurName).HasMaxLength(30);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__users__role_id__4D94879B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}