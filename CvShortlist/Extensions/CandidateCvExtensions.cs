using System.Text.Json;
using CvShortlist.Models;
using CvShortlist.POCOs;
using CvShortlist.ViewModels;

namespace CvShortlist.Extensions;

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
