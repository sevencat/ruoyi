using Autofac.Annotation;

namespace sevencat.ruoyi.common.captcha.config;

[Component]
public class CaptchaProperties
{
	[Value("captcha:enabled")]
	public bool Enabled { get; set; }
}