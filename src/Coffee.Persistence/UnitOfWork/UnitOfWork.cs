using Microsoft.EntityFrameworkCore.Storage;
using Coffee.Application.Interfaces;
using Coffee.Persistence.Data;

namespace Coffee.Persistence.UnitOfWork;

/// <summary>
/// Unit of Work pattern for coordinating multiple repositories and transactions
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    ICoffeeRepository Coffees { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}

/// <summary>
/// Implementation of Unit of Work pattern
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly CoffeeDbContext _context;
    private readonly ICoffeeRepository _coffeeRepository;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(
        CoffeeDbContext context,
        ICoffeeRepository coffeeRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _coffeeRepository = coffeeRepository ?? throw new ArgumentNullException(nameof(coffeeRepository));
    }

    public ICoffeeRepository Coffees => _coffeeRepository;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
            throw new InvalidOperationException("A transaction is already in progress");

        _transaction = await _context.Database.BeginTransactionAsync(ct);
        return _transaction;
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction in progress");

        try
        {
            await _context.SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackAsync(ct);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
            return;

        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }

            _disposed = true;
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
    }
}
