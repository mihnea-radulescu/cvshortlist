using CvShortlist.Data;

namespace CvShortlist.Services.Contracts;

public interface IDbExecutionService
{
	Task<TResult> ExecuteQueryAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> queryAsync);

	Task ExecuteUpdateAsync(Func<ApplicationDbContext, Task> updateAsync);
	Task<TResult> ExecuteUpdateWithResultAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> updateAsync);
}
