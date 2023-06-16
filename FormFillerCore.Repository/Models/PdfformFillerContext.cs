using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FormFillerCore.Repository.RepModels;

public partial class PdfformFillerContext : DbContext
{
    public PdfformFillerContext()
    {
    }

    public PdfformFillerContext(DbContextOptions<PdfformFillerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DataMapChildObject> DataMapChildObjects { get; set; }

    public virtual DbSet<Form> Forms { get; set; }

    public virtual DbSet<FormDataMap> FormDataMaps { get; set; }

    public virtual DbSet<FormDataType> FormDataTypes { get; set; }

    public virtual DbSet<FormRequest> FormRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataMapChildObject>(entity =>
        {
            entity.HasKey(e => e.ChildObjectId);

            entity.Property(e => e.ChildObjectId).HasColumnName("ChildObjectID");

            entity.HasOne(d => d.ParentObjectNavigation).WithMany(p => p.DataMapChildObjects)
                .HasForeignKey(d => d.ParentObject)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DataMapChildObjects_FormDataMap");
        });

        modelBuilder.Entity<Form>(entity =>
        {
            entity.HasKey(e => e.Fid);

            entity.Property(e => e.Fid).HasColumnName("fid");
            entity.Property(e => e.FileType).HasMaxLength(3);
            entity.Property(e => e.Form1).HasColumnName("Form");
            entity.Property(e => e.FormName).HasMaxLength(30);
        });

        modelBuilder.Entity<FormDataMap>(entity =>
        {
            entity.HasKey(e => e.DataMapId).HasName("PK_FormDataObject");

            entity.ToTable("FormDataMap");

            entity.Property(e => e.DataMapId).HasColumnName("DataMapID");
            entity.Property(e => e.FormDataTypeId).HasColumnName("FormDataTypeID");

            entity.HasOne(d => d.FormDataType).WithMany(p => p.FormDataMaps)
                .HasForeignKey(d => d.FormDataTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FormDataMap_FormDataType");
        });

        modelBuilder.Entity<FormDataType>(entity =>
        {
            entity.ToTable("FormDataType");

            entity.Property(e => e.FormDataTypeId).HasColumnName("FormDataTypeID");
            entity.Property(e => e.DataType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FormId).HasColumnName("FormID");

            entity.HasOne(d => d.Form).WithMany(p => p.FormDataTypes)
                .HasForeignKey(d => d.FormId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FormDataType_Forms");
        });

        modelBuilder.Entity<FormRequest>(entity =>
        {
            entity.HasKey(e => e.ReqId);

            entity.ToTable("FormRequest");

            entity.Property(e => e.ReqId).HasColumnName("ReqID");
            entity.Property(e => e.FormId).HasColumnName("FormID");
            entity.Property(e => e.RequestDateTime).HasColumnType("datetime");
            entity.Property(e => e.RequestObject).HasColumnType("text");
            entity.Property(e => e.RequestStatus).HasMaxLength(10);

            entity.HasOne(d => d.FormDataTypeNavigation).WithMany(p => p.FormRequests)
                .HasForeignKey(d => d.FormDataType)
                .HasConstraintName("FK_FormRequest_FormDataType");

            entity.HasOne(d => d.Form).WithMany(p => p.FormRequests)
                .HasForeignKey(d => d.FormId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FormRequest_Forms");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
