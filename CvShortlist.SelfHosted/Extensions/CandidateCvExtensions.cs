using System.Text.Json;
using CvShortlist.SelfHosted.Models;
using CvShortlist.SelfHosted.POCOs;
using CvShortlist.SelfHosted.ViewModels;

namespace CvShortlist.SelfHosted.Extensions;

public static class CandidateCvExtensions
{
	private static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		AllowTrailingCommas = true
	};

	extension(CandidateCv candidateCv)
	{
		public CandidateCvViewModel ToCandidateCvViewModel(UserSettings userSettings)
		{
			var candidateCvViewModel = new CandidateCvViewModel
			{
				Id = candidateCv.Id,

				FileName = candidateCv.FileName,
				DateCreated = candidateCv.DateCreated.ToUserDateTimeString(userSettings),

				Analysis = candidateCv.Analysis is null
					? null
					: JsonSerializer.Deserialize<CandidateAiAnalysisViewModel>(
						candidateCv.Analysis, JsonSerializerOptions)!,

				IsSelected = false
			};

			return candidateCvViewModel;
		}
	}
}
