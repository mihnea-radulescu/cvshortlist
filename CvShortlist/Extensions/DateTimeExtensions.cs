using System.Globalization;
using CvShortlist.POCOs;

namespace CvShortlist.Extensions;

public static class DateTimeExtensions
{
	private static readonly CultureInfo DefaultCultureInfo = new("en-US");

	extension(DateTime utcDateTime)
	{
		public string ToUserDateTimeString(UserSettings userSettings)
		{
			CultureInfo userCultureInfo;
			try
			{
				userCultureInfo = new CultureInfo(userSettings.Culture);
			}
			catch
			{
				userCultureInfo = DefaultCultureInfo;
			}

			var userDateTimeOffset = TimeSpan.FromMinutes(userSettings.DateTimeOffsetInMinutes);
			var userDateTime = utcDateTime + userDateTimeOffset;

			var userDateTimeString = userDateTime.ToString(userCultureInfo);
			return userDateTimeString;
		}
	}
}
