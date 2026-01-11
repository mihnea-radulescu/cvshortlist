namespace CvShortlist.POCOs;

public class UserSettings
{
	public UserSettings(string culture, int dateTimeOffsetInMinutes)
	{
		Culture = culture;
		DateTimeOffsetInMinutes = dateTimeOffsetInMinutes;
	}

	public string Culture { get; }
	public int DateTimeOffsetInMinutes { get; }
}
