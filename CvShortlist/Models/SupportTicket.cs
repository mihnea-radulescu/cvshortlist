using CvShortlist.Models.Contracts;

namespace CvShortlist.Models;

public class SupportTicket : IExpirationEnabled
{
	public const int NameMaxLength = 100;
	public const int EmailMaxLength = 100;

	private const int ExpirationInMonths = 6;

	public SupportTicket()
	{
		Id = Guid.NewGuid();

		var currentDate = DateTime.UtcNow;
		DateCreated = currentDate;
		DateOfExpiration = currentDate.AddMonths(ExpirationInMonths);
	}

	public Guid Id { get; set; }

	public string Name { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string Message { get; set; } = null!;

	public DateTime DateCreated { get; set; }
	public DateTime DateOfExpiration { get; set; }

	public string? Reply { get; set; }
	public DateTime? DateReplySent { get; set; }

	public string? ApplicationUserId { get; set; }
	public ApplicationUser? ApplicationUser { get; set; }
}
