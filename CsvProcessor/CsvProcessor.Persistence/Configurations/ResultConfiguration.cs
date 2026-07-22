using CsvProcessor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CsvProcessor.Persistence.Configurations;

public class ResultConfiguration : IEntityTypeConfiguration<Result>
{
    public void Configure(EntityTypeBuilder<Result> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.DeltaTimeSeconds)
            .IsRequired();
        
        builder.Property(r => r.MinDate)
            .IsRequired();
        
        builder.Property(r => r.AverageExecutionTime)
            .IsRequired();
        
        builder.Property(r => r.AverageValue)
            .IsRequired();
        
        builder.Property(r => r.MedianValue)
            .IsRequired();
        
        builder.Property(r => r.MaxValue)
            .IsRequired();
        
        builder.Property(r => r.MinValue)
            .IsRequired();
        
        builder.Property(r => r.TotalRecords)
            .IsRequired();
        
        builder.Property(r => r.ProcessedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.HasIndex(r => r.MinDate);
        builder.HasIndex(r => r.AverageValue);
        builder.HasIndex(r => r.AverageExecutionTime);
        
        builder.HasMany(r => r.ValueRecords)
            .WithOne(r => r.Result)
            .HasForeignKey(r => r.ResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}