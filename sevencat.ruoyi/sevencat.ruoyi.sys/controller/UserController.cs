using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.excel;
using sevencat.ruoyi.common.lang;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;
using BC = BCrypt.Net.BCrypt;

namespace sevencat.ruoyi.sys.controller;

/// <summary>
/// 用户信息（对应 Java 的 <c>SysUserController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/system/user")]
public class UserController(
	LoginService loginService,
	SysDeptService deptService,
	SysUserService userService,
	SysRoleService roleService,
	SysPostService postService,
	IHttpContextAccessor httpCtxAccessor)
{
	/// <summary>
	/// 用户数据导入模板/导出使用的 sheet 名
	/// </summary>
	private const string ExcelSheetName = "用户数据";

	/// <summary>
	/// 获取当前登录用户信息
	/// </summary>
	/// <returns>当前登录用户信息、角色与权限集合</returns>
	// Java 原注解 @GetMapping("/getInfo")：Java 此处无 @SaCheckPermission，登录即可访问
	[HttpGet("getInfo")]
	public async Task<CommonResult<UserInfoVo>> GetInfo()
	{
		var loginUser = await loginService.GetLoginUser();
		if (loginUser == null)
		{
			return CommonResult.Fail("没有权限访问用户数据!");
		}

		// Java 用 DataPermissionHelper.ignore 忽略数据权限，C# 端无数据权限设施，无需处理
		var user = await userService.SelectUserById(loginUser.UserId.Value);
		if (user == null)
		{
			return CommonResult.Fail("没有权限访问用户数据!");
		}

		var userInfoVo = new UserInfoVo
		{
			User = user,
			Permissions = loginUser.MenuPermission,
			Roles = loginUser.RolePermission
		};
		return userInfoVo.ToCommonResult();
	}

	/// <summary>
	/// 根据用户编号获取详细信息
	/// </summary>
	/// <param name="userId">用户ID，为空时只返回角色下拉列表</param>
	/// <returns>用户详情、角色与岗位信息</returns>
	// Java 原注解 @GetMapping(value = {"/", "/{userId}"})
	[SaCheckPermission("system:user:query")]
	[HttpGet]
	[HttpGet("{userId:long}")]
	public async Task<CommonResult<SysUserInfoVo>> GetInfoByUserId([FromRoute] long? userId)
	{
		var userInfoVo = new SysUserInfoVo();
		if (userId.HasValue)
		{
			await userService.CheckUserDataScope(userId);
			var sysUser = await userService.SelectUserById(userId.Value);
			userInfoVo.User = sysUser;
			userInfoVo.RoleIds = (await roleService.SelectRoleListByUserId(userId.Value))
				.Select(x => (long?)x).ToList();
			var deptId = sysUser?.DeptId;
			if (deptId.HasValue)
			{
				var postBo = new SysPostBo { DeptId = deptId };
				userInfoVo.Posts = await postService.SelectPostList(postBo);
				userInfoVo.PostIds = (await postService.SelectPostListByUserId(userId.Value))
					.Select(x => (long?)x).ToList();
			}
		}

		var roleBo = new SysRoleBo { Status = SystemConstants.NORMAL };
		var roles = await roleService.SelectRoleList(roleBo);
		// 非超管查询时过滤掉超管角色（对应 Java 的 StreamUtils.filter）
		userInfoVo.Roles = IsSuperAdmin(userId) ? roles : roles.Where(r => !r.IsSuperAdmin()).ToList();
		return userInfoVo.ToCommonResult();
	}

	/// <summary>
	/// 分页查询用户列表。
	/// </summary>
	/// <param name="user">用户查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>用户分页列表</returns>
	[SaCheckPermission("system:user:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysUserVo>>> List([FromQuery] SysUserBo user,
		[FromQuery] PageQuery2 pageQuery)
	{
		var itemlst = await userService.SelectPageUserList(user, pageQuery);
		return itemlst.ToCommonResult();
	}

	/// <summary>
	/// 获取指定部门下的全部用户信息。
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>用户列表</returns>
	[SaCheckPermission("system:user:list")]
	[HttpGet("list/dept/{deptId:long}")]
	public async Task<CommonResult<List<SysUserVo>>> ListByDept([FromRoute] long deptId)
	{
		return (await userService.SelectUserListByDept(deptId)).ToCommonResult();
	}

	/// <summary>
	/// 根据用户ID串批量获取用户基础信息
	/// </summary>
	/// <param name="userIds">用户ID串，形如 1,2,3</param>
	/// <param name="deptId">部门ID</param>
	/// <returns>用户基础信息列表</returns>
	// Java 原注解 @RequestParam(required = false) Long[] userIds：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[SaCheckPermission("system:user:query")]
	[HttpGet("optionselect")]
	public async Task<CommonResult<List<SysUserVo>>> Optionselect([FromQuery] string userIds,
		[FromQuery] long? deptId)
	{
		// Java 原注解 @DataPermission：C# 端无数据权限设施，未实现
		return (await userService.SelectUserByIds(ParseLongList(userIds), deptId)).ToCommonResult();
	}

	/// <summary>
	/// 新增用户。
	/// </summary>
	/// <param name="user">用户新增参数</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:user:add")]
	[HttpPost]
	public async Task<CommonResult> Add([FromBody] SysUserBo user)
	{
		await deptService.CheckDeptDataScope(user.DeptId);
		if (!await userService.CheckUserNameUnique(user))
		{
			return CommonResult.Fail($"新增用户'{user.UserName}'失败，登录账号已存在");
		}

		if (user.PhoneNumber.IsNotNullOrWhiteSpace() && !await userService.CheckPhoneUnique(user))
		{
			return CommonResult.Fail($"新增用户'{user.UserName}'失败，手机号码已存在");
		}

		if (user.Email.IsNotNullOrWhiteSpace() && !await userService.CheckEmailUnique(user))
		{
			return CommonResult.Fail($"新增用户'{user.UserName}'失败，邮箱账号已存在");
		}

		user.Password = BC.HashPassword(user.Password);
		return ToAjax(await userService.InsertUser(user));
	}

	/// <summary>
	/// 修改用户。
	/// </summary>
	/// <param name="user">用户编辑参数</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:user:edit")]
	[HttpPut]
	public async Task<CommonResult> Edit([FromBody] SysUserBo user)
	{
		userService.CheckUserAllowed(user.UserId);
		await userService.CheckUserDataScope(user.UserId);
		await deptService.CheckDeptDataScope(user.DeptId);
		if (!await userService.CheckUserNameUnique(user))
		{
			return CommonResult.Fail($"修改用户'{user.UserName}'失败，登录账号已存在");
		}

		if (user.PhoneNumber.IsNotNullOrWhiteSpace() && !await userService.CheckPhoneUnique(user))
		{
			return CommonResult.Fail($"修改用户'{user.UserName}'失败，手机号码已存在");
		}

		if (user.Email.IsNotNullOrWhiteSpace() && !await userService.CheckEmailUnique(user))
		{
			return CommonResult.Fail($"修改用户'{user.UserName}'失败，邮箱账号已存在");
		}

		return ToAjax(await userService.UpdateUser(user));
	}

	/// <summary>
	/// 删除用户
	/// </summary>
	/// <param name="userIds">用户ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @DeleteMapping("/{userIds}") Long[] userIds：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[SaCheckPermission("system:user:remove")]
	[HttpDelete("{userIds}")]
	public async Task<CommonResult> Remove([FromRoute] string userIds)
	{
		var ids = ParseLongList(userIds).ToArray();
		var loginUser = await loginService.GetLoginUser();
		if (loginUser?.UserId is { } loginUserId && ids.Contains(loginUserId))
		{
			return CommonResult.Fail("当前用户不能删除");
		}

		return ToAjax(await userService.DeleteUserByIds(ids));
	}

	/// <summary>
	/// 重置指定用户密码。
	/// </summary>
	/// <param name="user">用户参数</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @ApiEncrypt：C# 端无接口加密实现
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:user:resetPwd")]
	[HttpPut("resetPwd")]
	public async Task<CommonResult> ResetPwd([FromBody] SysUserBo user)
	{
		userService.CheckUserAllowed(user.UserId);
		await userService.CheckUserDataScope(user.UserId);
		user.Password = BC.HashPassword(user.Password);
		return ToAjax(await userService.ResetUserPwd(user.UserId ?? 0L, user.Password));
	}

	/// <summary>
	/// 修改用户状态。
	/// </summary>
	/// <param name="user">用户参数</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:user:edit")]
	[HttpPut("changeStatus")]
	public async Task<CommonResult> ChangeStatus([FromBody] SysUserBo user)
	{
		userService.CheckUserAllowed(user.UserId);
		await userService.CheckUserDataScope(user.UserId);
		return ToAjax(await userService.UpdateUserStatus(user.UserId ?? 0L, user.Status));
	}

	/// <summary>
	/// 解锁用户
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:user:edit")]
	[HttpGet("unlock/{userId:long}")]
	public async Task<CommonResult> Unlock([FromRoute] long userId)
	{
		var user = await userService.SelectUserById(userId);
		if (user == null)
		{
			return CommonResult.Fail("用户不存在");
		}

		// Java 用 RedisUtils 清除 CacheNames.PWD_ERR_CNT_KEY + userName 的密码错误次数并置 isUnlock = true；
		// C# 端 LoginService 尚未实现密码错误次数计数，故此处无可清除的计数
		return CommonResult.Ok();
	}

	/// <summary>
	/// 根据用户编号获取授权角色
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <returns>用户及其可授权角色信息</returns>
	[SaCheckPermission("system:user:query")]
	[HttpGet("authRole/{userId:long}")]
	public async Task<CommonResult<SysUserInfoVo>> AuthRole([FromRoute] long userId)
	{
		await userService.CheckUserDataScope(userId);
		var user = await userService.SelectUserById(userId);
		var roles = await roleService.SelectRolesAuthByUserId(userId);
		// 非超管查询时过滤掉超管角色（对应 Java 的 StreamUtils.filter）
		var userInfoVo = new SysUserInfoVo
		{
			User = user,
			Roles = IsSuperAdmin(userId) ? roles : roles.Where(r => !r.IsSuperAdmin()).ToList()
		};
		return userInfoVo.ToCommonResult();
	}

	/// <summary>
	/// 用户授权角色
	/// </summary>
	/// <param name="userId">用户Id</param>
	/// <param name="roleIds">角色ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:user:edit")]
	[HttpPut("authRole")]
	public async Task<CommonResult> InsertAuthRole([FromQuery] long userId, [FromQuery] string roleIds)
	{
		await userService.CheckUserDataScope(userId);
		await userService.InsertUserAuth(userId, ParseLongList(roleIds).ToArray());
		return CommonResult.Ok();
	}

	/// <summary>
	/// 导出符合条件的用户列表。
	/// </summary>
	/// <param name="user">用户查询条件</param>
	// Java 原注解 @PostMapping("/export") + ExcelBuilder.toResponse：C# 端改用 MiniExcel 写响应流
	[SaCheckPermission("system:user:export")]
	[HttpPost("export")]
	public async Task Export([FromQuery] SysUserBo user)
	{
		var list = await userService.SelectUserExportList(user);
		await WriteExcelAsync(list);
	}

	/// <summary>
	/// 导出用户导入模板。
	/// </summary>
	// Java 原注解 @PostMapping("/importTemplate") + ExcelBuilder.toResponse：C# 端改用 MiniExcel 写响应流
	[HttpPost("importTemplate")]
	public async Task ImportTemplate()
	{
		await WriteExcelAsync(new List<SysUserImportVo>());
	}

	/// <summary>
	/// 导入数据
	/// </summary>
	/// <param name="file">导入文件</param>
	/// <param name="updateSupport">是否更新已存在数据</param>
	/// <returns>导入结果说明</returns>
	// Java 原注解 @PostMapping(value = "/importData", consumes = MULTIPART_FORM_DATA_VALUE)
	// Java 通过 ExcelBuilder.read(...).listener(new SysUserImportListener(updateSupport)).doRead() 读取，
	// C# 端先用 MiniExcel 解析出列表，再把监听器逻辑交给 SysUserService.ImportUser
	[SaCheckPermission("system:user:import")]
	[HttpPost("importData")]
	public async Task<CommonResult> ImportData([FromForm] IFormFile file, [FromForm] bool updateSupport = false)
	{
		if (file == null)
		{
			return CommonResult.Fail("导入文件不能为空");
		}

		// MiniExcel 读取需要可随机访问的流，先整体拷进内存
		using var stream = new MemoryStream();
		await file.CopyToAsync(stream);
		stream.Position = 0;
		// MiniExcel 1.46 泛型 Query 用 hasHeader 指定首行为标题行
		var userList = MiniExcel.Query<SysUserImportVo>(stream, hasHeader: true).ToList();

		var analysis = await userService.ImportUser(userList, updateSupport, await loginService.GetLoginuid());
		return CommonResult.Ok(analysis);
	}

	/// <summary>
	/// 获取用户筛选用的部门树。
	/// </summary>
	/// <param name="dept">部门查询条件</param>
	/// <returns>部门树列表</returns>
	// Java 原注解 @GetMapping("/deptTree")
	[SaCheckPermission("system:user:list")]
	[HttpGet("deptTree")]
	public async Task<CommonResult<List<TreeSelectNode<TSysDept>>>> DeptTree([FromQuery] SysDeptBo dept)
	{
		return (await deptService.SelectDeptTreeList(dept)).ToCommonResult();
	}

	/// <summary>
	/// 判断是否为超级管理员用户ID
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <returns>true 是超级管理员 false 不是</returns>
	private static bool IsSuperAdmin(long? userId)
	{
		return userId.HasValue && userId.Value == SystemConstants.SUPER_ADMIN_USER_ID;
	}

	/// <summary>
	/// 响应结果处理（对应 Java BaseController 的 <c>toAjax</c>）
	/// </summary>
	/// <param name="rows">影响行数</param>
	/// <returns>操作结果</returns>
	private static CommonResult ToAjax(int rows)
	{
		return CommonResult.CreateDbUpdate(rows);
	}

	/// <summary>
	/// 解析逗号分隔的ID串（对应 Java 的 <c>StringUtils.splitTo(value, Convert::toLong)</c>）
	/// </summary>
	/// <param name="value">逗号分隔的ID串</param>
	/// <returns>ID列表</returns>
	private static List<long> ParseLongList(string value)
	{
		if (value.IsNullOrWhiteSpace())
		{
			return [];
		}

		return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(item => long.TryParse(item, out var id) ? id : (long?)null)
			.Where(id => id.HasValue)
			.Select(id => id.Value)
			.ToList();
	}

	/// <summary>
	/// 按照给定的数据类型把数据写进当前响应流（对应 Java 的 <c>ExcelBuilder.toResponse</c>）
	/// </summary>
	/// <typeparam name="T">Excel 行类型</typeparam>
	/// <param name="rows">数据行</param>
	private async Task WriteExcelAsync<T>(IEnumerable<T> rows)
	{
		await ExcelResponseWriter.WriteAsync(httpCtxAccessor.HttpContext.Response, rows, ExcelSheetName);
	}
}
