using Autofac.Annotation;
using SixLabors.ImageSharp;
using SixLaborsCaptcha.Core;

namespace sevencat.ruoyi.common.captcha.config;

[AutoConfiguration]
public class CaptchConfig
{
	[Bean]
	public SixLaborsCaptchaModule Create()
	{
		var slc = new SixLaborsCaptchaModule(new SixLaborsCaptchaOptions
		{
			DrawLines = 5,
			TextColor = [Color.Gray],
			DrawLinesColor = [Color.Gray, Color.Black, Color.DarkGrey, Color.SlateGray],
		});
		return slc;
	}
}