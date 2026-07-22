namespace CsvProcessor.Application.Dto;

public class ResultDto
{
    public Guid Id { get; set; }
    
    public string FileName { get; set; } = string.Empty;
    
    public double DeltaTimeSeconds { get; set; }
    
    public DateTime MinDate { get; set; }
    
    public double AverageExecutionTime { get; set; }
    
    public double AverageValue { get; set; }
    
    public double MedianValue { get; set; }
    
    public double MaxValue { get; set; }
    
    public double MinValue { get; set; }
    
    public int TotalRecords { get; set; }
    
    public DateTime ProcessedAt { get; set; }
}