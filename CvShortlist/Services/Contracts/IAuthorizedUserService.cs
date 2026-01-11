namespace CvShortlist.Services.Contracts;

public interface IAuthorizedUserService
{
	Task<string?> GetApplicationUserIdAsync();
}
