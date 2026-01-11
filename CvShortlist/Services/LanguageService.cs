using System.Collections.Immutable;
using CvShortlist.Services.Contracts;

namespace CvShortlist.Services;

public class LanguageService : ILanguageService
{
	public IReadOnlyList<string> GetAvailableLanguages() => AvailableLanguages;

	private static readonly IReadOnlyList<string> AvailableLanguages =
		new List<string>
		{
			"English",
			"Spanish", "French", "German", "Portuguese", "Italian", "Dutch", "Russian",
			"Chinese (Simplified)", "Chinese (Traditional)", "Japanese", "Korean",
			"Arabic", "Hindi", "Bengali", "Turkish", "Vietnamese", "Polish", "Ukrainian", "Romanian",
			"Persian (Farsi)", "Thai", "Malay / Indonesian", "Hebrew"
		}
		.OrderBy(language => language)
		.ToImmutableArray();
}
