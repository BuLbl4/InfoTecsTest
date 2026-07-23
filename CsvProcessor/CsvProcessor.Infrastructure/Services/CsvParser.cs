using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvProcessor.Application.Interfaces;
using CsvProcessor.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CsvProcessor.Infrastructure.Services;

public class CsvParser(ILogger<CsvParser> logger) : ICsvParser
{
    private static readonly DateTime MinDate = new(2000, 1, 1);

    private const int MaxRecordCount = 10000;
    private const int MinRecordCount = 1;

    private static readonly string[] DateFormats =
    [
        "yyyy-MM-ddTHH:mm:ss.ffffZ",
        "yyyy-MM-ddTHH:mm:ss.fffZ",
        "yyyy-MM-ddTHH:mm:ss.ffZ",
        "yyyy-MM-ddTHH:mm:ss.fZ",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss.ffff",
        "yyyy-MM-ddTHH:mm:ss.fff",
        "yyyy-MM-ddTHH:mm:ss.ff",
        "yyyy-MM-ddTHH:mm:ss.f",
        "yyyy-MM-ddTHH:mm:ss"
    ];

    public async Task<(List<ValueRecord> Records, List<string> Errors)> ParseAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken)
    {
        var records = new List<ValueRecord>();
        var errors = new List<string>();
        var lineNumber = 1;

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

                BadDataFound = _ =>
                {
                    errors.Add("Invalid CSV data format");
                }
            };
            
            using var csv = new CsvReader(reader, config);

            if (!await csv.ReadAsync())
            {
                errors.Add("File is empty");
                return (records, errors);
            }
            csv.ReadHeader();

            var headers = csv.HeaderRecord;
            
            if (headers == null)
            {
                errors.Add("CSV header is missing");
                return (records, errors);
            }
            
            var requiredColumns = new[]
            {
                "Date",
                "ExecutionTime",
                "Value"
            };


            var missingColumns = requiredColumns
                .Where(required => 
                    !headers.Any(header => string.Equals(header, required, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            
            if (missingColumns.Count > 0)
            {
                errors.Add($"Missing required columns: {string.Join(", ", missingColumns)}");
                return (records, errors);
            }
            
            while (await csv.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                lineNumber++;
                
                if (lineNumber - 1 > MaxRecordCount)
                {
                    errors.Add($"CSV contains more than {MaxRecordCount} records");
                    break;
                }
                
                try
                {
                    var dateString = csv.GetField("Date")?.Trim();
                    var executionTimeString = csv.GetField("ExecutionTime")?.Trim();
                    var valueString = csv.GetField("Value")?.Trim();
                    
                    if (string.IsNullOrWhiteSpace(dateString) ||
                        string.IsNullOrWhiteSpace(executionTimeString) ||
                        string.IsNullOrWhiteSpace(valueString))
                    {
                        errors.Add($"Line {lineNumber}: Missing required fields");
                        continue;
                    }
                    
                    if (!TryParseDate(dateString, out var date))
                    {
                        errors.Add($"Line {lineNumber}: Invalid date format '{dateString}'");
                        continue;
                    }
                    
                    if (date < MinDate)
                    {
                        errors.Add($"Line {lineNumber}: Date cannot be before 01.01.2000");
                        continue;
                    }
                    
                    if (date > DateTime.UtcNow)
                    {
                        errors.Add($"Line {lineNumber}: Date cannot be in the future");
                        continue;
                    }
                    
                    if (!double.TryParse(executionTimeString, NumberStyles.Float, CultureInfo.InvariantCulture, out var executionTime))
                    {
                        errors.Add($"Line {lineNumber}: Invalid ExecutionTime value '{executionTimeString}'");
                        continue;
                    }
                    
                    if (double.IsNaN(executionTime) || double.IsInfinity(executionTime))
                    {
                        errors.Add($"Line {lineNumber}: ExecutionTime must be a valid number");
                        continue;
                    }
                    
                    if (executionTime < 0)
                    {
                        errors.Add($"Line {lineNumber}: ExecutionTime cannot be negative");
                        continue;
                    }

                    if (!double.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    {
                        errors.Add($"Line {lineNumber}: Invalid Value '{valueString}'");
                        continue;
                    }
                    
                    if (double.IsNaN(value) || double.IsInfinity(value))
                    {
                        errors.Add($"Line {lineNumber}: Value must be a valid number");
                        continue;
                    }
                    
                    if (value < 0)
                    {
                        errors.Add($"Line {lineNumber}: Value cannot be negative");
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
                    logger.LogError(ex, "Error parsing CSV line {LineNumber}", lineNumber);
                }
            }
            
            if (records.Count < MinRecordCount)
            {
                errors.Add("CSV file must contain at least one record");
                records.Clear();
            }
            
            logger.LogInformation(
                "Parsed {RecordCount} records with {ErrorCount} errors from CSV {FileName}",
                records.Count,
                errors.Count,
                fileName);
            
            return (records, errors);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("CSV parsing cancelled for file {FileName}", fileName);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error parsing CSV {FileName}", fileName);

            errors.Add($"Fatal error parsing CSV: {ex.Message}");

            return (records, errors);
        }
    }
    
    private bool TryParseDate(string dateString, out DateTime date)
    {
        date = default;
        
        if (string.IsNullOrWhiteSpace(dateString))
            return false;
        
        try
        {
            var parts = dateString.Split('T');
            
            if (parts.Length != 2)
                return false;
            
            var timePart = parts[1];
            
            if (timePart.Count(c => c == '-') >= 2)
            {
                timePart = timePart.Replace('-', ':');
            }


            var normalizedDateString = $"{parts[0]}T{timePart}";
            
            foreach (var format in DateFormats)
            {
                if (DateTime.TryParseExact(
                        normalizedDateString,
                        format,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal |
                        DateTimeStyles.AdjustToUniversal,
                        out date))
                {
                    return true;
                }
            }
            
            return DateTime.TryParse(
                normalizedDateString,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal |
                DateTimeStyles.AdjustToUniversal,
                out date);
        }
        catch
        {
            return false;
        }
    }
}