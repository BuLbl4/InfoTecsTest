using CsvProcessor.Application.Dto;
using CsvProcessor.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CsvProcessor.Application.Features.GetLastValues;

public class GetLastValuesQueryHandler(IDbContext context)
    : IRequestHandler<GetLastValuesQuery, IEnumerable<ValueRecordDto>>
{
    public async Task<IEnumerable<ValueRecordDto>> Handle(
        GetLastValuesQuery request,
        CancellationToken cancellationToken)
    {
        var records = await context.ValueRecords
            .AsNoTracking()
            .Where(v => EF.Functions.ILike(v.FileName, "%" + request.FileName + "%"))
            .OrderByDescending(v => v.Date)
            .Take(10)
            .ToListAsync(cancellationToken);

        return records.Select(record => new ValueRecordDto(
            record.Id,
            record.Date, 
            record.ExecutionTime, 
            record.Value,
            record.RowNumber));
    }
}