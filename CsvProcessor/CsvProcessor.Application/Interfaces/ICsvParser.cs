using CsvProcessor.Domain.Entities;

namespace CsvProcessor.Application.Interfaces;

public interface ICsvParser
{
    public Task<(List<ValueRecord> Records, List<string> Errors)> ParseAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken);
}