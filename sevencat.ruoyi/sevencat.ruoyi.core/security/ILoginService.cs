namespace sevencat.ruoyi.core.security;

public interface ILoginService
{
	Task<long?> GetLoginuid();
}