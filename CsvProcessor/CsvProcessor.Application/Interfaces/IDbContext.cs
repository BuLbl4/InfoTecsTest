using CsvProcessor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CsvProcessor.Application.Interfaces;

/// <summary>
/// 
/// </summary>
public interface IDbContext
{
    public DbSet<ValueRecord> ValueRecords { get; set; }
    
    public DbSet<Result> Results { get; set; }
    
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    
    public DbSet<T> Set<T>() where T : class;
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}