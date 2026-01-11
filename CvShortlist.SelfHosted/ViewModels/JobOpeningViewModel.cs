using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CvShortlist.SelfHosted.Models;
using CvShortlist.SelfHosted.POCOs;

namespace CvShortlist.SelfHosted.ViewModels;

public class JobOpeningViewModel
{
	private const int DescriptionMaxLength = 10000;

	public Guid Id { get; set; }

	[Required(ErrorMessage = "Job opening name is required")]
	[MaxLength(Constraints.NameMaxLength, ErrorMessage = "The entered name is too long.")]
	public string Name { get; set; } = null!;

	[Required(ErrorMessage = "Job opening description is required")]
	[MinLength(5, ErrorMessage = "Minimum description length is 5 characters.")]
	[MaxLength(DescriptionMaxLength, ErrorMessage = "The entered description is too long.")]
	public string Description { get; set; } = null!;

	public IReadOnlyList<string> AvailableLanguages { get; set; } = null!;
	public string AnalysisLanguage { get; set; } = null!;

	public string Status { get; set; } = null!;

	public string DateCreated { get; set; } = null!;
	public string DateLastModified { get; set; } = null!;

	public IReadOnlyList<CandidateCvViewModel> CandidateCvViewModels { get; set; } = null!;

	public int JobOpeningAnalysisTimeInMinutes { get; set; }

	public JobOpeningSubmitActionType SubmitActionType { get; set; }

	public int TotalCandidateCvsCount { get; set; }
	public int CurrentPage { get; set; }
	public int PageSize { get; set; }

	public int TotalPages
	{
		get
		{
			var totalPages = TotalCandidateCvsCount / PageSize;

			if (TotalCandidateCvsCount % PageSize != 0)
			{
				totalPages++;
			}

			return totalPages;
		}
	}
}
