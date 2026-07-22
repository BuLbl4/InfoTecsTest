using CsvProcessor.Application.Dto;
using CsvProcessor.Application.Dto.Responses;
using CsvProcessor.Application.Interfaces;
using CsvProcessor.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CsvProcessor.Application.Features.ProcessCsv;

public class ProcessCsvCommandHandler(
    IDbContext context,
    ICsvParser csvParser,
    ILogger<ProcessCsvCommandHandler> logger)
    : IRequestHandler<ProcessCsvCommand, ProcessCsvResponse>
{
    public async Task<ProcessCsvResponse> Handle(ProcessCsvCommand request, CancellationToken cancellationToken)
    {
        var fileName = request.File.FileName;
        var response = new ProcessCsvResponse();

        try
        {
            await using var stream = request.File.OpenReadStream();
            var (valueRecords, errors) = await csvParser.ParseAsync(stream, fileName, cancellationToken);

            if (errors.Count != 0)
            {
                response.IsSuccess = false;
                response.Errors = errors;
                response.Message = "CSV validation failed";
                return response;
            }

            if (valueRecords.Count == 0)
            {
                response.IsSuccess = false;
                response.Message = "CSV validation failed";
                response.Errors = ["No valid records found in CSV"];
                return response;
            }

            await using var transaction = await context.BeginTransactionAsync(cancellationToken);

            try
            {
                var existingResult = await context.Results
                    .FirstOrDefaultAsync(r => r.FileName == fileName, cancellationToken);

                if (existingResult != null)
                {
                    context.ValueRecords.RemoveRange(
                        context.ValueRecords.Where(v => v.FileName == fileName));
                    context.Results.Remove(existingResult);
                    await context.SaveChangesAsync(cancellationToken);
                    
                    logger.LogInformation("Deleted existing data for file: {FileName}", fileName);
                }

                var stats = CalculateStatistics(valueRecords);

                var result = new Result
                {
                    FileName = fileName,
                    DeltaTimeSeconds = stats.DeltaTimeSeconds,
                    MinDate = stats.MinDate,
                    AverageExecutionTime = stats.AverageExecutionTime,
                    AverageValue = stats.AverageValue,
                    MedianValue = stats.MedianValue,
                    MaxValue = stats.MaxValue,
                    MinValue = stats.MinValue,
                    TotalRecords = valueRecords.Count,
                    ProcessedAt = DateTime.UtcNow,
                    FileSize = request.File.Length
                };

                await context.Results.AddAsync(result, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                foreach (var record in valueRecords)
                {
                    record.ResultId = result.Id;
                }

                const int batchSize = 1000;
                for (var i = 0; i < valueRecords.Count; i += batchSize)
                {
                    var batch = valueRecords.Skip(i).Take(batchSize);
                    await context.ValueRecords.AddRangeAsync(batch, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                response.IsSuccess = true;
                response.TotalCount = valueRecords.Count;
                response.Message = "File processed successfully";
                response.Result = MapToResultDto(result);

                logger.LogInformation(
                    "Successfully processed file: {FileName}, Records: {RecordCount}",
                    fileName,
                    valueRecords.Count);

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                logger.LogError(ex, "Error during transaction for file: {FileName}", fileName);
                
                response.IsSuccess = false;
                response.Message = "Error processing file";
                response.Errors = [ex.Message];
                return response;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing file: {FileName}", fileName);
            response.IsSuccess = false;
            response.Message = "An error occurred while processing the file";
            response.Errors = [ex.Message];
            return response;
        }
    }

    private (double DeltaTimeSeconds, DateTime MinDate, double AverageExecutionTime,
        double AverageValue, double MedianValue, double MaxValue, double MinValue)
        CalculateStatistics(List<ValueRecord> records)
    {
        var minDate = records.Min(r => r.Date);
        var maxDate = records.Max(r => r.Date);
        var deltaTime = (maxDate - minDate).TotalSeconds;

        var avgExecutionTime = records.Average(r => r.ExecutionTime);
        var avgValue = records.Average(r => r.Value);

        var sortedValues = records.Select(r => r.Value).OrderBy(v => v).ToList();
        var median = CalculateMedian(sortedValues);

        var maxValue = records.Max(r => r.Value);
        var minValue = records.Min(r => r.Value);

        return (deltaTime, minDate, avgExecutionTime, avgValue, median, maxValue, minValue);
    }

    private double CalculateMedian(List<double> sortedValues)
    {
        var count = sortedValues.Count;
        if (count % 2 == 1)
            return sortedValues[count / 2];
        return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
    }

    private ResultDto MapToResultDto(Result result)
    {
        return new ResultDto
        {
            Id = result.Id,
            FileName = result.FileName,
            DeltaTimeSeconds = result.DeltaTimeSeconds,
            MinDate = result.MinDate,
            AverageExecutionTime = result.AverageExecutionTime,
            AverageValue = result.AverageValue,
            MedianValue = result.MedianValue,
            MaxValue = result.MaxValue,
            MinValue = result.MinValue,
            TotalRecords = result.TotalRecords,
            ProcessedAt = result.ProcessedAt
        };
    }
}