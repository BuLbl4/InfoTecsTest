using CsvProcessor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CsvProcessor.Persistence.Configurations;

public class ValueRecordConfiguration : IEntityTypeConfiguration<ValueRecord>
{
    public void Configure(EntityTypeBuilder<ValueRecord> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(v => v.FileName)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(v => v.Date)
            .IsRequired();
        
        builder.Property(v => v.ExecutionTime)
            .IsRequired()
            .HasPrecision(18, 6);
        
        builder.Property(v => v.Value)
            .IsRequired()
            .HasPrecision(18, 6);
        
        builder.Property(v => v.RowNumber)
            .IsRequired();
        
        builder.Property(v => v.ResultId)
            .IsRequired();
        
        builder.HasIndex(v => new { v.FileName, v.Date })
            .IsDescending(); 
        
        builder.HasIndex(v => new { v.FileName, v.RowNumber })
            .IsUnique();
        
        builder.HasIndex(v => v.ResultId);
        builder.HasIndex(v => v.Date);
        
        builder.HasOne(x => x.Result)
            .WithMany(x => x.ValueRecords)
            .HasForeignKey(x => x.ResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}