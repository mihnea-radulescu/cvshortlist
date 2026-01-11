using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CvShortlist.SelfHosted.Data;

public static class SqliteDatabaseConfiguration
{
	public const string ConnectionString = "Data Source=Database/CvShortlist.sqlite.db";
	public const int CommandTimeoutInSeconds = 60;

	public static async Task ExecuteSetup(ApplicationDbContext dbContext)
	{
		await using (var autoVacuumSetupConnection = dbContext.Database.GetDbConnection())
		{
			await autoVacuumSetupConnection.OpenAsync();
			await using (var autoVacuumSetupCommand = autoVacuumSetupConnection.CreateCommand())
			{
				autoVacuumSetupCommand.CommandText = "PRAGMA auto_vacuum = INCREMENTAL;";
				await autoVacuumSetupCommand.ExecuteNonQueryAsync();
			}
		}

		await dbContext.Database.MigrateAsync();

		await using (var journalModeSetupConnection = dbContext.Database.GetDbConnection())
		{
			await journalModeSetupConnection.OpenAsync();
			await using (var journalModeSetupCommand = journalModeSetupConnection.CreateCommand())
			{
				journalModeSetupCommand.CommandText = "PRAGMA journal_mode = WAL;";
				await journalModeSetupCommand.ExecuteNonQueryAsync();
			}
		}
	}
}
