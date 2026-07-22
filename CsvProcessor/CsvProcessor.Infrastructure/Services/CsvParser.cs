using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvProcessor.Application.Interfaces;
using CsvProcessor.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CsvProcessor.Infrastructure.Services;

public class CsvParser : ICsvParser
{
    private readonly ILogger _logger;
    private static readonly DateTime MinDate = new(2000, 1, 1);

    public CsvParser(ILogger<CsvParser> logger)
    {
        _logger = logger;
    }
    
    public async Task<(List<ValueRecord> Records, List<string> Errors)> ParseAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken)
    {
        var records = new List<ValueRecord>();
        var errors = new List<string>();
        var lineNumber = 0;

        try
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                HeaderValidated = null,
                MissingFieldFound = null,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = context =>
                {
                    var badLine = context.RawRecord?.Trim() ?? "unknown";
                    errors.Add($"Invalid data format at line: {badLine}");
                }
            };

            using var csv = new CsvReader(reader, config);

            if (!await csv.ReadAsync())
            {
                errors.Add("File is empty or has no header");
                return (records, errors);
            }

            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                lineNumber++;

                try
                {
                    var dateString = csv.GetField(0)?.Trim();
                    var execTimeString = csv.GetField(1)?.Trim();
                    var valueString = csv.GetField(2)?.Trim();

                    if (string.IsNullOrEmpty(dateString) || 
                        string.IsNullOrEmpty(execTimeString) || 
                        string.IsNullOrEmpty(valueString))
                    {
                        errors.Add($"Line {lineNumber}: Missing required fields");
                        continue;
                    }

                    if (!TryParseCustomDate(dateString, out var date))
                    {
                        errors.Add($"Line {lineNumber}: Invalid date format '{dateString}'. Expected: YYYY-MM-DDThh-mm-ss.ffffZ");
                        continue;
                    }

                    if (date < MinDate)
                    {
                        errors.Add($"Line {lineNumber}: Date '{date:yyyy-MM-dd HH:mm:ss}' is before 01.01.2000");
                        continue;
                    }

                    if (date > DateTime.UtcNow)
                    {
                        errors.Add($"Line {lineNumber}: Date '{date:yyyy-MM-dd HH:mm:ss}' is in the future");
                        continue;
                    }

                    if (!double.TryParse(execTimeString, NumberStyles.Float, CultureInfo.InvariantCulture, out var executionTime))
                    {
                        errors.Add($"Line {lineNumber}: Invalid ExecutionTime format '{execTimeString}'. Expected a number");
                        continue;
                    }

                    if (executionTime < 0)
                    {
                        errors.Add($"Line {lineNumber}: ExecutionTime '{executionTime}' cannot be negative");
                        continue;
                    }

                    if (!double.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    {
                        errors.Add($"Line {lineNumber}: Invalid Value format '{valueString}'. Expected a number");
                        continue;
                    }

                    if (value < 0)
                    {
                        errors.Add($"Line {lineNumber}: Value '{value}' cannot be negative");
                        continue;
                    }

                    records.Add(new ValueRecord
                    {
                        FileName = fileName,
                        Date = date,
                        ExecutionTime = executionTime,
                        Value = value,
                        RowNumber = lineNumber
                    });
                }
                catch (Exception ex)
                {
                    errors.Add($"Line {lineNumber}: Unexpected error - {ex.Message}");
                    _logger.LogError(ex, "Error parsing line {LineNumber}", lineNumber);
                }
            }

            if (records.Count > 0 && (records.Count < 1 || records.Count > 10000))
            {
                errors.Add($"Invalid number of records: {records.Count}. Must be between 1 and 10,000");
                records.Clear();
            }

            _logger.LogInformation(
                "Parsed {RecordCount} records with {ErrorCount} errors from CSV {FileName}",
                records.Count,
                errors.Count,
                fileName);

            return (records, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error parsing CSV {FileName}", fileName);
            errors.Add($"Fatal error parsing CSV: {ex.Message}");
            return (records, errors);
        }
    }
    private bool TryParseCustomDate(string dateString, out DateTime date)
    {
        date = default;

        if (string.IsNullOrWhiteSpace(dateString))
            return false;

        try
        {
            var parts = dateString.Split('T');
            if (parts.Length != 2)
                return false;

            var datePart = parts[0];
            var timePart = parts[1];

            timePart = timePart.Replace('-', ':');

            var normalizedDateString = $"{datePart}T{timePart}";

            var formats = new[]
            {
                "yyyy-MM-ddTHH:mm:ss.ffffZ",
                "yyyy-MM-ddTHH:mm:ss.fffZ",
                "yyyy-MM-ddTHH:mm:ss.ffZ",
                "yyyy-MM-ddTHH:mm:ss.fZ",
                "yyyy-MM-ddTHH:mm:ssZ"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(
                        normalizedDateString,
                        format,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out date))
                {
                    return true;
                }
            }

            if (DateTime.TryParse(normalizedDateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}