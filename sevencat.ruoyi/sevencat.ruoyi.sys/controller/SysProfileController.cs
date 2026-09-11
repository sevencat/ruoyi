using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.db.datapermission;
using sevencat.ruoyi.common.enums;
using sevencat.ruoyi.common.log.attr;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;
using BC = BCrypt.Net.BCrypt;

namespace sevencat.ruoyi.sys.controller;

/// <summary>
/// 个人信息 业务处理（对应 Java 的 <c>SysProfileController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/system/user/profile")]
public class SysProfileController(SysUserService userService, LoginService loginService, IMapper mapper)
{
	/// <summary>
	/// 获取当前登录用户的个人中心信息。
	/// </summary>
	/// <returns>用户信息、角色组和岗位组</returns>
	// Java 原注解 @GetMapping：无 @SaCheckPermission，登录即可访问
	[HttpGet]
	public async Task<CommonResult<ProfileVo>> Profile()
	{
		var loginUser = await loginService.GetLoginUser();
		if (loginUser?.UserId is not { } userId)
		{
			return CommonResult.Fail("没有权限访问用户数据!");
		}

		// C# 端数据权限按「实体」生效，覆盖面比 Java 按「Mapper 方法」更广。
		// Java 的 selectUserById / selectUserRoleGroup / selectUserPostGroup 均未声明 @DataPermission，
		// 故这里整体忽略数据权限，以对齐 Java 的实际行为
		return await DataPermissionHelper.IgnoreAsync(async () =>
		{
			var user = await userService.SelectUserById(userId);
			var roleGroup = await userService.SelectUserRoleGroup(userId);
			var postGroup = await userService.SelectUserPostGroup(userId);
			// 单独做一个 vo 专门给个人中心用 避免数据被脱敏（对应 Java 的 BeanUtil.toBean）
			return new ProfileVo(user.MapTo<ProfileUserVo>(mapper), roleGroup, postGroup).ToCommonResult();
		});
	}

	/// <summary>
	/// 修改当前登录用户的个人资料。
	/// </summary>
	/// <param name="profile">个人资料参数</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[Log("个人信息", BusinessTypeEnum.Update)]
	[HttpPut]
	public async Task<CommonResult> UpdateProfile([FromBody] SysUserProfileBo profile)
	{
		var loginUser = await loginService.GetLoginUser();
		if (loginUser?.UserId is not { } userId)
		{
			return CommonResult.Fail("没有权限访问用户数据!");
		}

		// 对应 Java 的 BeanUtil.toBean(profile, SysUserBo.class) 后 setUserId
		var user = new SysUserBo
		{
			UserId = userId,
			NickName = profile.NickName,
			Email = profile.Email,
			PhoneNumber = profile.PhoneNumber,
			Gender = profile.Gender,
			Avatar = profile.Avatar
		};

		var username = loginUser.Username;
		if (user.PhoneNumber.IsNotNullOrWhiteSpace() && !await userService.CheckPhoneUnique(user))
		{
			return CommonResult.Fail("修改用户'" + username + "'失败，手机号码已存在");
		}

		if (user.Email.IsNotNullOrWhiteSpace() && !await userService.CheckEmailUnique(user))
		{
			return CommonResult.Fail("修改用户'" + username + "'失败，邮箱账号已存在");
		}

		// 对应 Java 的 DataPermissionHelper.ignore(...)：修改的是自己的记录，可能落在自身数据范围之外
		var rows = await DataPermissionHelper.IgnoreAsync(() => userService.UpdateUserProfile(user));
		if (rows > 0)
		{
			return CommonResult.Ok();
		}

		return CommonResult.Fail("修改个人信息异常，请联系管理员");
	}

	/// <summary>
	/// 重置密码
	/// </summary>
	/// <param name="bo">新旧密码</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	// Java 原注解 @ApiEncrypt：C# 端无接口加密实现
	[Log("个人信息", BusinessTypeEnum.Update)]
	[HttpPut("updatePwd")]
	public async Task<CommonResult> UpdatePwd([FromBody] SysUserPasswordBo bo)
	{
		var loginUser = await loginService.GetLoginUser();
		if (loginUser?.UserId is not { } userId)
		{
			return CommonResult.Fail("没有权限访问用户数据!");
		}

		// Java 的 selectUserById 未声明 @DataPermission，这里忽略数据权限保持一致
		var user = await DataPermissionHelper.IgnoreAsync(() => userService.SelectUserById(userId));
		var password = user?.Password;
		if (password == null)
		{
			return CommonResult.Fail("修改密码失败，用户不存在");
		}

		if (!BC.Verify(bo.OldPassword, password))
		{
			return CommonResult.Fail("修改密码失败，旧密码错误");
		}

		if (BC.Verify(bo.NewPassword, password))
		{
			return CommonResult.Fail("新密码不能与旧密码相同");
		}

		// 对应 Java 的 DataPermissionHelper.ignore(...)
		var rows = await DataPermissionHelper.IgnoreAsync(
			() => userService.ResetUserPwd(userId, BC.HashPassword(bo.NewPassword)));
		if (rows > 0)
		{
			return CommonResult.Ok();
		}

		return CommonResult.Fail("修改密码异常，请联系管理员");
	}
}
