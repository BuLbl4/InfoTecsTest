using CsvProcessor.Domain.Common;

namespace CsvProcessor.Domain.Entities;

public class ValueRecord : BaseEntity
{
    public string FileName { get; set; } = null!;
    
    public DateTime Date { get; set; }
    
    public double ExecutionTime { get; set; }
    
    public double Value { get; set; }
    
    public int RowNumber { get; set; }
    
    public Guid ResultId { get; set; }
    
    public Result Result { get; set; } = null!;
}