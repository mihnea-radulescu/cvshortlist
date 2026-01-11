using SkiaSharp;
using CvShortlist.Captcha.Contracts;
using CvShortlist.POCOs;

namespace CvShortlist.Captcha;

public class CaptchaGenerator : ICaptchaGenerator
{
    private const int Width = 200;
    private const int Height = 80;

    private const string AvailableCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private const string FontFamily = "Verdana";
    private const int FontSize = 40;

    public CaptchaInfo GenerateCaptcha()
    {
        var random = new Random();

        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));

        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        for (var i = 0; i < 10; i++)
        {
            paint.Color = new SKColor((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
            paint.StrokeWidth = random.Next(1, 3);

            canvas.DrawLine(random.Next(Width), random.Next(Height), random.Next(Width), random.Next(Height), paint);
        }

        var captchaAnswer = new string(
            Enumerable.Repeat(AvailableCharacters, 5)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());

        using var textPaint = new SKPaint();
        textPaint.IsAntialias = true;

        using var font = new SKFont();
        font.Size = FontSize;
        font.Typeface = SKTypeface.FromFamilyName(FontFamily, SKFontStyle.Bold);

        for (var i = 0; i < captchaAnswer.Length; i++)
        {
            textPaint.Color = new SKColor((byte)random.Next(100), (byte)random.Next(100), (byte)random.Next(100));

            var x = 20 + i * 35;
            var y = 55 + random.Next(-10, 10);

            canvas.Save();
            canvas.RotateDegrees(random.Next(-15, 15), x, y);
            canvas.DrawText(captchaAnswer[i].ToString(), x, y, font, textPaint);
            canvas.Restore();
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var captchaImageBase64 = Convert.ToBase64String(data.ToArray());

        return new CaptchaInfo(captchaImageBase64, captchaAnswer);
    }
}
