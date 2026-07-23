using CsvProcessor.Application.Dto;
using CsvProcessor.Application.Features.GetLastValues;
using CsvProcessor.Application.Features.GetResults;
using CsvProcessor.Application.Features.ProcessCsv;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CsvProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CsvController(
    IMediator mediator,
    ILogger<CsvController> logger) : ControllerBase
{
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadCsv(
        [FromForm] FileUploadDto upload,
        CancellationToken cancellationToken)
    {
        var file = upload.File;

        logger.LogInformation("CSV upload started. FileName: {FileName}, Size: {FileSize} bytes", file?.FileName, file?.Length);

        if (file == null || file.Length == 0)
        {
            logger.LogWarning("CSV upload failed. File is empty or null");

            return BadRequest(new
            {
                IsSuccess = false,
                Message = "File is empty or null",
                Errors = new[] { "File is required" }
            });
        }

        var extension = Path.GetExtension(file.FileName);

        if (!string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("CSV upload failed. Invalid extension: {Extension}, FileName: {FileName}", extension, file.FileName);

            return BadRequest(new
            {
                IsSuccess = false,
                Message = "Invalid file format",
                Errors = new[] { "Only CSV files are allowed" }
            });
        }

        try
        {
            var command = new ProcessCsvCommand { File = file };

            var result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                logger.LogWarning("CSV processing failed. FileName: {FileName}. Errors: {@Errors}", file.FileName, result.Errors);

                return BadRequest(result);
            }

            logger.LogInformation("CSV processed successfully. FileName: {FileName}", file.FileName);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while processing CSV. FileName: {FileName}", file.FileName);

            return StatusCode(500, new
            {
                IsSuccess = false,
                Message = "An unexpected error occurred",
                Errors = new[] { ex.Message }
            });
        }
    }


    [HttpGet("results")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFiltered(
        [FromQuery] GetFilteredResultsQuery query,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Getting filtered results. FileName: {FileName}, DateFrom: {DateFrom}, DateTo: {DateTo}",
            query.FileName,
            query.MinDateFrom,
            query.MinDateTo);

        try
        {
            var results = await mediator.Send(query, cancellationToken);

            logger.LogInformation("Filtered results retrieved successfully");

            return Ok(results);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while getting filtered results");

            return StatusCode(500, new
            {
                Message = "Error while retrieving results"
            });
        }
    }


    [HttpGet("values/last/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLastValues(
        string fileName,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting last values for file: {FileName}", fileName);

        try
        {
            var query = new GetLastValuesQuery { FileName = fileName };

            var values = await mediator.Send(query, cancellationToken);

            if (!values.Any())
            {
                logger.LogWarning("No values found for file: {FileName}", fileName);

                return NotFound($"No records found for file: {fileName}");
            }

            logger.LogInformation("Last values retrieved successfully. FileName: {FileName}", fileName);

            return Ok(values);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting last values. FileName: {FileName}", fileName);

            return StatusCode(500, new { Message = "Error while retrieving values" });
        }
    }
}