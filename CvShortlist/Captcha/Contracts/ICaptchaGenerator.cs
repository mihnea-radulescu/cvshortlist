using CvShortlist.POCOs;

namespace CvShortlist.Captcha.Contracts;

public interface ICaptchaGenerator
{
	CaptchaInfo GenerateCaptcha();
}
