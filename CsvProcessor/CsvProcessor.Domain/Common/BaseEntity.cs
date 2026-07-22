namespace CsvProcessor.Domain.Common;

/// <summary>
/// 
/// </summary>
public class BaseEntity
{
    /// <summary>
    /// 
    /// </summary>
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public DateTime UpdatedAt { get;  set; }
}