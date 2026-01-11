using CvShortlist.Models.Contracts;

namespace CvShortlist.Models;

public class Notification : IExpirationEnabled
{
	public const int ExpirationInMonths = 6;

	public Notification()
	{
		Id = Guid.NewGuid();

		var currentDate = DateTime.UtcNow;
		DateCreated = currentDate;
		DateOfExpiration = currentDate.AddMonths(ExpirationInMonths);
	}

	public Guid Id { get; set; }

	public string Title { get; set; } = null!;
	public string Content { get; set; } = null!;

	public DateTime DateCreated { get; set; }
	public DateTime DateOfExpiration { get; set; }

	public string? ApplicationUserId { get; set; }
	public ApplicationUser? ApplicationUser { get; set; }
}
