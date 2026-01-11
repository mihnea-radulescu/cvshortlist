using System;
using System.Threading.Tasks;
using CvShortlist.SelfHosted.Data;

namespace CvShortlist.SelfHosted.Services.Contracts;

public interface IDbContextService
{
	Task<TResult> ExecuteQueryAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> queryAsync);

	Task ExecuteUpdateAsync(Func<ApplicationDbContext, Task> updateAsync);
}
