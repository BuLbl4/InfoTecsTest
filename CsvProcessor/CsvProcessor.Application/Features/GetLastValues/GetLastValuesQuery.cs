using CsvProcessor.Application.Dto;
using MediatR;

namespace CsvProcessor.Application.Features.GetLastValues;

public class GetLastValuesQuery : IRequest<IEnumerable<ValueRecordDto>>
{
    public string FileName { get; set; } = string.Empty;
}