namespace sevencat.ruoyi.common.security;

public interface ILoginService
{
	Task<long?> GetLoginuid();
}