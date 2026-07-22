using CsvProcessor.Application.Dto.Responses;
using Microsoft.AspNetCore.Http;
using MediatR;

namespace CsvProcessor.Application.Features.ProcessCsv;

public record ProcessCsvCommand(IFormFile File) : IRequest<ProcessCsvResponse>;
