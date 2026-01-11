using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CvShortlist.Models;
using CvShortlist.Models.SubscriptionTiers.Contracts;

namespace CvShortlist.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
	private const string DateTimeColumnType = "smalldatetime";
	private const int SubscriptionTierNameMaxLength = 25;

	private readonly ISubscriptionTierFactory _subscriptionTierFactory;

	public DbSet<SupportTicket> SupportTickets { get; set; }
	public DbSet<Notification> Notifications { get; set; }
	public DbSet<Subscription> Subscriptions { get; set; }
	public DbSet<JobOpening> JobOpenings { get; set; }
	public DbSet<CandidateCv> CandidateCvs { get; set; }

	public ApplicationDbContext(
		ISubscriptionTierFactory subscriptionTierFactory, DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
		_subscriptionTierFactory = subscriptionTierFactory;
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		SetupSupportTicket(modelBuilder);
		SetupNotification(modelBuilder);
		SetupSubscription(modelBuilder);
		SetupJobOpening(modelBuilder);
		SetupCandidateCv(modelBuilder);
		SetupApplicationUser(modelBuilder);

		SetupRelationships(modelBuilder);
	}

	private static void SetupSupportTicket(ModelBuilder modelBuilder)
	{
		var supportTicketBuilder = modelBuilder.Entity<SupportTicket>();

		supportTicketBuilder.HasKey(supportTicket => supportTicket.Id);

		supportTicketBuilder.Property(supportTicket => supportTicket.Name)
			.HasMaxLength(SupportTicket.NameMaxLength).IsRequired();
		supportTicketBuilder.Property(supportTicket => supportTicket.Email)
			.HasMaxLength(SupportTicket.EmailMaxLength).IsRequired();
		supportTicketBuilder.Property(supportTicket => supportTicket.Message).IsRequired();
		supportTicketBuilder.Property(supportTicket => supportTicket.DateCreated)
			.HasColumnType(DateTimeColumnType).IsRequired();
		supportTicketBuilder.Property(supportTicket => supportTicket.DateOfExpiration)
			.HasColumnType(DateTimeColumnType).IsRequired();
		supportTicketBuilder.Property(supportTicket => supportTicket.DateReplySent)
			.HasColumnType(DateTimeColumnType);

		supportTicketBuilder
			.HasIndex(supportTicket => supportTicket.DateCreated)
			.HasFilter("DateReplySent IS NULL AND Reply IS NOT NULL");
		supportTicketBuilder.HasIndex(supportTicket => supportTicket.DateOfExpiration);
		supportTicketBuilder.HasIndex(supportTicket => supportTicket.ApplicationUserId);
	}

	private static void SetupNotification(ModelBuilder modelBuilder)
	{
		var notificationBuilder = modelBuilder.Entity<Notification>();

		notificationBuilder.HasKey(notification => notification.Id);

		notificationBuilder.Property(notification => notification.Title).HasMaxLength(150).IsRequired();
		notificationBuilder.Property(notification => notification.Content).IsRequired();
		notificationBuilder.Property(notification => notification.DateCreated)
			.HasColumnType(DateTimeColumnType).IsRequired();
		notificationBuilder.Property(notification => notification.DateOfExpiration)
			.HasColumnType(DateTimeColumnType).IsRequired();

		notificationBuilder.HasIndex(notification => notification.DateOfExpiration);
		notificationBuilder.HasIndex(notification => notification.ApplicationUserId);
	}

	private void SetupSubscription(ModelBuilder modelBuilder)
	{
		var subscriptionBuilder = modelBuilder.Entity<Subscription>();

		subscriptionBuilder.HasKey(subscription => subscription.Id);

		subscriptionBuilder.Property(subscription => subscription.DateCreated)
			.HasColumnType(DateTimeColumnType).IsRequired();
		subscriptionBuilder.Property(subscription => subscription.DateUpdated)
			.HasColumnType(DateTimeColumnType).IsRequired();
		subscriptionBuilder.Property(subscription => subscription.DoesRenew).IsRequired();
		subscriptionBuilder.Property(subscription => subscription.SubscriptionTier)
			.HasConversion(
				subscriptionTier => subscriptionTier.Name,
				subscriptionTierName => _subscriptionTierFactory.FromString(subscriptionTierName)
			).HasMaxLength(SubscriptionTierNameMaxLength).IsRequired();
		subscriptionBuilder.Property(subscription => subscription.JobOpeningsAvailable).IsRequired();
		subscriptionBuilder.Property(subscription => subscription.MaxCandidateCvsPerJobOpening).IsRequired();
		subscriptionBuilder.Property(subscription => subscription.ApplicationUserId).IsRequired();

		subscriptionBuilder.HasIndex(subscription => subscription.ApplicationUserId);
	}

	private static void SetupJobOpening(ModelBuilder modelBuilder)
	{
		var jobOpeningBuilder = modelBuilder.Entity<JobOpening>();

		jobOpeningBuilder.HasKey(jobOpening => jobOpening.Id);

		jobOpeningBuilder.Property(jobOpening => jobOpening.Name).HasMaxLength(JobOpening.NameMaxLength).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.Description).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.AnalysisLanguage).HasMaxLength(25).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.Status).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.DateCreated).HasColumnType(DateTimeColumnType).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.DateOfExpiration)
			.HasColumnType(DateTimeColumnType).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.DateLastModified)
			.HasColumnType(DateTimeColumnType).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.MaxCandidateCvs).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.ApplicationUserId).IsRequired();

		jobOpeningBuilder.Ignore(jobOpening => jobOpening.TotalCandidateCvsCount);
		jobOpeningBuilder.Ignore(jobOpening => jobOpening.AllCandidateCvHashes);

		jobOpeningBuilder.HasIndex(jobOpening => jobOpening.Status).HasFilter("Status = 1");
		jobOpeningBuilder.HasIndex(jobOpening => jobOpening.DateOfExpiration);
		jobOpeningBuilder.HasIndex(jobOpening => jobOpening.ApplicationUserId);
	}

	private static void SetupCandidateCv(ModelBuilder modelBuilder)
	{
		var candidateCvBuilder = modelBuilder.Entity<CandidateCv>();

		candidateCvBuilder.HasKey(candidateCv => candidateCv.Id);

		candidateCvBuilder
			.Property(candidateCv => candidateCv.FileName).HasMaxLength(150).IsRequired();
		candidateCvBuilder
			.Property(candidateCv => candidateCv.Sha256FileHash).HasMaxLength(64).HasColumnType("char")
			.IsRequired();
		candidateCvBuilder
			.Property(candidateCv => candidateCv.DateCreated).HasColumnType(DateTimeColumnType).IsRequired();
		candidateCvBuilder.Property(candidateCv => candidateCv.JobOpeningId).IsRequired();

		candidateCvBuilder
			.HasIndex(candidateCv => new { JobId = candidateCv.JobOpeningId, candidateCv.DateCreated, candidateCv.FileName })
			.IsDescending(false, true, false);
		candidateCvBuilder
			.HasIndex(candidateCv => new { JobId = candidateCv.JobOpeningId, candidateCv.Rating })
			.IsDescending(false, true);
		candidateCvBuilder
			.HasIndex(candidateCv => candidateCv.JobOpeningId)
			.IncludeProperties(candidateCv => candidateCv.Sha256FileHash);
	}

	private static void SetupApplicationUser(ModelBuilder modelBuilder)
	{
		var applicationUserBuilder = modelBuilder.Entity<ApplicationUser>();

		applicationUserBuilder.Property(applicationUser => applicationUser.ApplicationUserType).IsRequired();
	}

	private static void SetupRelationships(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ApplicationUser>()
			.HasMany(applicationUser => applicationUser.SupportTickets)
			.WithOne(supportTicket => supportTicket.ApplicationUser)
			.HasForeignKey(supportTicket => supportTicket.ApplicationUserId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<ApplicationUser>()
			.HasMany(applicationUser => applicationUser.Notifications)
			.WithOne(notification => notification.ApplicationUser)
			.HasForeignKey(notification => notification.ApplicationUserId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<ApplicationUser>()
			.HasMany(applicationUser => applicationUser.Subscriptions)
			.WithOne(subscription => subscription.ApplicationUser)
			.HasForeignKey(subscription => subscription.ApplicationUserId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<ApplicationUser>()
			.HasMany(applicationUser => applicationUser.JobOpenings)
			.WithOne(jobOpening => jobOpening.ApplicationUser)
			.HasForeignKey(jobOpening => jobOpening.ApplicationUserId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<JobOpening>()
			.HasMany(jobOpening => jobOpening.CandidateCvs)
			.WithOne(candidateCv => candidateCv.JobOpening)
			.HasForeignKey(candidateCv => candidateCv.JobOpeningId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
