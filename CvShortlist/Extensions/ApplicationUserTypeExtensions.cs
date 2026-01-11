using CvShortlist.Models;

namespace CvShortlist.Extensions;

public static class ApplicationUserTypeExtensions
{
	extension(ApplicationUserType applicationUserType)
	{
		public string Description => applicationUserType switch
		{
			ApplicationUserType.RecruiterOrHr => "Professional recruiter or HR department",
			ApplicationUserType.Candidate => "Candidate",
			_ => string.Empty
		};
	}
}
