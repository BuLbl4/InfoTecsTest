using CsvProcessor.Domain.Entities;

namespace CsvProcessor.Application.Interfaces;

public interface IStatisticsCalculator
{
    Result CalculateStatistics(List<ValueRecord> records, string fileName, long fileSize);
}