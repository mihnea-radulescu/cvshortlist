using Microsoft.AspNetCore.Identity;

namespace CvShortlist.Models;

public class ApplicationUser : IdentityUser
{
	public ApplicationUserType ApplicationUserType { get; set; }

	public ICollection<SupportTicket> SupportTickets { get; set; } = null!;
	public ICollection<Notification> Notifications { get; set; } = null!;
	public ICollection<Subscription> Subscriptions { get; set; } = null!;
	public ICollection<JobOpening> JobOpenings { get; set; } = null!;
}
