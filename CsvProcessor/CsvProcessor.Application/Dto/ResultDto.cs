namespace CsvProcessor.Application.Dto;

public record ResultDto(
    Guid Id,
    string FileName,
    double DeltaTimeSeconds,
    DateTime MinDate,
    double AverageExecutionTime,
    double AverageValue,
    double MedianValue,
    double MaxValue,
    double MinValue,
    int TotalRecords,
    DateTime ProcessedAt
);