using CsvProcessor.Application.Interfaces;
using CsvProcessor.Domain.Entities;

namespace CsvProcessor.Infrastructure.Services;


public class StatisticsCalculator : IStatisticsCalculator
{
    public Result CalculateStatistics(List<ValueRecord> records, string fileName, long fileSize)
    {
        if (records == null || records.Count == 0)
            throw new ArgumentException("Records cannot be empty", nameof(records));
        
        var minDate = records.Min(r => r.Date);
        var maxDate = records.Max(r => r.Date);
        var deltaTime = (maxDate - minDate).TotalSeconds;
        
        var avgExecutionTime = records.Average(r => r.ExecutionTime);
        var avgValue = records.Average(r => r.Value);
        
        var sortedValues = records.Select(r => r.Value).OrderBy(v => v).ToList();
        var median = CalculateMedian(sortedValues);
        
        var maxValue = records.Max(r => r.Value);
        var minValue = records.Min(r => r.Value);
        
        return new Result
        {
            FileName = fileName,
            DeltaTimeSeconds = deltaTime,
            MinDate = minDate,
            AverageExecutionTime = avgExecutionTime,
            AverageValue = avgValue,
            MedianValue = median,
            MaxValue = maxValue,
            MinValue = minValue,
            TotalRecords = records.Count,
            ProcessedAt = DateTime.UtcNow,
            FileSize = fileSize
        };
    }
    
    private double CalculateMedian(List<double> sortedValues)
    {
        var count = sortedValues.Count;
        if (count % 2 == 1)
            return sortedValues[count / 2];
        return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
    }
}