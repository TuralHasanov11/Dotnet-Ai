using Microsoft.EntityFrameworkCore;

namespace SharedKernel.Persistence;

public interface IInTransaction
{
    Task<T> Run<T>(DbContext dbContext, Func<DbContext, T> action);

    Task Run(DbContext dbContext, Action<DbContext> action);
}

public class InTransaction : IInTransaction
{
    public async Task<T> Run<T>(DbContext dbContext, Func<DbContext, T> action)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var result = action(dbContext);

            if (dbContext.ChangeTracker.HasChanges())
            {
                await dbContext.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task Run(DbContext dbContext, Action<DbContext> action)
    {
        await Run(dbContext, dbContext => { action(dbContext); return true; });
    }
}
