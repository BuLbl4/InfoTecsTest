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
    IStatisticsCalculator statisticsCalculator,
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

            if (errors.Count > 0 || valueRecords.Count == 0)
            {
                response.IsSuccess = false;
                response.Errors = errors;
                response.Message = errors.Count > 0 ? "CSV validation failed" : "No valid records found";
                return response;
            }

            await using var transaction = await context.BeginTransactionAsync(cancellationToken);

            try
            {
                await DeleteExistingData(fileName, cancellationToken);

                var result = statisticsCalculator.CalculateStatistics(valueRecords, fileName, request.File.Length);

                await context.Results.AddAsync(result, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                foreach (var record in valueRecords)
                {
                    record.ResultId = result.Id;
                }

                await context.ValueRecords.AddRangeAsync(valueRecords, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

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

    private async Task DeleteExistingData(string fileName, CancellationToken cancellationToken)
    {
        var existingResult = await context.Results
            .FirstOrDefaultAsync(r => r.FileName == fileName, cancellationToken);

        if (existingResult != null)
        {
            context.Results.Remove(existingResult);
            await context.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Deleted existing data for file: {FileName}", fileName);
        }
    }

    private ResultDto MapToResultDto(Result result)
    {
        return new ResultDto(
            result.Id,
            result.FileName,
            result.DeltaTimeSeconds,
            result.MinDate,
            result.AverageExecutionTime,
            result.AverageValue,
            result.MedianValue,
            result.MaxValue,
            result.MinValue,
            result.TotalRecords,
            result.ProcessedAt
        );
    }
}