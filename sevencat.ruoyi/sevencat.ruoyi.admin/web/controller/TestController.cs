using Microsoft.AspNetCore.Mvc;

namespace sevencat.ruoyi.web.controller;

[ApiController]
[Route("[controller]")]
public class TestController
{
	[HttpGet("echo")]
	public string Echo([FromQuery] string msg)
	{
		return msg;
	}
}