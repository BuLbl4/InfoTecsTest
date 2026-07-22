namespace CsvProcessor.Application.Dto.Responses;

public class BaseResponse
{
    public string Message { get; set; } = string.Empty;

    public List<string> Errors { get; set; } = null!;
    
    public bool IsSuccess { get; set; }
    
    public int TotalCount { get; set; }
}