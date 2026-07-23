using CsvProcessor.Application.Dto;
using CsvProcessor.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CsvProcessor.Application.Features.GetResults;

public class GetFilteredResultsQueryHandler(IDbContext context) : 
    IRequestHandler<GetFilteredResultsQuery, IEnumerable<ResultDto>>
{
    public async Task<IEnumerable<ResultDto>> Handle(GetFilteredResultsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Results.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.FileName))
            query = query.Where(r => EF.Functions.ILike(r.FileName, "%" + request.FileName + "%"));

        if (request.MinDateFrom.HasValue)
            query = query.Where(r => r.MinDate >= request.MinDateFrom.Value);

        if (request.MinDateTo.HasValue)
            query = query.Where(r => r.MinDate <= request.MinDateTo.Value);

        if (request.AverageValueFrom.HasValue)
            query = query.Where(r => r.AverageValue >= request.AverageValueFrom.Value);

        if (request.AverageValueTo.HasValue)
            query = query.Where(r => r.AverageValue <= request.AverageValueTo.Value);

        if (request.AverageTimeFrom.HasValue)
            query = query.Where(r => r.AverageExecutionTime >= request.AverageTimeFrom.Value);

        if (request.AverageTimeTo.HasValue)
            query = query.Where(r => r.AverageExecutionTime <= request.AverageTimeTo.Value);

        var results = await query
            .OrderByDescending(r => r.ProcessedAt)
            .ToListAsync(cancellationToken);

        return results.Select(result => new ResultDto(
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
            result.ProcessedAt));
    }
}