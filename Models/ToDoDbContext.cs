using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace ToDo.Models;

/*
 This class is an Entity Framework Core (EF Core) DbContext 
 that manages the database connection and provides access to the Activity and User tables.
 */
public partial class ToDoDbContext : DbContext
{
    // Add a constructor that accepts DbContextOptions
    public ToDoDbContext(DbContextOptions<ToDoDbContext> options)
        : base(options)
    {
    }

    // EF Core uses DbSet properties to query and update database tables.
    public virtual DbSet<Activity> Activity { get; set; }
    public virtual DbSet<User> User { get; set; }

    // Override the OnModelCreating method to configure the model that EF Core uses to map the database schema.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_uca1400_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UserId, "userId");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Deadline)
                .HasColumnType("datetime")
                .HasColumnName("deadline");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("userId");
            entity.Property(e => e.Confirmed)
                .HasColumnType("tinyint(1)")
                .HasColumnName("confirmed")
                .HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.Activity)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Activity_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("firstName");
            entity.Property(e => e.HashedPassword)
                .HasMaxLength(44)
                .IsFixedLength()
                .HasColumnName("hashedPassword");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("lastName");
            entity.Property(e => e.NationalId)
                .HasMaxLength(13)
                .IsFixedLength()
                .HasColumnName("nationalId");
            entity.Property(e => e.Salt)
                .HasMaxLength(24)
                .IsFixedLength()
                .HasColumnName("salt");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    // Add a partial method to allow additional configuration in a separate file
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
