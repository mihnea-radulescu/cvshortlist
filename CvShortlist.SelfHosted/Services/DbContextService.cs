using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CvShortlist.SelfHosted.Data;
using CvShortlist.SelfHosted.Services.Contracts;

namespace CvShortlist.SelfHosted.Services;

public class DbContextService : IDbContextService
{
	private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

	public DbContextService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
	{
		_dbContextFactory = dbContextFactory;
	}

	public async Task<TResult> ExecuteQueryAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> queryAsync)
	{
		await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

		return await queryAsync(dbContext);
	}

	public async Task ExecuteUpdateAsync(Func<ApplicationDbContext, Task> updateAsync)
	{
		await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

		await updateAsync(dbContext);

		await dbContext.SaveChangesAsync();
	}
}
