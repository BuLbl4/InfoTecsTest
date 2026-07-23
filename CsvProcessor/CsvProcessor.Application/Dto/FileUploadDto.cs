using Microsoft.AspNetCore.Http;

namespace CsvProcessor.Application.Dto;

public record FileUploadDto(IFormFile? File);
    