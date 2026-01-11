namespace CvShortlist.Services.Contracts;

public interface ILanguageService
{
	IReadOnlyList<string> GetAvailableLanguages();
}
