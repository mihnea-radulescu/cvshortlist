using Microsoft.EntityFrameworkCore;
using CvShortlist.SelfHosted.Models;

namespace CvShortlist.SelfHosted.Data;

public class ApplicationDbContext : DbContext
{
	public DbSet<JobOpening> JobOpenings { get; set; }
	public DbSet<CandidateCv> CandidateCvs { get; set; }
	public DbSet<CandidateCvBlob> CandidateCvBlobs { get; set; }

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		SetupJobOpening(modelBuilder);
		SetupCandidateCv(modelBuilder);
		SetupCandidateCvBlob(modelBuilder);

		SetupRelationships(modelBuilder);
	}

	private static void SetupJobOpening(ModelBuilder modelBuilder)
	{
		var jobOpeningBuilder = modelBuilder.Entity<JobOpening>();

		jobOpeningBuilder.HasKey(jobOpening => jobOpening.Id);

		jobOpeningBuilder.Property(jobOpening => jobOpening.Name).HasMaxLength(Constraints.NameMaxLength).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.Description).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.AnalysisLanguage)
			.HasMaxLength(JobOpening.AnalysisLanguageMaxLength).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.Status).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.DateCreated).IsRequired();
		jobOpeningBuilder.Property(jobOpening => jobOpening.DateLastModified).IsRequired();

		jobOpeningBuilder.Ignore(jobOpening => jobOpening.TotalCandidateCvsCount);
		jobOpeningBuilder.Ignore(jobOpening => jobOpening.AllCandidateCvHashes);

		jobOpeningBuilder.HasIndex(jobOpening => jobOpening.Status).HasFilter("Status = 1");
	}

	private static void SetupCandidateCv(ModelBuilder modelBuilder)
	{
		var candidateCvBuilder = modelBuilder.Entity<CandidateCv>();

		candidateCvBuilder.HasKey(candidateCv => candidateCv.Id);

		candidateCvBuilder.Property(candidateCv => candidateCv.FileName)
			.HasMaxLength(Constraints.NameMaxLength).IsRequired();
		candidateCvBuilder.Property(candidateCv => candidateCv.Sha256FileHash)
			.HasMaxLength(CandidateCv.Sha256FileHashLength).IsRequired();
		candidateCvBuilder.Property(candidateCv => candidateCv.DateCreated).IsRequired();
		candidateCvBuilder.Property(candidateCv => candidateCv.JobOpeningId).IsRequired();

		candidateCvBuilder
			.HasIndex(candidateCv => new { candidateCv.JobOpeningId, candidateCv.DateCreated, candidateCv.FileName })
			.IsDescending(false, true, false);
		candidateCvBuilder
			.HasIndex(candidateCv => new { candidateCv.JobOpeningId, candidateCv.Rating })
			.IsDescending(false, true);
		candidateCvBuilder
			.HasIndex(candidateCv => new { candidateCv.JobOpeningId, candidateCv.Sha256FileHash });
	}

	private static void SetupCandidateCvBlob(ModelBuilder modelBuilder)
	{
		var candidateCvBlobBuilder = modelBuilder.Entity<CandidateCvBlob>();

		candidateCvBlobBuilder.HasKey(candidateCvBlob => candidateCvBlob.CandidateCvId);

		candidateCvBlobBuilder.Property(candidateCvBlob => candidateCvBlob.Data).IsRequired();
	}

	private static void SetupRelationships(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<JobOpening>()
			.HasMany(jobOpening => jobOpening.CandidateCvs)
			.WithOne(candidateCv => candidateCv.JobOpening)
			.HasForeignKey(candidateCv => candidateCv.JobOpeningId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<CandidateCv>()
			.HasOne(candidateCv => candidateCv.CandidateCvBlob)
			.WithOne()
			.HasForeignKey<CandidateCvBlob>(candidateCvBlob => candidateCvBlob.CandidateCvId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
