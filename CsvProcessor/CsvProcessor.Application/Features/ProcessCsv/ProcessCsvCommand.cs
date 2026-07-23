using CsvProcessor.Application.Dto.Responses;
using Microsoft.AspNetCore.Http;
using MediatR;

namespace CsvProcessor.Application.Features.ProcessCsv;

public record ProcessCsvCommand : IRequest<ProcessCsvResponse>
{
    public required IFormFile File { get; init; }
}