using System.Text.Json.Serialization;

namespace sevencat.ruoyi.sys.vo;

public class LoginVo
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; }
	
	[JsonPropertyName("expire_in")]
	public long expireqIn { get; set; }
	
	[JsonPropertyName("client_id")]
	public string ClientId { get; set; }
}

public class LoginBody
{
	public string clientId { get; set; }
	public string grantType { get; set; }
	public string code { get; set; }
	public string uuid { get; set; }
	public string username { get; set; }
	public string password { get; set; }
}