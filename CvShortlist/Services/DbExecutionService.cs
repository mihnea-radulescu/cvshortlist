using Microsoft.EntityFrameworkCore;
using CvShortlist.Data;
using CvShortlist.Services.Contracts;

namespace CvShortlist.Services;

public class DbExecutionService : IDbExecutionService
{
	private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

	public DbExecutionService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
	{
		_dbContextFactory = dbContextFactory;
	}

	public async Task<TResult> ExecuteQueryAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> queryAsync)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync();

		var strategy = db.Database.CreateExecutionStrategy();

		return await strategy.ExecuteAsync(async () => await queryAsync(db));
	}

	public async Task ExecuteUpdateAsync(Func<ApplicationDbContext, Task> updateAsync)
	{
		await ExecuteUpdateWithResultAsync<object>(async db =>
		{
			await updateAsync(db);
			return null!;
		});
	}

	public async Task<TResult> ExecuteUpdateWithResultAsync<TResult>(
		Func<ApplicationDbContext, Task<TResult>> updateAsync)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync();

		var strategy = db.Database.CreateExecutionStrategy();

		return await strategy.ExecuteAsync(async () =>
		{
			await using var transaction = await db.Database.BeginTransactionAsync();

			var result = await updateAsync(db);
			await db.SaveChangesAsync();

			await transaction.CommitAsync();

			return result;
		});
	}
}
