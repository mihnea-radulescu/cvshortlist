using System.Collections.Generic;

namespace CvShortlist.SelfHosted.Services.Contracts;

public interface ILanguageService
{
	IReadOnlyList<string> GetAvailableLanguages();
}
