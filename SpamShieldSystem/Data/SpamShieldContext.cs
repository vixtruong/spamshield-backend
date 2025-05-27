using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SpamShieldSystem.Models;

namespace SpamShieldSystem.Data;

public partial class SpamShieldContext : DbContext
{
    public SpamShieldContext()
    {
    }

    public SpamShieldContext(DbContextOptions<SpamShieldContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Email> Emails { get; set; }

    public virtual DbSet<EmailExplanation> EmailExplanations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-NKAC2572;Initial Catalog=SpamShield;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Email>(entity =>
        {
            entity.HasKey(e => e.EmailId).HasName("PK__Emails__7ED91AEF2BFD4F28");

            entity.Property(e => e.EmailId).HasColumnName("EmailID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmailDate).HasColumnType("datetime");
            entity.Property(e => e.Label).HasMaxLength(10);
            entity.Property(e => e.Sender).HasMaxLength(255);
            entity.Property(e => e.Subject).HasMaxLength(500);
        });

        modelBuilder.Entity<EmailExplanation>(entity =>
        {
            entity.HasKey(e => e.ExplanationId).HasName("PK__EmailExp__9257D8707357C003");

            entity.Property(e => e.ExplanationId).HasColumnName("ExplanationID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmailId).HasColumnName("EmailID");
            entity.Property(e => e.ExplanationMessage).HasColumnType("ntext");
            entity.Property(e => e.PredictedLabel)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.Email).WithMany(p => p.EmailExplanations)
                .HasForeignKey(d => d.EmailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Email_Explanation");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
