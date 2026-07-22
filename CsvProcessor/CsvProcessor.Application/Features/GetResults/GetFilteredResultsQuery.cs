using CsvProcessor.Application.Dto;
using MediatR;

namespace CsvProcessor.Application.Features.GetResults;

public class GetFilteredResultsQuery : IRequest<IEnumerable<ResultDto>>
{
    public string? FileName { get; set; }
    public DateTime? MinDateFrom { get; set; }
    public DateTime? MinDateTo { get; set; }
    public double? AverageValueFrom { get; set; }
    public double? AverageValueTo { get; set; }
    public double? AverageTimeFrom { get; set; }
    public double? AverageTimeTo { get; set; }
}