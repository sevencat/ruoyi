using sevencat.ruoyi.common.entity;

namespace sevencat.ruoyi.common.security;

public interface ILoginService
{
	Task<long?> GetLoginuid();
	LoginUser FastgetLoginUser();
}