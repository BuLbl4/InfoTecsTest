using CsvProcessor.Domain.Common;

namespace CsvProcessor.Domain.Entities;

public class Result : BaseEntity
{
    public string FileName { get; set; } = null!;
    
    public double DeltaTimeSeconds { get; set; }
    
    public DateTime MinDate { get; set; }
    
    public double AverageExecutionTime { get; set; }
    
    public double AverageValue { get; set; }
    
    public double MedianValue  { get; set; }

    public double MaxValue { get; set; }
    
    public double MinValue { get; set; }
    
    public int TotalRecords { get; set; }
    
    public DateTime ProcessedAt { get; set; }
    
    public long? FileSize { get; set; }

    public ICollection<ValueRecord> ValueRecords { get; set; } = null!;
}