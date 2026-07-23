namespace CsvProcessor.Application.Dto;

public record ValueRecordDto(
    Guid Id,
    DateTime Date,
    double ExecutionTime,
    double Value,
    int RowNumber
);