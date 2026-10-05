-- --------------------------------------------------------
-- 主机:                           127.0.0.1
-- 服务器版本:                        11.8.3-MariaDB - mariadb.org binary distribution
-- 服务器操作系统:                      Win64
-- HeidiSQL 版本:                  12.20.0.7334
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- 导出  表 rdc.rdc_map_switch_point 结构
CREATE TABLE IF NOT EXISTS `rdc_map_switch_point` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Name` varchar(64) NOT NULL,
  `FromMapId` bigint(20) NOT NULL,
  `ToMapId` bigint(20) NOT NULL,
  `FromPosX` float NOT NULL,
  `FromPosY` float NOT NULL,
  `FromPosZ` float NOT NULL,
  `FromAngleYaw` float NOT NULL,
  `ToPosX` float NOT NULL,
  `ToPosY` float NOT NULL,
  `ToPosZ` float NOT NULL,
  `ToAngleYaw` float NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `idx_rdc_map_switch_point_1` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='地图切换点位设置';

-- 正在导出表  rdc.rdc_map_switch_point 的数据：~0 rows (大约)

-- 导出  表 rdc.rdc_nav_leg 结构
CREATE TABLE IF NOT EXISTS `rdc_nav_leg` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `PathId` bigint(20) NOT NULL,
  `ToPointId` bigint(20) NOT NULL,
  `PointInfo` smallint(6) NOT NULL,
  `Gait` smallint(6) NOT NULL,
  `Speed` smallint(6) NOT NULL,
  `Manner` smallint(6) NOT NULL,
  `ObsMode` smallint(6) NOT NULL,
  `NavMode` smallint(6) NOT NULL,
  `Terrain` smallint(6) NOT NULL,
  `Posture` smallint(6) NOT NULL,
  `TaskId` bigint(20) NOT NULL COMMENT '任务id，对应 TRdcNavTask.Id',
  `TaskConfig` longtext DEFAULT NULL COMMENT '任务配置',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 正在导出表  rdc.rdc_nav_leg 的数据：~0 rows (大约)
INSERT INTO `rdc_nav_leg` (`Id`, `PathId`, `ToPointId`, `PointInfo`, `Gait`, `Speed`, `Manner`, `ObsMode`, `NavMode`, `Terrain`, `Posture`, `TaskId`, `TaskConfig`) VALUES
	(2, 1, 8, 1, 0, 0, 0, 0, 0, 0, 0, 0, '{}');

-- 导出  表 rdc.rdc_nav_map 结构
CREATE TABLE IF NOT EXISTS `rdc_nav_map` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Name` varchar(64) NOT NULL,
  `MapFn` varchar(256) DEFAULT NULL COMMENT '地图文件名',
  `InitX` double NOT NULL,
  `InitY` double NOT NULL,
  `CreateAt` datetime(3) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_rdc_nav_map_1` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='这个是用来定义，后面其他系统方便使用的';

-- 正在导出表  rdc.rdc_nav_map 的数据：~0 rows (大约)
INSERT INTO `rdc_nav_map` (`Id`, `Name`, `MapFn`, `InitX`, `InitY`, `CreateAt`) VALUES
	(1, 'test1', 'test1', 0, 0, '2026-09-14 12:53:07.055');

-- 导出  表 rdc.rdc_nav_map_path 结构
CREATE TABLE IF NOT EXISTS `rdc_nav_map_path` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `TaskPathId` bigint(20) NOT NULL,
  `MapId` bigint(20) NOT NULL,
  `Order` int(11) NOT NULL COMMENT '次序',
  `Name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_rdc_nav_map_path_1` (`TaskPathId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='子地图路径';

-- 正在导出表  rdc.rdc_nav_map_path 的数据：~0 rows (大约)

-- 导出  表 rdc.rdc_nav_path 结构
CREATE TABLE IF NOT EXISTS `rdc_nav_path` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `MapId` bigint(20) NOT NULL,
  `Name` varchar(255) DEFAULT NULL,
  `InitPointId` bigint(20) NOT NULL COMMENT '巡检的初始点位',
  PRIMARY KEY (`Id`),
  KEY `idx_rdc_nav_path_1` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='地图路径';

-- 正在导出表  rdc.rdc_nav_path 的数据：~0 rows (大约)
INSERT INTO `rdc_nav_path` (`Id`, `MapId`, `Name`, `InitPointId`) VALUES
	(1, 1, 'ttt1', 7);

-- 导出  表 rdc.rdc_nav_point 结构
CREATE TABLE IF NOT EXISTS `rdc_nav_point` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `MapId` bigint(20) NOT NULL,
  `Value` int(11) NOT NULL,
  `PosX` float NOT NULL,
  `PosY` float NOT NULL,
  `PosZ` float NOT NULL,
  `AngleYaw` float NOT NULL,
  `PointInfo` smallint(6) NOT NULL,
  `NavTaskId` bigint(20) NOT NULL DEFAULT 0 COMMENT '所属哪个路径',
  `PathId` bigint(20) NOT NULL DEFAULT 0 COMMENT '所属哪个路径',
  `SwitchPointId` bigint(20) DEFAULT NULL COMMENT '地图切换用的点位，每条路径最后一个点位必须是这个，如果不是最终地图',
  `Gait` smallint(6) NOT NULL DEFAULT 0,
  `Speed` smallint(6) NOT NULL DEFAULT 0,
  `Manner` smallint(6) NOT NULL DEFAULT 0,
  `ObsMode` smallint(6) NOT NULL DEFAULT 0,
  `NavMode` smallint(6) NOT NULL DEFAULT 0,
  `Terrain` smallint(6) NOT NULL DEFAULT 0,
  `Posture` smallint(6) NOT NULL DEFAULT 0,
  `PointTaskId` bigint(20) NOT NULL DEFAULT 0 COMMENT '任务id，对应 TRdcNavTask.Id',
  `PointTaskConfig` longtext DEFAULT NULL COMMENT '任务配置（点位上要做哪些事，按字段是否填决定是否执行）',
  `AlgoId` int(11) NOT NULL DEFAULT 0 COMMENT '配置的算法id',
  `AlgoConfig` longtext DEFAULT NULL COMMENT '算法相关配置',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `idx_rdc_nav_point_1` (`PathId`,`Value`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='这个是导航点位，包括怎么走到这里来';

-- 正在导出表  rdc.rdc_nav_point 的数据：~6 rows (大约)
INSERT INTO `rdc_nav_point` (`Id`, `MapId`, `Value`, `PosX`, `PosY`, `PosZ`, `AngleYaw`, `PointInfo`, `NavTaskId`, `PathId`, `SwitchPointId`, `Gait`, `Speed`, `Manner`, `ObsMode`, `NavMode`, `Terrain`, `Posture`, `PointTaskId`, `PointTaskConfig`, `AlgoId`, `AlgoConfig`) VALUES
	(7, 1, 1, 0, 0, 0, 0, 0, 0, 0, NULL, 0, 0, 0, 0, 0, 0, 0, 0, NULL, 0, NULL),
	(8, 1, 2, 0, 0, 0, 0, 0, 0, 0, NULL, 0, 0, 0, 0, 0, 0, 0, 0, NULL, 0, NULL),
	(9, 1, 3, 0, 0, 0, 0, 0, 0, 0, NULL, 0, 0, 0, 0, 0, 0, 0, 0, NULL, 0, NULL),
	(10, 1, 4, 0, 0, 0, 0, 0, 0, 0, NULL, 0, 0, 0, 0, 0, 0, 0, 0, NULL, 0, NULL),
	(11, 1, 5, 0, 0, 0, 0, 0, 0, 0, NULL, 0, 0, 0, 0, 0, 0, 0, 0, NULL, 0, NULL),
	(12, 1, 6, 0, 0, 0, 0, 0, 0, 0, NULL, 0, 0, 0, 0, 0, 0, 0, 0, NULL, 0, NULL);

-- 导出  表 rdc.rdc_nav_point_task 结构
CREATE TABLE IF NOT EXISTS `rdc_nav_point_task` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `NavPointId` bigint(20) NOT NULL COMMENT '导航点id,对应TNavPoint',
  `TaskId` bigint(20) NOT NULL COMMENT '任务id，这个对应着TRdcNavTask',
  `TaskConfig` longtext DEFAULT NULL COMMENT '任务配置（点位上要做哪些事，按字段是否填决定是否执行）',
  `UpdateAt` datetime(3) NOT NULL,
  `AlgoId` int(11) NOT NULL DEFAULT 0 COMMENT '配置的算法id',
  `AlgoConfig` longtext DEFAULT NULL COMMENT '算法相关配置',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='导航点任务 ,这个是单点任务';

-- 正在导出表  rdc.rdc_nav_point_task 的数据：~1 rows (大约)
INSERT INTO `rdc_nav_point_task` (`Id`, `NavPointId`, `TaskId`, `TaskConfig`, `UpdateAt`, `AlgoId`, `AlgoConfig`) VALUES
	(2, 9, 2, '{}', '2026-09-14 13:12:40.298', 0, NULL);

-- 导出  表 rdc.rdc_nav_task 结构
CREATE TABLE IF NOT EXISTS `rdc_nav_task` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `PathId` bigint(20) NOT NULL COMMENT '这个是对应路径的',
  `Name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_rdc_nav_task_1` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='总路径，包含多个地图';

-- 正在导出表  rdc.rdc_nav_task 的数据：~2 rows (大约)
INSERT INTO `rdc_nav_task` (`Id`, `PathId`, `Name`) VALUES
	(1, 1, NULL),
	(2, 1, NULL);

-- 导出  表 rdc.rdc_patrol_rec 结构
CREATE TABLE IF NOT EXISTS `rdc_patrol_rec` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `TaskId` bigint(20) NOT NULL,
  `NavPathId` bigint(20) NOT NULL,
  `taskRecId` bigint(20) NOT NULL,
  `Status` int(11) NOT NULL,
  `CurrentPointIndex` int(11) NOT NULL,
  `StartAt` datetime(3) DEFAULT NULL,
  `EndAt` datetime(3) DEFAULT NULL,
  `CreateAt` datetime(3) NOT NULL,
  `DogId` bigint(20) NOT NULL DEFAULT 0,
  `PathId` bigint(20) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `idx_rdc_patrol_rec_1` (`CreateAt`),
  KEY `idx_rdc_patrol_rec_3` (`DogId`,`CreateAt`),
  KEY `idx_rdc_patrol_rec_2` (`CreateAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 正在导出表  rdc.rdc_patrol_rec 的数据：~0 rows (大约)

-- 导出  表 rdc.rdc_patrol_rec_item 结构
CREATE TABLE IF NOT EXISTS `rdc_patrol_rec_item` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `RecId` bigint(20) NOT NULL,
  `PointIndex` int(11) NOT NULL,
  `PointValue` bigint(20) NOT NULL,
  `PicFn1` varchar(255) DEFAULT NULL,
  `PicFn2` varchar(255) DEFAULT NULL,
  `PicFn3` varchar(255) DEFAULT NULL,
  `Ret` bigint(20) NOT NULL,
  `Msg` varchar(255) DEFAULT NULL,
  `AlgoStatus` int(11) NOT NULL DEFAULT 0 COMMENT '算法执行状态',
  `AlgoId` int(11) NOT NULL DEFAULT 0 COMMENT '配置的算法id',
  `AlgoConfig` longtext DEFAULT NULL COMMENT '算法相关配置',
  `ResultData` longtext DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 正在导出表  rdc.rdc_patrol_rec_item 的数据：~0 rows (大约)

-- 导出  表 rdc.rdc_robot_dog 结构
CREATE TABLE IF NOT EXISTS `rdc_robot_dog` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(64) NOT NULL,
  `Model` varchar(16) NOT NULL COMMENT '型号',
  `FirmwareVer` varchar(16) NOT NULL COMMENT '固件版本',
  `IpAddr` varchar(255) DEFAULT NULL,
  `NavHost` varchar(255) DEFAULT NULL,
  `NavPort` int(11) NOT NULL,
  `SciHost` varchar(255) DEFAULT NULL,
  `SciPort` int(11) NOT NULL,
  `SciLocalPort` int(11) NOT NULL,
  `SshHost` varchar(255) DEFAULT NULL,
  `SshUName` varchar(255) DEFAULT NULL,
  `SshPwd` varchar(255) DEFAULT NULL,
  `SshPort` int(11) NOT NULL,
  `Status` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `idx_rdc_robot_dog_1` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 正在导出表  rdc.rdc_robot_dog 的数据：~0 rows (大约)
INSERT INTO `rdc_robot_dog` (`Id`, `Name`, `Model`, `FirmwareVer`, `IpAddr`, `NavHost`, `NavPort`, `SciHost`, `SciPort`, `SciLocalPort`, `SshHost`, `SshUName`, `SshPwd`, `SshPort`, `Status`) VALUES
	(1, 'X3002671', 'X30', '4.0', '192.168.1.106', '192.168.1.106', 1, '192.168.1.106', 9000, 1, '192.168.1.106', 'ysc', '\'', 22, 1);

-- 导出  表 rdc.sys_client 结构
CREATE TABLE IF NOT EXISTS `sys_client` (
  `id` bigint(20) NOT NULL COMMENT 'id',
  `client_id` varchar(64) DEFAULT NULL COMMENT '客户端id',
  `client_key` varchar(32) DEFAULT NULL COMMENT '客户端key',
  `client_secret` varchar(255) DEFAULT NULL COMMENT '客户端秘钥',
  `grant_type` varchar(255) DEFAULT NULL COMMENT '授权类型',
  `device_type` varchar(32) DEFAULT NULL COMMENT '设备类型',
  `access_path` varchar(2000) DEFAULT NULL COMMENT '允许访问路径',
  `ip_whitelist` varchar(1000) DEFAULT NULL COMMENT 'IP白名单',
  `active_timeout` int(11) DEFAULT 1800 COMMENT 'token活跃超时时间',
  `timeout` int(11) DEFAULT 604800 COMMENT 'token固定超时',
  `status` char(1) DEFAULT '0' COMMENT '状态（0正常 1停用）',
  `del_flag` char(1) DEFAULT '0' COMMENT '删除标志（0代表存在 1代表删除）',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='系统授权表';

-- 正在导出表  rdc.sys_client 的数据：~2 rows (大约)
INSERT INTO `sys_client` (`id`, `client_id`, `client_key`, `client_secret`, `grant_type`, `device_type`, `access_path`, `ip_whitelist`, `active_timeout`, `timeout`, `status`, `del_flag`, `create_dept`, `create_by`, `create_time`, `update_by`, `update_time`) VALUES
	(1762000000000000001, 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'pc123', 'password,social', 'pc', NULL, NULL, 1800, 604800, '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', 1761100000000000001, '2026-09-10 08:18:58'),
	(1762000000000000002, '428a8310cd442757ae699df5d894f051', 'app', 'app123', 'password,sms,social', 'android', '/app/**', NULL, 1800, 604800, '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', 1761100000000000001, '2026-09-10 08:18:58');

-- 导出  表 rdc.sys_config 结构
CREATE TABLE IF NOT EXISTS `sys_config` (
  `config_id` bigint(20) NOT NULL COMMENT '参数主键',
  `config_name` varchar(100) DEFAULT '' COMMENT '参数名称',
  `config_key` varchar(100) DEFAULT '' COMMENT '参数键名',
  `config_value` varchar(500) DEFAULT '' COMMENT '参数键值',
  `config_type` char(1) DEFAULT 'N' COMMENT '系统内置（Y是 N否）',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `remark` varchar(500) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY (`config_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='参数配置表';

-- 正在导出表  rdc.sys_config 的数据：~3 rows (大约)
INSERT INTO `sys_config` (`config_id`, `config_name`, `config_key`, `config_value`, `config_type`, `create_dept`, `create_by`, `create_time`, `update_by`, `update_time`, `remark`) VALUES
	(1761700000000000001, '用户管理-账号初始密码', 'sys.user.initPassword', '123456', 'Y', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '初始化密码 123456'),
	(1761700000000000002, '账号自助-是否开启用户注册功能', 'sys.account.registerUser', 'false', 'Y', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '是否开启注册用户功能（true开启，false关闭）'),
	(1761700000000000003, 'OSS预览列表资源开关', 'sys.oss.previewListResource', 'true', 'Y', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, 'true:开启, false:关闭');

-- 导出  表 rdc.sys_dept 结构
CREATE TABLE IF NOT EXISTS `sys_dept` (
  `dept_id` bigint(20) NOT NULL COMMENT '部门id',
  `parent_id` bigint(20) DEFAULT 0 COMMENT '父部门id',
  `ancestors` varchar(500) DEFAULT '' COMMENT '祖级列表',
  `dept_name` varchar(30) DEFAULT '' COMMENT '部门名称',
  `dept_category` varchar(100) DEFAULT NULL COMMENT '部门类别编码',
  `order_num` int(4) DEFAULT 0 COMMENT '显示顺序',
  `leader` bigint(20) DEFAULT NULL COMMENT '负责人',
  `phone` varchar(11) DEFAULT NULL COMMENT '联系电话',
  `email` varchar(50) DEFAULT NULL COMMENT '邮箱',
  `status` char(1) DEFAULT '0' COMMENT '部门状态（0正常 1停用）',
  `del_flag` char(1) DEFAULT '0' COMMENT '删除标志（0代表存在 1代表删除）',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`dept_id`),
  KEY `idx_sys_dept_parent_id` (`parent_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='部门表';

-- 正在导出表  rdc.sys_dept 的数据：~10 rows (大约)
INSERT INTO `sys_dept` (`dept_id`, `parent_id`, `ancestors`, `dept_name`, `dept_category`, `order_num`, `leader`, `phone`, `email`, `status`, `del_flag`, `create_dept`, `create_by`, `create_time`, `update_by`, `update_time`) VALUES
	(1761000000000000100, 0, '0', 'XXX科技', NULL, 0, NULL, '15888888888', 'xxx@qq.com', '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL),
	(1761000000000000101, 1761000000000000100, '0,1761000000000000100', '深圳总公司', NULL, 1, NULL, '15888888888', 'xxx@qq.com', '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL),
	(1761000000000000102, 1761000000000000100, '0,1761000000000000100', '长沙分公司', NULL, 2, NULL, '15888888888', 'xxx@qq.com', '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL),
	(1761000000000000103, 1761000000000000101, '0,1761000000000000100,1761000000000000101', '研发部门', NULL, 1, 1761100000000000001, '15888888888', 'xxx@qq.com', '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL),
	(1761000000000000104, 1761000000000000101, '0,1761000000000000100,1761000000000000101', '市场部门', NULL, 2, NULL, '15888888888', 'xxx@qq.com', '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL),
	(1761000000000000105, 1761000000000000101, '0,1761000000000000100,1761000000000000101', '测试部门', NULL, 3, NULL, '15888888888', 'xxx@qq.com', '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL),
	(1761000000000000106, 1761000000000000101, '0,1761000000000000100,1761000000000000101', '财务部门', NULL, 4, NULL, '15888888888', 'xxx@qq.com', '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL),
	(1761000000000000107, 1761000000000000101, '0,1761000000000000100,1761000000000000101', '运维部门', NULL, 5, NULL, '15888888888', 'xxx@qq.com', '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL),
	(1761000000000000108, 1761000000000000102, '0,1761000000000000100,1761000000000000102', '市场部门', NULL, 1, NULL, '15888888888', 'xxx@qq.com', '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL),
	(1761000000000000109, 1761000000000000102, '0,1761000000000000100,1761000000000000102', '财务部门', NULL, 2, NULL, '15888888888', 'xxx@qq.com', '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL);

-- 导出  表 rdc.sys_dict_data 结构
CREATE TABLE IF NOT EXISTS `sys_dict_data` (
  `dict_code` bigint(20) NOT NULL COMMENT '字典编码',
  `dict_sort` int(4) DEFAULT 0 COMMENT '字典排序',
  `dict_label` varchar(100) DEFAULT '' COMMENT '字典标签',
  `dict_value` varchar(100) DEFAULT '' COMMENT '字典键值',
  `dict_type` varchar(100) DEFAULT '' COMMENT '字典类型',
  `css_class` varchar(100) DEFAULT NULL COMMENT '样式属性（其他样式扩展）',
  `list_class` varchar(100) DEFAULT NULL COMMENT '表格回显样式',
  `is_default` char(1) DEFAULT 'N' COMMENT '是否默认（Y是 N否）',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `remark` varchar(500) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY (`dict_code`),
  KEY `idx_sys_dict_data_type` (`dict_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='字典数据表';

-- 正在导出表  rdc.sys_dict_data 的数据：~34 rows (大约)
INSERT INTO `sys_dict_data` (`dict_code`, `dict_sort`, `dict_label`, `dict_value`, `dict_type`, `css_class`, `list_class`, `is_default`, `create_dept`, `create_by`, `create_time`, `update_by`, `update_time`, `remark`) VALUES
	(1761600000000000001, 1, '男', '0', 'sys_user_gender', '', '', 'Y', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '性别男'),
	(1761600000000000002, 2, '女', '1', 'sys_user_gender', '', '', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '性别女'),
	(1761600000000000003, 3, '未知', '2', 'sys_user_gender', '', '', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '性别未知'),
	(1761600000000000004, 1, '显示', '0', 'sys_show_hide', '', 'primary', 'Y', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '显示菜单'),
	(1761600000000000005, 2, '隐藏', '1', 'sys_show_hide', '', 'danger', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '隐藏菜单'),
	(1761600000000000006, 1, '正常', '0', 'sys_normal_disable', '', 'primary', 'Y', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '正常状态'),
	(1761600000000000007, 2, '停用', '1', 'sys_normal_disable', '', 'danger', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '停用状态'),
	(1761600000000000012, 1, '是', 'Y', 'sys_yes_no', '', 'primary', 'Y', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '系统默认是'),
	(1761600000000000013, 2, '否', 'N', 'sys_yes_no', '', 'danger', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '系统默认否'),
	(1761600000000000014, 1, '通知', '1', 'sys_notice_type', '', 'warning', 'Y', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '通知'),
	(1761600000000000015, 2, '公告', '2', 'sys_notice_type', '', 'success', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '公告'),
	(1761600000000000016, 1, '正常', '0', 'sys_notice_status', '', 'primary', 'Y', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '正常状态'),
	(1761600000000000017, 2, '关闭', '1', 'sys_notice_status', '', 'danger', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '关闭状态'),
	(1761600000000000018, 1, '新增', '1', 'sys_oper_type', '', 'info', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '新增操作'),
	(1761600000000000019, 2, '修改', '2', 'sys_oper_type', '', 'info', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '修改操作'),
	(1761600000000000020, 3, '删除', '3', 'sys_oper_type', '', 'danger', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '删除操作'),
	(1761600000000000021, 4, '授权', '4', 'sys_oper_type', '', 'primary', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '授权操作'),
	(1761600000000000022, 5, '导出', '5', 'sys_oper_type', '', 'warning', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '导出操作'),
	(1761600000000000023, 6, '导入', '6', 'sys_oper_type', '', 'warning', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '导入操作'),
	(1761600000000000024, 7, '强退', '7', 'sys_oper_type', '', 'danger', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '强退操作'),
	(1761600000000000025, 8, '生成代码', '8', 'sys_oper_type', '', 'warning', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '生成操作'),
	(1761600000000000026, 9, '清空数据', '9', 'sys_oper_type', '', 'danger', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '清空操作'),
	(1761600000000000027, 1, '成功', '0', 'sys_common_status', '', 'primary', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '正常状态'),
	(1761600000000000028, 2, '失败', '1', 'sys_common_status', '', 'danger', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '停用状态'),
	(1761600000000000029, 99, '其他', '0', 'sys_oper_type', '', 'info', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '其他操作'),
	(1761600000000000030, 0, '密码认证', 'password', 'sys_grant_type', 'el-check-tag', 'default', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '密码认证'),
	(1761600000000000031, 0, '短信认证', 'sms', 'sys_grant_type', 'el-check-tag', 'default', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '短信认证'),
	(1761600000000000032, 0, '邮件认证', 'email', 'sys_grant_type', 'el-check-tag', 'default', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '邮件认证'),
	(1761600000000000033, 0, '小程序认证', 'xcx', 'sys_grant_type', 'el-check-tag', 'default', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '小程序认证'),
	(1761600000000000034, 0, '三方登录认证', 'social', 'sys_grant_type', 'el-check-tag', 'default', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '三方登录认证'),
	(1761600000000000035, 0, 'PC', 'pc', 'sys_device_type', '', 'default', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, 'PC'),
	(1761600000000000036, 0, '安卓', 'android', 'sys_device_type', '', 'default', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '安卓'),
	(1761600000000000037, 0, 'iOS', 'ios', 'sys_device_type', '', 'default', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, 'iOS'),
	(1761600000000000038, 0, '小程序', 'xcx', 'sys_device_type', '', 'default', 'N', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '小程序');

-- 导出  表 rdc.sys_dict_type 结构
CREATE TABLE IF NOT EXISTS `sys_dict_type` (
  `dict_id` bigint(20) NOT NULL COMMENT '字典主键',
  `dict_name` varchar(100) DEFAULT '' COMMENT '字典名称',
  `dict_type` varchar(100) DEFAULT '' COMMENT '字典类型',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `remark` varchar(500) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY (`dict_id`),
  UNIQUE KEY `dict_type` (`dict_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='字典类型表';

-- 正在导出表  rdc.sys_dict_type 的数据：~10 rows (大约)
INSERT INTO `sys_dict_type` (`dict_id`, `dict_name`, `dict_type`, `create_dept`, `create_by`, `create_time`, `update_by`, `update_time`, `remark`) VALUES
	(1761500000000000001, '用户性别', 'sys_user_gender', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '用户性别列表'),
	(1761500000000000002, '菜单状态', 'sys_show_hide', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '菜单状态列表'),
	(1761500000000000003, '系统开关', 'sys_normal_disable', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '系统开关列表'),
	(1761500000000000006, '系统是否', 'sys_yes_no', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '系统是否列表'),
	(1761500000000000007, '通知类型', 'sys_notice_type', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '通知类型列表'),
	(1761500000000000008, '通知状态', 'sys_notice_status', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '通知状态列表'),
	(1761500000000000009, '操作类型', 'sys_oper_type', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '操作类型列表'),
	(1761500000000000010, '系统状态', 'sys_common_status', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '登录状态列表'),
	(1761500000000000011, '授权类型', 'sys_grant_type', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '认证授权类型'),
	(1761500000000000012, '设备类型', 'sys_device_type', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '客户端设备类型');

-- 导出  表 rdc.sys_login_info 结构
CREATE TABLE IF NOT EXISTS `sys_login_info` (
  `info_id` bigint(20) NOT NULL COMMENT '访问ID',
  `user_name` varchar(50) DEFAULT '' COMMENT '用户账号',
  `client_key` varchar(32) DEFAULT '' COMMENT '客户端',
  `device_type` varchar(32) DEFAULT '' COMMENT '设备类型',
  `ipaddr` varchar(128) DEFAULT '' COMMENT '登录IP地址',
  `login_location` varchar(255) DEFAULT '' COMMENT '登录地点',
  `browser` varchar(50) DEFAULT '' COMMENT '浏览器类型',
  `os` varchar(50) DEFAULT '' COMMENT '操作系统',
  `status` char(1) DEFAULT '0' COMMENT '登录状态（0正常 1异常）',
  `msg` varchar(255) DEFAULT '' COMMENT '提示消息',
  `login_time` datetime DEFAULT NULL COMMENT '访问时间',
  PRIMARY KEY (`info_id`),
  KEY `idx_sys_login_info_s` (`status`),
  KEY `idx_sys_login_info_lt` (`login_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='系统访问记录';

-- 正在导出表  rdc.sys_login_info 的数据：~2 rows (大约)
INSERT INTO `sys_login_info` (`info_id`, `user_name`, `client_key`, `device_type`, `ipaddr`, `login_location`, `browser`, `os`, `status`, `msg`, `login_time`) VALUES
	(98660906371780608, 'admin', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', '::1', '内网IP', 'Chrome', 'Windows', '0', '登录成功', '2026-09-30 14:03:13'),
	(99709816221601792, 'admin', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', '::1', '内网IP', 'Chrome', 'Windows', '0', '登录成功', '2026-10-03 11:31:12');

-- 导出  表 rdc.sys_menu 结构
CREATE TABLE IF NOT EXISTS `sys_menu` (
  `menu_id` bigint(20) NOT NULL COMMENT '菜单ID',
  `menu_name` varchar(50) NOT NULL COMMENT '菜单名称',
  `parent_id` bigint(20) DEFAULT 0 COMMENT '父菜单ID',
  `order_num` int(4) DEFAULT 0 COMMENT '显示顺序',
  `path` varchar(200) DEFAULT '' COMMENT '路由地址',
  `component` varchar(255) DEFAULT NULL COMMENT '组件路径',
  `query_param` varchar(255) DEFAULT NULL COMMENT '路由参数',
  `is_frame` char(1) DEFAULT 'N' COMMENT '是否为外链（Y是 N否）',
  `is_cache` char(1) DEFAULT 'Y' COMMENT '是否缓存（Y缓存 N不缓存）',
  `menu_type` char(1) DEFAULT '' COMMENT '菜单类型（M目录 C菜单 F按钮）',
  `visible` char(1) DEFAULT '0' COMMENT '显示状态（0显示 1隐藏）',
  `status` char(1) DEFAULT '0' COMMENT '菜单状态（0正常 1停用）',
  `perms` varchar(100) DEFAULT NULL COMMENT '权限标识',
  `icon` varchar(100) DEFAULT '#' COMMENT '菜单图标',
  `active_menu` varchar(255) DEFAULT '' COMMENT '激活菜单路径',
  `ext` varchar(2000) DEFAULT '' COMMENT '扩展字段',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `remark` varchar(500) DEFAULT '' COMMENT '备注',
  PRIMARY KEY (`menu_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='菜单权限表';

-- 正在导出表  rdc.sys_menu 的数据：~99 rows (大约)
INSERT INTO `sys_menu` (`menu_id`, `menu_name`, `parent_id`, `order_num`, `path`, `component`, `query_param`, `is_frame`, `is_cache`, `menu_type`, `visible`, `status`, `perms`, `icon`, `active_menu`, `ext`, `create_dept`, `create_by`, `create_time`, `update_by`, `update_time`, `remark`) VALUES
	(92793505776275456, '巡检管理', 0, 40, 'rdc', NULL, NULL, 'N', 'Y', 'M', '0', '0', NULL, 'international', '', NULL, 1761000000000000103, 1761100000000000001, '2026-09-14 09:28:15', 1761100000000000001, NULL, ''),
	(92794566889050112, '任务编辑', 92793505776275456, 20, 'rdc/patroledit', 'rdc/patroledit/index', NULL, 'N', 'Y', 'C', '0', '0', NULL, '', '', NULL, 1761000000000000103, 1761100000000000001, '2026-09-14 09:32:28', 1761100000000000001, NULL, ''),
	(92795533978112000, '实时监控', 92793505776275456, 60, 'rdc/dogmonitor', 'rdc/dogmonitor/index', NULL, 'N', 'Y', 'C', '1', '0', NULL, '', '', NULL, 1761000000000000103, 1761100000000000001, '2026-09-14 09:36:19', 1761100000000000001, NULL, ''),
	(92795796579291136, '任务记录', 92793505776275456, 30, 'rdc/tasklog', 'rdc/tasklog/index', NULL, 'N', 'Y', 'C', '0', '0', NULL, '', '', NULL, 1761000000000000103, 1761100000000000001, '2026-09-14 09:37:21', NULL, NULL, ''),
	(92796042914959360, '任务结果', 92793505776275456, 50, 'rdc/taskitemexeclog', 'rdc/taskitemexeclog/index', NULL, 'N', 'Y', 'C', '0', '0', NULL, '', '', NULL, 1761000000000000103, 1761100000000000001, '2026-09-14 09:38:20', 1761100000000000001, NULL, ''),
	(92799148981620736, '地图管理', 92793505776275456, 10, 'rdc/mapmgr', 'rdc/mapmgr/index', NULL, 'N', 'Y', 'C', '0', '0', NULL, '', '', NULL, 1761000000000000103, 1761100000000000001, '2026-09-14 09:50:41', 1761100000000000001, NULL, ''),
	(92846003241226240, '路线管理', 92793505776275456, 15, 'rdc/pathmgr', 'rdc/pathmgr/index', NULL, 'N', 'Y', 'C', '0', '0', NULL, '', '', NULL, 1761000000000000103, 1761100000000000001, '2026-09-14 12:56:52', 1761100000000000001, NULL, ''),
	(1761400000000000001, '系统管理', 0, 1, 'system', NULL, '', 'N', 'Y', 'M', '0', '0', '', 'system', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '系统管理目录'),
	(1761400000000000002, '系统监控', 0, 3, 'monitor', NULL, '', 'N', 'Y', 'M', '0', '0', '', 'monitor', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '系统监控目录'),
	(1761400000000000004, '官网', 0, 100, 'https://www.tianyancha.com/company/2346125636', NULL, '', 'Y', 'Y', 'M', '0', '0', '', 'guide', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', 1761100000000000001, NULL, '官网地址'),
	(1761400000000000005, '测试菜单', 0, 50, 'demo', NULL, '', 'N', 'Y', 'M', '1', '0', '', 'star', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', 1761100000000000001, NULL, '测试菜单'),
	(1761400000000000100, '用户管理', 1761400000000000001, 1, 'user', 'system/user/index', '', 'N', 'Y', 'C', '0', '0', 'system:user:list', 'user', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '用户管理菜单'),
	(1761400000000000101, '角色管理', 1761400000000000001, 2, 'role', 'system/role/index', '', 'N', 'Y', 'C', '0', '0', 'system:role:list', 'peoples', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '角色管理菜单'),
	(1761400000000000102, '菜单管理', 1761400000000000001, 3, 'menu', 'system/menu/index', '', 'N', 'Y', 'C', '0', '0', 'system:menu:list', 'tree-table', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '菜单管理菜单'),
	(1761400000000000103, '部门管理', 1761400000000000001, 4, 'dept', 'system/dept/index', '', 'N', 'Y', 'C', '0', '0', 'system:dept:list', 'tree', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '部门管理菜单'),
	(1761400000000000104, '岗位管理', 1761400000000000001, 5, 'post', 'system/post/index', '', 'N', 'Y', 'C', '0', '0', 'system:post:list', 'post', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '岗位管理菜单'),
	(1761400000000000105, '字典管理', 1761400000000000001, 6, 'dict', 'system/dict/index', '', 'N', 'Y', 'C', '0', '0', 'system:dict:list', 'dict', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '字典管理菜单'),
	(1761400000000000106, '参数设置', 1761400000000000001, 7, 'config', 'system/config/index', '', 'N', 'Y', 'C', '0', '0', 'system:config:list', 'edit', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '参数设置菜单'),
	(1761400000000000107, '通知公告', 1761400000000000001, 8, 'notice', 'system/notice/index', '', 'N', 'Y', 'C', '0', '0', 'system:notice:list', 'message', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '通知公告菜单'),
	(1761400000000000108, '日志管理', 1761400000000000001, 9, 'log', '', '', 'N', 'Y', 'M', '0', '0', '', 'log', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '日志管理菜单'),
	(1761400000000000109, '在线用户', 1761400000000000002, 1, 'online', 'monitor/online/index', '', 'N', 'Y', 'C', '0', '0', 'monitor:online:list', 'online', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '在线用户菜单'),
	(1761400000000000113, '缓存监控', 1761400000000000002, 5, 'cache', 'monitor/cache/index', '', 'N', 'Y', 'C', '0', '0', 'monitor:cache:list', 'redis', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '缓存监控菜单'),
	(1761400000000000118, '文件管理', 1761400000000000001, 10, 'oss', 'system/oss/index', '', 'N', 'Y', 'C', '0', '0', 'system:oss:list', 'upload', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '文件管理菜单'),
	(1761400000000000123, '客户端管理', 1761400000000000001, 11, 'client', 'system/client/index', '', 'N', 'Y', 'C', '0', '0', 'system:client:list', 'international', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '客户端管理菜单'),
	(1761400000000000130, '分配用户', 1761400000000000001, 2, 'role-auth/user/:roleId', 'system/role/authUser', '', 'N', 'N', 'C', '1', '0', 'system:role:edit', '#', '/system/role', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000000131, '分配角色', 1761400000000000001, 1, 'user-auth/role/:userId', 'system/user/authRole', '', 'N', 'N', 'C', '1', '0', 'system:user:edit', '#', '/system/user', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000000133, '文件配置管理', 1761400000000000001, 10, 'oss-config/index', 'system/oss/config', '', 'N', 'N', 'C', '1', '0', 'system:ossConfig:list', '#', '/system/oss', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000000500, '操作日志', 1761400000000000108, 1, 'operlog', 'monitor/operlog/index', '', 'N', 'Y', 'C', '0', '0', 'monitor:operlog:list', 'form', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '操作日志菜单'),
	(1761400000000000501, '登录日志', 1761400000000000108, 2, 'logininfo', 'monitor/logininfo/index', '', 'N', 'Y', 'C', '0', '0', 'monitor:logininfo:list', 'logininfo', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '登录日志菜单'),
	(1761400000000001001, '用户查询', 1761400000000000100, 1, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:user:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001002, '用户新增', 1761400000000000100, 2, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:user:add', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001003, '用户修改', 1761400000000000100, 3, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:user:edit', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001004, '用户删除', 1761400000000000100, 4, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:user:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001005, '用户导出', 1761400000000000100, 5, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:user:export', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001006, '用户导入', 1761400000000000100, 6, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:user:import', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001007, '重置密码', 1761400000000000100, 7, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:user:resetPwd', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001008, '角色查询', 1761400000000000101, 1, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:role:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001009, '角色新增', 1761400000000000101, 2, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:role:add', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001010, '角色修改', 1761400000000000101, 3, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:role:edit', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001011, '角色删除', 1761400000000000101, 4, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:role:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001012, '角色导出', 1761400000000000101, 5, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:role:export', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001013, '菜单查询', 1761400000000000102, 1, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:menu:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001014, '菜单新增', 1761400000000000102, 2, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:menu:add', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001015, '菜单修改', 1761400000000000102, 3, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:menu:edit', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001016, '菜单删除', 1761400000000000102, 4, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:menu:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001017, '部门查询', 1761400000000000103, 1, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:dept:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001018, '部门新增', 1761400000000000103, 2, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:dept:add', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001019, '部门修改', 1761400000000000103, 3, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:dept:edit', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001020, '部门删除', 1761400000000000103, 4, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:dept:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001021, '岗位查询', 1761400000000000104, 1, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:post:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001022, '岗位新增', 1761400000000000104, 2, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:post:add', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001023, '岗位修改', 1761400000000000104, 3, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:post:edit', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001024, '岗位删除', 1761400000000000104, 4, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:post:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001025, '岗位导出', 1761400000000000104, 5, '', '', '', 'N', 'Y', 'F', '0', '0', 'system:post:export', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001026, '字典查询', 1761400000000000105, 1, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:dict:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001027, '字典新增', 1761400000000000105, 2, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:dict:add', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001028, '字典修改', 1761400000000000105, 3, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:dict:edit', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001029, '字典删除', 1761400000000000105, 4, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:dict:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001030, '字典导出', 1761400000000000105, 5, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:dict:export', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001031, '参数查询', 1761400000000000106, 1, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:config:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001032, '参数新增', 1761400000000000106, 2, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:config:add', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001033, '参数修改', 1761400000000000106, 3, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:config:edit', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001034, '参数删除', 1761400000000000106, 4, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:config:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001035, '参数导出', 1761400000000000106, 5, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:config:export', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001036, '公告查询', 1761400000000000107, 1, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:notice:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001037, '公告新增', 1761400000000000107, 2, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:notice:add', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001038, '公告修改', 1761400000000000107, 3, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:notice:edit', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001039, '公告删除', 1761400000000000107, 4, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:notice:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001040, '操作查询', 1761400000000000500, 1, '#', '', '', 'N', 'Y', 'F', '0', '0', 'monitor:operlog:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001041, '操作删除', 1761400000000000500, 2, '#', '', '', 'N', 'Y', 'F', '0', '0', 'monitor:operlog:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001042, '日志导出', 1761400000000000500, 4, '#', '', '', 'N', 'Y', 'F', '0', '0', 'monitor:operlog:export', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001043, '登录查询', 1761400000000000501, 1, '#', '', '', 'N', 'Y', 'F', '0', '0', 'monitor:logininfo:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001044, '登录删除', 1761400000000000501, 2, '#', '', '', 'N', 'Y', 'F', '0', '0', 'monitor:logininfo:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001045, '日志导出', 1761400000000000501, 3, '#', '', '', 'N', 'Y', 'F', '0', '0', 'monitor:logininfo:export', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001046, '在线查询', 1761400000000000109, 1, '#', '', '', 'N', 'Y', 'F', '0', '0', 'monitor:online:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001047, '批量强退', 1761400000000000109, 2, '#', '', '', 'N', 'Y', 'F', '0', '0', 'monitor:online:batchLogout', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001048, '单条强退', 1761400000000000109, 3, '#', '', '', 'N', 'Y', 'F', '0', '0', 'monitor:online:forceLogout', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001050, '账户解锁', 1761400000000000501, 4, '#', '', '', 'N', 'Y', 'F', '0', '0', 'monitor:logininfo:unlock', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001061, '客户端管理查询', 1761400000000000123, 1, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:client:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001062, '客户端管理新增', 1761400000000000123, 2, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:client:add', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001063, '客户端管理修改', 1761400000000000123, 3, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:client:edit', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001064, '客户端管理删除', 1761400000000000123, 4, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:client:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001065, '客户端管理导出', 1761400000000000123, 5, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:client:export', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001500, '测试单表', 1761400000000000005, 1, 'demo', 'demo/demo/index', '', 'N', 'Y', 'C', '0', '0', 'demo:demo:list', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '测试单表菜单'),
	(1761400000000001501, '测试单表查询', 1761400000000001500, 1, '#', '', '', 'N', 'Y', 'F', '0', '0', 'demo:demo:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001502, '测试单表新增', 1761400000000001500, 2, '#', '', '', 'N', 'Y', 'F', '0', '0', 'demo:demo:add', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001503, '测试单表修改', 1761400000000001500, 3, '#', '', '', 'N', 'Y', 'F', '0', '0', 'demo:demo:edit', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001504, '测试单表删除', 1761400000000001500, 4, '#', '', '', 'N', 'Y', 'F', '0', '0', 'demo:demo:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001505, '测试单表导出', 1761400000000001500, 5, '#', '', '', 'N', 'Y', 'F', '0', '0', 'demo:demo:export', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001506, '测试树表', 1761400000000000005, 1, 'tree', 'demo/tree/index', '', 'N', 'Y', 'C', '0', '0', 'demo:tree:list', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '测试树表菜单'),
	(1761400000000001507, '测试树表查询', 1761400000000001506, 1, '#', '', '', 'N', 'Y', 'F', '0', '0', 'demo:tree:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001508, '测试树表新增', 1761400000000001506, 2, '#', '', '', 'N', 'Y', 'F', '0', '0', 'demo:tree:add', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001509, '测试树表修改', 1761400000000001506, 3, '#', '', '', 'N', 'Y', 'F', '0', '0', 'demo:tree:edit', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001510, '测试树表删除', 1761400000000001506, 4, '#', '', '', 'N', 'Y', 'F', '0', '0', 'demo:tree:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001511, '测试树表导出', 1761400000000001506, 5, '#', '', '', 'N', 'Y', 'F', '0', '0', 'demo:tree:export', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001600, '文件查询', 1761400000000000118, 1, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:oss:query', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001601, '文件上传', 1761400000000000118, 2, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:oss:upload', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001602, '文件下载', 1761400000000000118, 3, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:oss:download', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001603, '文件删除', 1761400000000000118, 4, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:oss:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001620, '配置列表', 1761400000000000118, 5, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:ossConfig:list', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001621, '配置添加', 1761400000000000118, 6, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:ossConfig:add', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001622, '配置编辑', 1761400000000000118, 6, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:ossConfig:edit', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761400000000001623, '配置删除', 1761400000000000118, 6, '#', '', '', 'N', 'Y', 'F', '0', '0', 'system:ossConfig:remove', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761700000000000001, '机器狗地图运维', 92793505776275456, 16, 'rdc/mapops', 'rdc/mapops/index', NULL, 'N', 'N', 'C', '0', '0', NULL, '', '', NULL, 1761000000000000103, 1761100000000000001, '2026-10-03 11:31:54', 1761100000000000001, NULL, '机器狗地图运维（X30 / M20 Pro 双机型）'),
	(1761700000000000011, '地图运维查询', 1761700000000000001, 1, '', '', '', 'N', 'Y', 'F', '0', '0', 'rdc:mapOps:list', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-10-03 11:31:54', NULL, NULL, ''),
	(1761700000000000012, '启动建图', 1761700000000000001, 2, '', '', '', 'N', 'Y', 'F', '0', '0', 'rdc:mapOps:start', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-10-03 11:31:54', NULL, NULL, ''),
	(1761700000000000013, '结束建图', 1761700000000000001, 3, '', '', '', 'N', 'Y', 'F', '0', '0', 'rdc:mapOps:stop', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-10-03 11:31:54', NULL, NULL, ''),
	(1761700000000000014, '打包地图', 1761700000000000001, 4, '', '', '', 'N', 'Y', 'F', '0', '0', 'rdc:mapOps:pack', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-10-03 11:31:54', NULL, NULL, ''),
	(1761700000000000015, '下载产物', 1761700000000000001, 5, '', '', '', 'N', 'Y', 'F', '0', '0', 'rdc:mapOps:download', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-10-03 11:31:54', NULL, NULL, ''),
	(1761700000000000016, '上传产物', 1761700000000000001, 6, '', '', '', 'N', 'Y', 'F', '0', '0', 'rdc:mapOps:upload', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-10-03 11:31:54', NULL, NULL, ''),
	(1761700000000000017, '应用地图', 1761700000000000001, 7, '', '', '', 'N', 'Y', 'F', '0', '0', 'rdc:mapOps:apply', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-10-03 11:31:54', NULL, NULL, ''),
	(1761700000000000018, '自定义命令', 1761700000000000001, 8, '', '', '', 'N', 'Y', 'F', '0', '0', 'rdc:mapOps:custom', '#', '', '', 1761000000000000103, 1761100000000000001, '2026-10-03 11:31:54', NULL, NULL, '');

-- 导出  表 rdc.sys_message 结构
CREATE TABLE IF NOT EXISTS `sys_message` (
  `message_id` bigint(20) NOT NULL COMMENT '消息ID',
  `category` varchar(20) NOT NULL COMMENT '消息分组(system/notice/workflow)',
  `type` varchar(20) NOT NULL COMMENT '消息类型',
  `source` varchar(20) NOT NULL COMMENT '消息来源',
  `title` varchar(100) DEFAULT '' COMMENT '标题',
  `message` varchar(500) DEFAULT '' COMMENT '摘要消息',
  `content` longtext DEFAULT NULL COMMENT '详细内容',
  `data_json` longtext DEFAULT NULL COMMENT '扩展数据JSON',
  `path` varchar(500) DEFAULT NULL COMMENT '前端跳转路径',
  `send_user_ids` varchar(2000) NOT NULL DEFAULT '0' COMMENT '目标用户ID串，0表示全局',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`message_id`),
  KEY `idx_sys_message_category_time` (`category`,`create_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='消息记录表';

-- 正在导出表  rdc.sys_message 的数据：~8 rows (大约)
INSERT INTO `sys_message` (`message_id`, `category`, `type`, `source`, `title`, `message`, `content`, `data_json`, `path`, `send_user_ids`, `create_dept`, `create_by`, `create_time`, `update_by`, `update_time`) VALUES
	(91861999897677824, 'notice', 'notice', 'notice', '通知公告消息', '[公告] x2', '<p>x2</p>', '{"noticeType":"2","noticeTypeLabel":"\\u516C\\u544A","noticeTitle":"x2","noticeId":0,"noticeContent":"\\u003Cp\\u003Ex2\\u003C/p\\u003E","status":"0"}', '/system/notice?noticeId=0', '0', 1761000000000000103, 1761100000000000001, '2026-09-11 19:46:47', NULL, NULL),
	(91862714992955392, 'notice', 'notice', 'notice', '通知公告消息', '[通知] x5', '<p>sss</p>', '{"noticeType":"1","noticeTypeLabel":"\\u901A\\u77E5","noticeTitle":"x5","noticeId":91862699935404032,"noticeContent":"\\u003Cp\\u003Esss\\u003C/p\\u003E","status":"0"}', '/system/notice?noticeId=91862699935404032', '0', 1761000000000000103, 1761100000000000001, '2026-09-11 19:49:37', NULL, NULL),
	(2097845368933957634, 'system', 'message', 'backend', '系统消息', '上午好，欢迎登录 RuoYi-Vue-Plus 后台管理系统', NULL, NULL, NULL, '1761100000000000001', -1, -1, '2026-09-10 08:31:26', -1, '2026-09-10 08:31:26'),
	(2097899867161513985, 'system', 'message', 'backend', '系统消息', '中午好，欢迎登录 RuoYi-Vue-Plus 后台管理系统', NULL, NULL, NULL, '1761100000000000001', -1, -1, '2026-09-10 12:08:00', -1, '2026-09-10 12:08:00'),
	(2097910435926790146, 'system', 'message', 'backend', '系统消息', '中午好，欢迎登录 RuoYi-Vue-Plus 后台管理系统', NULL, NULL, NULL, '1761100000000000001', -1, -1, '2026-09-10 12:49:59', -1, '2026-09-10 12:49:59'),
	(2098001700173402114, 'system', 'message', 'backend', '系统消息', '下午好，欢迎登录 RuoYi-Vue-Plus 后台管理系统', NULL, NULL, NULL, '1761100000000000001', -1, -1, '2026-09-10 18:52:38', -1, '2026-09-10 18:52:38'),
	(2098369045232517121, 'system', 'message', 'backend', '系统消息', '晚上好，欢迎登录 RuoYi-Vue-Plus 后台管理系统', NULL, NULL, NULL, '1761100000000000001', -1, -1, '2026-09-11 19:12:20', -1, '2026-09-11 19:12:20'),
	(2098369231006629891, 'notice', 'notice', 'notice', '通知公告消息', '[通知] 测试', '<p>11</p>', '{"noticeContent":"<p>11</p>","noticeTypeLabel":"通知","noticeType":"1","noticeTitle":"测试","noticeId":"2098369231006629890","status":"0"}', '/system/notice?noticeId=2098369231006629890', '0', 1761000000000000103, 1761100000000000001, '2026-09-11 19:13:05', 1761100000000000001, '2026-09-11 19:13:05'),
	(2098594219030642689, 'system', 'message', 'backend', '系统消息', '上午好，欢迎登录 RuoYi-Vue-Plus 后台管理系统', NULL, NULL, NULL, '1761100000000000001', -1, -1, '2026-09-12 10:07:06', -1, '2026-09-12 10:07:06');

-- 导出  表 rdc.sys_notice 结构
CREATE TABLE IF NOT EXISTS `sys_notice` (
  `notice_id` bigint(20) NOT NULL COMMENT '公告ID',
  `notice_title` varchar(50) NOT NULL COMMENT '公告标题',
  `notice_type` char(1) NOT NULL COMMENT '公告类型（1通知 2公告）',
  `notice_content` longblob DEFAULT NULL COMMENT '公告内容',
  `status` char(1) DEFAULT '0' COMMENT '公告状态（0正常 1关闭）',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `remark` varchar(255) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY (`notice_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='通知公告表';

-- 正在导出表  rdc.sys_notice 的数据：~4 rows (大约)
INSERT INTO `sys_notice` (`notice_id`, `notice_title`, `notice_type`, `notice_content`, `status`, `create_dept`, `create_by`, `create_time`, `update_by`, `update_time`, `remark`) VALUES
	(91862699935404032, 'x5', '1', _binary 0x3c703e7373733c2f703e, '0', 1761000000000000103, 1761100000000000001, '2026-09-11 19:49:34', NULL, NULL, ''),
	(1761800000000000001, '温馨提醒：2018-07-01 新版本发布啦', '2', _binary 0xe696b0e78988e69cace58685e5aeb9, '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '管理员'),
	(1761800000000000002, '维护通知：2018-07-01 系统凌晨维护', '1', _binary 0xe7bbb4e68aa4e58685e5aeb9, '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '管理员'),
	(2098369231006629890, '测试', '1', _binary 0x3c703e31313c2f703e, '0', 1761000000000000103, 1761100000000000001, '2026-09-11 19:13:05', 1761100000000000001, '2026-09-11 19:13:05', '');

-- 导出  表 rdc.sys_oper_log 结构
CREATE TABLE IF NOT EXISTS `sys_oper_log` (
  `oper_id` bigint(20) NOT NULL COMMENT '日志主键',
  `title` varchar(50) DEFAULT '' COMMENT '模块标题',
  `business_type` int(2) DEFAULT 0 COMMENT '业务类型（0其它 1新增 2修改 3删除）',
  `method` varchar(100) DEFAULT '' COMMENT '方法名称',
  `request_method` varchar(10) DEFAULT '' COMMENT '请求方式',
  `operator_type` int(1) DEFAULT 0 COMMENT '操作类别（0其它 1后台用户 2手机端用户）',
  `oper_name` varchar(50) DEFAULT '' COMMENT '操作人员',
  `user_id` bigint(20) DEFAULT NULL COMMENT '操作用户ID',
  `dept_id` bigint(20) DEFAULT NULL COMMENT '操作部门ID',
  `dept_name` varchar(50) DEFAULT '' COMMENT '部门名称',
  `client_key` varchar(32) DEFAULT '' COMMENT '客户端',
  `device_type` varchar(32) DEFAULT '' COMMENT '设备类型',
  `browser` varchar(50) DEFAULT '' COMMENT '浏览器类型',
  `os` varchar(50) DEFAULT '' COMMENT '操作系统',
  `oper_url` varchar(255) DEFAULT '' COMMENT '请求URL',
  `oper_ip` varchar(128) DEFAULT '' COMMENT '主机地址',
  `oper_location` varchar(255) DEFAULT '' COMMENT '操作地点',
  `oper_param` varchar(4000) DEFAULT '' COMMENT '请求参数',
  `json_result` varchar(4000) DEFAULT '' COMMENT '返回参数',
  `status` int(1) DEFAULT 0 COMMENT '操作状态（0正常 1异常）',
  `error_msg` varchar(4000) DEFAULT '' COMMENT '错误消息',
  `oper_time` datetime DEFAULT NULL COMMENT '操作时间',
  `cost_time` bigint(20) DEFAULT 0 COMMENT '消耗时间',
  PRIMARY KEY (`oper_id`),
  KEY `idx_sys_oper_log_bt` (`business_type`),
  KEY `idx_sys_oper_log_uid` (`user_id`),
  KEY `idx_sys_oper_log_s` (`status`),
  KEY `idx_sys_oper_log_ot` (`oper_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='操作日志记录';

-- 正在导出表  rdc.sys_oper_log 的数据：~110 rows (大约)
INSERT INTO `sys_oper_log` (`oper_id`, `title`, `business_type`, `method`, `request_method`, `operator_type`, `oper_name`, `user_id`, `dept_id`, `dept_name`, `client_key`, `device_type`, `browser`, `os`, `oper_url`, `oper_ip`, `oper_location`, `oper_param`, `json_result`, `status`, `error_msg`, `oper_time`, `cost_time`) VALUES
	(92550322622435328, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-13 17:21:56', 30),
	(92550322689544192, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-13 17:21:56', 1),
	(92550322693738496, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-13 17:21:56', 1),
	(92550365437890560, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_user_gender', '::1', '内网IP', '{"dictType":"sys_user_gender"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000001,"dictSort":1,"dictLabel":"男","dictValue":"0","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"Y","remark":"性别男","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000002,"dictSort":2,"dictLabel":"女","dictValue":"1","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别女","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000003,"dictSort":3,"dictLabel":"未知","dictValue":"2","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别未知","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-13 17:22:06', 1),
	(92759553321603072, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/0', '::1', '内网IP', '{"ossIds":"0"}', NULL, 1, 'Value cannot be null. (Parameter \'path1\')', '2026-09-14 07:13:20', 1152),
	(92759577929584640, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/0', '::1', '内网IP', '{"ossIds":"0"}', NULL, 1, 'Value cannot be null. (Parameter \'path1\')', '2026-09-14 07:13:26', 1578),
	(92759754987933696, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/0', '::1', '内网IP', '{"ossIds":"0"}', NULL, 1, 'Value cannot be null. (Parameter \'path1\')', '2026-09-14 07:14:08', 1123),
	(92761633662832640, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/0', '::1', '内网IP', '{"ossIds":"0"}', NULL, 1, 'Value cannot be null. (Parameter \'path1\')', '2026-09-14 07:21:36', 155769),
	(92761701128212480, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/0', '::1', '内网IP', '{"ossIds":"0"}', NULL, 1, 'Value cannot be null. (Parameter \'path1\')', '2026-09-14 07:21:52', 11310),
	(92761735810912256, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/0', '::1', '内网IP', '{"ossIds":"0"}', NULL, 1, 'Value cannot be null. (Parameter \'path1\')', '2026-09-14 07:22:01', 1635),
	(92761777737175040, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/0', '::1', '内网IP', '{"ossIds":"0"}', NULL, 1, 'Value cannot be null. (Parameter \'path1\')', '2026-09-14 07:22:11', 1681),
	(92761887036542976, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"配料说明.txt\\"","contentType":"text/plain","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"配料说明.txt\\""],"Content-Type":["text/plain"]},"length":152,"name":"file","fileName":"配料说明.txt"}}', NULL, 1, 'Value cannot be null. (Parameter \'path1\')', '2026-09-14 07:22:37', 2393),
	(92766200597188608, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"配料说明.txt\\"","contentType":"text/plain","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"配料说明.txt\\""],"Content-Type":["text/plain"]},"length":152,"name":"file","fileName":"配料说明.txt"}}', '{"code":200,"msg":"success","data":{"url":"http://127.0.0.1:7050/api/s3/ruoyi/2026-09-14/7c55ce7ff4894438b08fac5a652c3af2.txt","fileName":"配料说明.txt","ossId":"92766199812853760"}}', 0, NULL, '2026-09-14 07:39:45', 181832),
	(92766529447399424, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/92766199812853760', '::1', '内网IP', '{"ossIds":"92766199812853760"}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 07:41:04', 64572),
	(92766566965448704, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/92766199812853760', '::1', '内网IP', '{"ossIds":"92766199812853760"}', '{"code":110,"msg":"数据库操作失败"}', 0, NULL, '2026-09-14 07:41:13', 2),
	(92766709584367616, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"配料说明.txt\\"","contentType":"text/plain","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"配料说明.txt\\""],"Content-Type":["text/plain"]},"length":152,"name":"file","fileName":"配料说明.txt"}}', '{"code":200,"msg":"success","data":{"url":"http://127.0.0.1:7050/api/s3/ruoyi/2026-09-14/22067f7edca74d3fb1af0c4be7ca05cf.txt","fileName":"配料说明.txt","ossId":"92766709580173312"}}', 0, NULL, '2026-09-14 07:41:47', 2177),
	(92766936202612736, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"Copilot_20260823_104314.jpg\\"","contentType":"image/jpeg","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"Copilot_20260823_104314.jpg\\""],"Content-Type":["image/jpeg"]},"length":69527,"name":"file","fileName":"Copilot_20260823_104314.jpg"}}', '{"code":200,"msg":"success","data":{"url":"http://127.0.0.1:7050/api/s3/ruoyi/2026-09-14/37cd9e783c3e40279af4774426e62e27.jpg","fileName":"Copilot_20260823_104314.jpg","ossId":"92766936198418432"}}', 0, NULL, '2026-09-14 07:42:41', 2168),
	(92769950376267776, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\"","contentType":"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\""],"Content-Type":["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]},"length":8708,"name":"file","fileName":"资源申请表(王猛)_2026080828.xlsx"}}', NULL, 1, 'The bucket you are attempting to access must be addressed using the specified endpoint. Please send all future requests to this endpoint.', '2026-09-14 07:54:39', 883),
	(92770223576453120, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\"","contentType":"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\""],"Content-Type":["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]},"length":8708,"name":"file","fileName":"资源申请表(王猛)_2026080828.xlsx"}}', NULL, 1, 'The bucket you are attempting to access must be addressed using the specified endpoint. Please send all future requests to this endpoint.', '2026-09-14 07:55:44', 320),
	(92770991138279424, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\"","contentType":"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\""],"Content-Type":["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]},"length":8708,"name":"file","fileName":"资源申请表(王猛)_2026080828.xlsx"}}', NULL, 1, 'The bucket you are attempting to access must be addressed using the specified endpoint. Please send all future requests to this endpoint.', '2026-09-14 07:58:47', 1022),
	(92771045840392192, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"配料说明.txt\\"","contentType":"text/plain","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"配料说明.txt\\""],"Content-Type":["text/plain"]},"length":152,"name":"file","fileName":"配料说明.txt"}}', NULL, 1, 'The bucket you are attempting to access must be addressed using the specified endpoint. Please send all future requests to this endpoint.', '2026-09-14 07:59:00', 386),
	(92771763645190144, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"20260824边界通道业务接入_技术方案-临安数智非现场执法(1)(2).doc\\"","contentType":"application/msword","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"20260824边界通道业务接入_技术方案-临安数智非现场执法(1)(2).doc\\""],"Content-Type":["application/msword"]},"length":30208,"name":"file","fileName":"20260824边界通道业务接入_技术方案-临安数智非现场执法(1)(2).doc"}}', NULL, 1, 'The bucket you are attempting to access must be addressed using the specified endpoint. Please send all future requests to this endpoint.', '2026-09-14 08:01:51', 1189),
	(92772080424194048, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\"","contentType":"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\""],"Content-Type":["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]},"length":8708,"name":"file","fileName":"资源申请表(王猛)_2026080828.xlsx"}}', NULL, 1, 'The bucket you are attempting to access must be addressed using the specified endpoint. Please send all future requests to this endpoint.', '2026-09-14 08:03:07', 1329),
	(92772404954271744, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\"","contentType":"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\""],"Content-Type":["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]},"length":8708,"name":"file","fileName":"资源申请表(王猛)_2026080828.xlsx"}}', NULL, 1, 'The bucket you are attempting to access must be addressed using the specified endpoint. Please send all future requests to this endpoint.', '2026-09-14 08:04:24', 1504),
	(92773222575116288, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\"","contentType":"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\""],"Content-Type":["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]},"length":8708,"name":"file","fileName":"资源申请表(王猛)_2026080828.xlsx"}}', NULL, 1, '不知道这样的主机。 (s3.local-proxy.amazonaws.com:80)', '2026-09-14 08:07:39', 2869),
	(92774623317463040, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\"","contentType":"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\""],"Content-Type":["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]},"length":8708,"name":"file","fileName":"资源申请表(王猛)_2026080828.xlsx"}}', '{"code":200,"msg":"success","data":{"url":"http://127.0.0.1:7050/api/s3/ruoyi/2026-09-14/1758e32c9c874bc5b26790673d739a68.xlsx","fileName":"资源申请表(王猛)_2026080828.xlsx","ossId":"92774622466019328"}}', 0, NULL, '2026-09-14 08:13:13', 80456),
	(92774719320887296, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/92774622466019328', '::1', '内网IP', '{"ossIds":"92774622466019328"}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 08:13:36', 2955),
	(92774775075770368, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/92766709580173312', '::1', '内网IP', '{"ossIds":"92766709580173312"}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 08:13:50', 3105),
	(92774821754179584, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"20260824边界通道业务接入_技术方案-临安数智非现场执法(1)(2).doc\\"","contentType":"application/msword","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"20260824边界通道业务接入_技术方案-临安数智非现场执法(1)(2).doc\\""],"Content-Type":["application/msword"]},"length":30208,"name":"file","fileName":"20260824边界通道业务接入_技术方案-临安数智非现场执法(1)(2).doc"}}', '{"code":200,"msg":"success","data":{"url":"http://127.0.0.1:7050/api/s3/ruoyi/2026-09-14/19e156812aef457890c5564f3a0b1d9b.doc","fileName":"20260824边界通道业务接入_技术方案-临安数智非现场执法(1)(2).doc","ossId":"92774821749985280"}}', 0, NULL, '2026-09-14 08:14:01', 4),
	(92775231067918336, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"20260824边界通道业务接入_技术方案-临安数智非现场执法(1)(2).doc\\"","contentType":"application/msword","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"20260824边界通道业务接入_技术方案-临安数智非现场执法(1)(2).doc\\""],"Content-Type":["application/msword"]},"length":30208,"name":"file","fileName":"20260824边界通道业务接入_技术方案-临安数智非现场执法(1)(2).doc"}}', '{"code":200,"msg":"success","data":{"url":"http://127.0.0.1:7050/api/s3/ruoyi/2026-09-14/438f3d1e56994460b63f457c281da0f1.doc","fileName":"20260824边界通道业务接入_技术方案-临安数智非现场执法(1)(2).doc","ossId":"92775230283583488"}}', 0, NULL, '2026-09-14 08:15:38', 7417),
	(92775481199431680, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\"","contentType":"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\""],"Content-Type":["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]},"length":8708,"name":"file","fileName":"资源申请表(王猛)_2026080828.xlsx"}}', NULL, 1, '不支持的存储桶操作', '2026-09-14 08:16:38', 328),
	(92776750232244224, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\"","contentType":"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"资源申请表(王猛)_2026080828.xlsx\\""],"Content-Type":["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]},"length":8708,"name":"file","fileName":"资源申请表(王猛)_2026080828.xlsx"}}', '{"code":200,"msg":"success","data":{"url":"http://127.0.0.1:7050/api/s3/ruoyi/2026-09-14/00ee54f46646420dbee3c8fd6a905af2.xlsx","fileName":"资源申请表(王猛)_2026080828.xlsx","ossId":"92776750039306240"}}', 0, NULL, '2026-09-14 08:21:40', 2198),
	(92776788513656832, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/92774821749985280', '::1', '内网IP', '{"ossIds":"92774821749985280"}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 08:21:50', 20),
	(92776807442550784, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/92775230283583488', '::1', '内网IP', '{"ossIds":"92775230283583488"}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 08:21:54', 5),
	(92776824471425024, 'OSS对象存储', 3, 'sevencat.ruoyi.sys.controller.SysOssController.Remove()', 'DELETE', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/92776750039306240', '::1', '内网IP', '{"ossIds":"92776750039306240"}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 08:21:58', 4),
	(92776847636566016, 'OSS对象存储', 1, 'sevencat.ruoyi.sys.controller.SysOssController.Upload()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/resource/oss/upload', '::1', '内网IP', '{"file":{"contentDisposition":"form-data; name=\\"file\\"; filename=\\"图片合成样例.jpg\\"","contentType":"image/jpeg","headers":{"Content-Disposition":["form-data; name=\\"file\\"; filename=\\"图片合成样例.jpg\\""],"Content-Type":["image/jpeg"]},"length":403056,"name":"file","fileName":"图片合成样例.jpg"}}', '{"code":200,"msg":"success","data":{"url":"http://127.0.0.1:7050/api/s3/ruoyi/2026-09-14/ebeb90dcae8c4d1cb691e594e8f3080c.jpg","fileName":"图片合成样例.jpg","ossId":"92776847628177408"}}', 0, NULL, '2026-09-14 08:22:04', 22),
	(92787290690162688, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_device_type', '::1', '内网IP', '{"dictType":"sys_device_type"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000035,"dictSort":0,"dictLabel":"PC","dictValue":"pc","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"PC","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000036,"dictSort":0,"dictLabel":"安卓","dictValue":"android","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"安卓","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000037,"dictSort":0,"dictLabel":"iOS","dictValue":"ios","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"iOS","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000038,"dictSort":0,"dictLabel":"小程序","dictValue":"xcx","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"小程序","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:03:33', 14),
	(92787290799214592, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_user_gender', '::1', '内网IP', '{"dictType":"sys_user_gender"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000001,"dictSort":1,"dictLabel":"男","dictValue":"0","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"Y","remark":"性别男","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000002,"dictSort":2,"dictLabel":"女","dictValue":"1","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别女","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000003,"dictSort":3,"dictLabel":"未知","dictValue":"2","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别未知","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:03:33', 121),
	(92788423030607872, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_user_gender', '::1', '内网IP', '{"dictType":"sys_user_gender"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000001,"dictSort":1,"dictLabel":"男","dictValue":"0","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"Y","remark":"性别男","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000002,"dictSort":2,"dictLabel":"女","dictValue":"1","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别女","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000003,"dictSort":3,"dictLabel":"未知","dictValue":"2","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别未知","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:08:03', 23),
	(92788423034802176, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_device_type', '::1', '内网IP', '{"dictType":"sys_device_type"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000035,"dictSort":0,"dictLabel":"PC","dictValue":"pc","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"PC","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000036,"dictSort":0,"dictLabel":"安卓","dictValue":"android","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"安卓","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000037,"dictSort":0,"dictLabel":"iOS","dictValue":"ios","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"iOS","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000038,"dictSort":0,"dictLabel":"小程序","dictValue":"xcx","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"小程序","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:08:03', 52),
	(92788609060573184, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_user_gender', '::1', '内网IP', '{"dictType":"sys_user_gender"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000001,"dictSort":1,"dictLabel":"男","dictValue":"0","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"Y","remark":"性别男","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000002,"dictSort":2,"dictLabel":"女","dictValue":"1","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别女","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000003,"dictSort":3,"dictLabel":"未知","dictValue":"2","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别未知","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:08:48', 33),
	(92788609064767488, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_device_type', '::1', '内网IP', '{"dictType":"sys_device_type"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000035,"dictSort":0,"dictLabel":"PC","dictValue":"pc","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"PC","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000036,"dictSort":0,"dictLabel":"安卓","dictValue":"android","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"安卓","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000037,"dictSort":0,"dictLabel":"iOS","dictValue":"ios","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"iOS","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000038,"dictSort":0,"dictLabel":"小程序","dictValue":"xcx","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"小程序","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:08:48', 33),
	(92792424514064384, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:23:58', 1),
	(92792424576978944, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:23:58', 1),
	(92792424883163136, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:23:58', 1),
	(92792519049482240, '菜单管理', 2, 'sevencat.ruoyi.sys.controller.SysMenuController.Edit()', 'PUT', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":1761400000000000004,"parentId":0,"menuName":"官网","orderNum":100,"path":"https://www.tianyancha.com/company/2346125636","component":null,"queryParam":"","isFrame":"Y","isCache":"Y","menuType":"M","visible":"0","status":"0","perms":"","icon":"guide","activeMenu":"","ext":"","remark":"官网地址"}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:24:20', 55),
	(92792557427363840, '菜单管理', 2, 'sevencat.ruoyi.sys.controller.SysMenuController.Edit()', 'PUT', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":1761400000000000005,"parentId":0,"menuName":"测试菜单","orderNum":50,"path":"demo","component":null,"queryParam":"","isFrame":"N","isCache":"Y","menuType":"M","visible":"0","status":"0","perms":"","icon":"star","activeMenu":"","ext":"","remark":"测试菜单"}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:24:29', 4),
	(92793505784664064, '菜单管理', 1, 'sevencat.ruoyi.sys.controller.SysMenuController.Add()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":null,"parentId":0,"menuName":"巡检管理","orderNum":40,"path":"patrol","component":null,"queryParam":null,"isFrame":"N","isCache":"Y","menuType":"M","visible":"0","status":"0","perms":null,"icon":"international","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:28:15', 8),
	(92793532644986880, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:28:22', 1),
	(92793532758233088, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:28:22', 2),
	(92793532762427392, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:28:22', 2),
	(92793602803109888, '菜单管理', 2, 'sevencat.ruoyi.sys.controller.SysMenuController.Edit()', 'PUT', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":1761400000000000005,"parentId":0,"menuName":"测试菜单","orderNum":50,"path":"demo","component":null,"queryParam":"","isFrame":"N","isCache":"Y","menuType":"M","visible":"1","status":"0","perms":"","icon":"star","activeMenu":"","ext":"","remark":"测试菜单"}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:28:38', 3),
	(92793615553794048, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:28:41', 1),
	(92793615562182656, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:28:41', 1),
	(92793615566376960, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:28:41', 1),
	(92794566893244416, '菜单管理', 1, 'sevencat.ruoyi.sys.controller.SysMenuController.Add()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":null,"parentId":92793505776275456,"menuName":"任务监控","orderNum":1,"path":"rdc/patroledit","component":"rdc/patroledit/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"0","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:32:28', 4),
	(92794603786342400, '菜单管理', 2, 'sevencat.ruoyi.sys.controller.SysMenuController.Edit()', 'PUT', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":92793505776275456,"parentId":0,"menuName":"巡检管理","orderNum":40,"path":"rdc","component":null,"queryParam":null,"isFrame":"N","isCache":"Y","menuType":"M","visible":"0","status":"0","perms":null,"icon":"international","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:32:37', 2),
	(92794726092247040, '菜单管理', 2, 'sevencat.ruoyi.sys.controller.SysMenuController.Edit()', 'PUT', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":92794566889050112,"parentId":92793505776275456,"menuName":"任务编辑","orderNum":1,"path":"rdc/patroledit","component":"rdc/patroledit/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"0","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:33:06', 5),
	(92795008805113856, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:34:14', 32),
	(92795008813502464, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:34:14', 32),
	(92795008817696768, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:34:14', 32),
	(92795190758215680, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:34:57', 30),
	(92795190884044800, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:34:57', 30),
	(92795190888239104, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:34:57', 30),
	(92795533982306304, '菜单管理', 1, 'sevencat.ruoyi.sys.controller.SysMenuController.Add()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":null,"parentId":92793505776275456,"menuName":"实时监控","orderNum":1,"path":"rdc/dogmonitor","component":"rdc/dogmonitor/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"0","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:36:19', 4),
	(92795594787131392, '菜单管理', 2, 'sevencat.ruoyi.sys.controller.SysMenuController.Edit()', 'PUT', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":92794566889050112,"parentId":92793505776275456,"menuName":"任务编辑","orderNum":20,"path":"rdc/patroledit","component":"rdc/patroledit/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"0","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:36:33', 2),
	(92795796579291137, '菜单管理', 1, 'sevencat.ruoyi.sys.controller.SysMenuController.Add()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":null,"parentId":92793505776275456,"menuName":"任务记录","orderNum":30,"path":"rdc/tasklog","component":"rdc/tasklog/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"0","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:37:21', 3),
	(92796042919153664, '菜单管理', 1, 'sevencat.ruoyi.sys.controller.SysMenuController.Add()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":null,"parentId":92793505776275456,"menuName":"任务结果","orderNum":1,"path":"rdc/taskitemexeclog","component":"rdc/taskitemexeclog/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"0","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:38:20', 2),
	(92796083826200576, '菜单管理', 2, 'sevencat.ruoyi.sys.controller.SysMenuController.Edit()', 'PUT', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":92796042914959360,"parentId":92793505776275456,"menuName":"任务结果","orderNum":50,"path":"rdc/taskitemexeclog","component":"rdc/taskitemexeclog/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"0","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:38:30', 4),
	(92796121277140992, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:38:39', 34),
	(92796121402970112, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:38:39', 34),
	(92796121407164416, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:38:39', 1),
	(92796333471174656, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_user_gender', '::1', '内网IP', '{"dictType":"sys_user_gender"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000001,"dictSort":1,"dictLabel":"男","dictValue":"0","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"Y","remark":"性别男","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000002,"dictSort":2,"dictLabel":"女","dictValue":"1","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别女","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000003,"dictSort":3,"dictLabel":"未知","dictValue":"2","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别未知","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:39:29', 27),
	(92796333483757568, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_device_type', '::1', '内网IP', '{"dictType":"sys_device_type"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000035,"dictSort":0,"dictLabel":"PC","dictValue":"pc","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"PC","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000036,"dictSort":0,"dictLabel":"安卓","dictValue":"android","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"安卓","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000037,"dictSort":0,"dictLabel":"iOS","dictValue":"ios","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"iOS","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000038,"dictSort":0,"dictLabel":"小程序","dictValue":"xcx","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"小程序","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:39:29', 27),
	(92796407949430784, '个人信息', 2, 'sevencat.ruoyi.sys.controller.SysProfileController.UpdateProfile()', 'PUT', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/user/profile', '::1', '内网IP', '{"profile":{"nickName":"管理员","email":"bastet.wang@aliyun.com","phoneNumber":"15888888888","gender":"0","avatar":null}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:39:47', 11),
	(92796422277173248, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_user_gender', '::1', '内网IP', '{"dictType":"sys_user_gender"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000001,"dictSort":1,"dictLabel":"男","dictValue":"0","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"Y","remark":"性别男","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000002,"dictSort":2,"dictLabel":"女","dictValue":"1","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别女","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000003,"dictSort":3,"dictLabel":"未知","dictValue":"2","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别未知","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:39:51', 29),
	(92796422285561856, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_device_type', '::1', '内网IP', '{"dictType":"sys_device_type"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000035,"dictSort":0,"dictLabel":"PC","dictValue":"pc","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"PC","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000036,"dictSort":0,"dictLabel":"安卓","dictValue":"android","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"安卓","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000037,"dictSort":0,"dictLabel":"iOS","dictValue":"ios","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"iOS","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000038,"dictSort":0,"dictLabel":"小程序","dictValue":"xcx","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"小程序","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:39:51', 29),
	(92796971189932032, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_user_gender', '::1', '内网IP', '{"dictType":"sys_user_gender"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000001,"dictSort":1,"dictLabel":"男","dictValue":"0","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"Y","remark":"性别男","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000002,"dictSort":2,"dictLabel":"女","dictValue":"1","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别女","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000003,"dictSort":3,"dictLabel":"未知","dictValue":"2","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别未知","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:42:02', 25),
	(92796971319955456, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_device_type', '::1', '内网IP', '{"dictType":"sys_device_type"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000035,"dictSort":0,"dictLabel":"PC","dictValue":"pc","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"PC","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000036,"dictSort":0,"dictLabel":"安卓","dictValue":"android","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"安卓","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000037,"dictSort":0,"dictLabel":"iOS","dictValue":"ios","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"iOS","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000038,"dictSort":0,"dictLabel":"小程序","dictValue":"xcx","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"小程序","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:42:02', 25),
	(92797053360541696, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_user_gender', '::1', '内网IP', '{"dictType":"sys_user_gender"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000001,"dictSort":1,"dictLabel":"男","dictValue":"0","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"Y","remark":"性别男","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000002,"dictSort":2,"dictLabel":"女","dictValue":"1","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别女","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000003,"dictSort":3,"dictLabel":"未知","dictValue":"2","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别未知","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:42:21', 1),
	(92797053368930304, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_device_type', '::1', '内网IP', '{"dictType":"sys_device_type"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000035,"dictSort":0,"dictLabel":"PC","dictValue":"pc","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"PC","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000036,"dictSort":0,"dictLabel":"安卓","dictValue":"android","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"安卓","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000037,"dictSort":0,"dictLabel":"iOS","dictValue":"ios","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"iOS","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000038,"dictSort":0,"dictLabel":"小程序","dictValue":"xcx","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"小程序","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:42:21', 1),
	(92797411092729856, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_user_gender', '::1', '内网IP', '{"dictType":"sys_user_gender"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000001,"dictSort":1,"dictLabel":"男","dictValue":"0","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"Y","remark":"性别男","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000002,"dictSort":2,"dictLabel":"女","dictValue":"1","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别女","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000003,"dictSort":3,"dictLabel":"未知","dictValue":"2","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别未知","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:43:46', 1),
	(92797411101118464, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_device_type', '::1', '内网IP', '{"dictType":"sys_device_type"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000035,"dictSort":0,"dictLabel":"PC","dictValue":"pc","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"PC","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000036,"dictSort":0,"dictLabel":"安卓","dictValue":"android","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"安卓","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000037,"dictSort":0,"dictLabel":"iOS","dictValue":"ios","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"iOS","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000038,"dictSort":0,"dictLabel":"小程序","dictValue":"xcx","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"小程序","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:43:46', 1),
	(92797633751552000, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_device_type', '::1', '内网IP', '{"dictType":"sys_device_type"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000035,"dictSort":0,"dictLabel":"PC","dictValue":"pc","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"PC","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000036,"dictSort":0,"dictLabel":"安卓","dictValue":"android","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"安卓","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000037,"dictSort":0,"dictLabel":"iOS","dictValue":"ios","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"iOS","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000038,"dictSort":0,"dictLabel":"小程序","dictValue":"xcx","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"小程序","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:44:39', 1),
	(92797633755746304, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_user_gender', '::1', '内网IP', '{"dictType":"sys_user_gender"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000001,"dictSort":1,"dictLabel":"男","dictValue":"0","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"Y","remark":"性别男","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000002,"dictSort":2,"dictLabel":"女","dictValue":"1","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别女","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000003,"dictSort":3,"dictLabel":"未知","dictValue":"2","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别未知","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:44:39', 1),
	(92798990575341568, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:50:03', 1),
	(92798991519059968, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:50:03', 29),
	(92798991523254272, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:50:03', 30),
	(92799148985815040, '菜单管理', 1, 'sevencat.ruoyi.sys.controller.SysMenuController.Add()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":null,"parentId":92793505776275456,"menuName":"地图管理","orderNum":1,"path":"rdc/mapmgr","component":"rdc/mapmgr/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"0","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 09:50:41', 4),
	(92799159131836416, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:50:43', 1),
	(92799159240888320, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:50:43', 1),
	(92799159245082624, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:50:43', 1),
	(92800591440515072, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_device_type', '::1', '内网IP', '{"dictType":"sys_device_type"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000035,"dictSort":0,"dictLabel":"PC","dictValue":"pc","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"PC","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000036,"dictSort":0,"dictLabel":"安卓","dictValue":"android","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"安卓","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000037,"dictSort":0,"dictLabel":"iOS","dictValue":"ios","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"iOS","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000038,"dictSort":0,"dictLabel":"小程序","dictValue":"xcx","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"小程序","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 09:56:25', 1),
	(92845730691158016, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 12:55:47', 30),
	(92845731324497920, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 12:55:47', 1),
	(92845731349663744, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 12:55:47', 7),
	(92845838459604992, '菜单管理', 2, 'sevencat.ruoyi.sys.controller.SysMenuController.Edit()', 'PUT', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":92795533978112000,"parentId":92793505776275456,"menuName":"实时监控","orderNum":60,"path":"rdc/dogmonitor","component":"rdc/dogmonitor/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"0","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 12:56:12', 59),
	(92846003253809152, '菜单管理', 1, 'sevencat.ruoyi.sys.controller.SysMenuController.Add()', 'POST', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":null,"parentId":92793505776275456,"menuName":"路线管理","orderNum":1,"path":"rdc/pathmgr","component":"rdc/pathmgr/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"0","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 12:56:52', 8),
	(92846016298094592, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 12:56:55', 1),
	(92846016306483200, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 12:56:55', 1),
	(92846016314871808, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 12:56:55', 5),
	(92846078608674816, '菜单管理', 2, 'sevencat.ruoyi.sys.controller.SysMenuController.Edit()', 'PUT', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":92799148981620736,"parentId":92793505776275456,"menuName":"地图管理","orderNum":10,"path":"rdc/mapmgr","component":"rdc/mapmgr/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"0","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 12:57:10', 4),
	(92846129330393088, '菜单管理', 2, 'sevencat.ruoyi.sys.controller.SysMenuController.Edit()', 'PUT', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":92846003241226240,"parentId":92793505776275456,"menuName":"路线管理","orderNum":15,"path":"rdc/pathmgr","component":"rdc/pathmgr/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"0","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 12:57:22', 3),
	(92851244258955264, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 13:17:41', 127),
	(92851244342841344, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 13:17:41', 1),
	(92851244347035648, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 13:17:41', 7),
	(92851281378545664, '菜单管理', 2, 'sevencat.ruoyi.sys.controller.SysMenuController.Edit()', 'PUT', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/menu', '::1', '内网IP', '{"menu":{"menuId":92795533978112000,"parentId":92793505776275456,"menuName":"实时监控","orderNum":60,"path":"rdc/dogmonitor","component":"rdc/dogmonitor/index","queryParam":null,"isFrame":"N","isCache":"Y","menuType":"C","visible":"1","status":"0","perms":null,"icon":"","activeMenu":"","ext":null,"remark":""}}', '{"code":200,"msg":"success"}', 0, NULL, '2026-09-14 13:17:50', 58),
	(92851293130985472, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 13:17:53', 1),
	(92851293168734208, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 13:17:53', 1),
	(92851293172928512, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_show_hide', '::1', '内网IP', '{"dictType":"sys_show_hide"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000004,"dictSort":1,"dictLabel":"显示","dictValue":"0","dictType":"sys_show_hide","cssClass":"","listClass":"primary","isDefault":"Y","remark":"显示菜单","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000005,"dictSort":2,"dictLabel":"隐藏","dictValue":"1","dictType":"sys_show_hide","cssClass":"","listClass":"danger","isDefault":"N","remark":"隐藏菜单","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 13:17:53', 3),
	(92870121453916160, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_user_gender', '::1', '内网IP', '{"dictType":"sys_user_gender"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000001,"dictSort":1,"dictLabel":"男","dictValue":"0","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"Y","remark":"性别男","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000002,"dictSort":2,"dictLabel":"女","dictValue":"1","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别女","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000003,"dictSort":3,"dictLabel":"未知","dictValue":"2","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别未知","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 14:32:42', 62),
	(92870121458110464, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 14:32:42', 62),
	(92893563536412672, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_device_type', '116.147.145.16', 'XX XX', '{"dictType":"sys_device_type"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000035,"dictSort":0,"dictLabel":"PC","dictValue":"pc","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"PC","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000036,"dictSort":0,"dictLabel":"安卓","dictValue":"android","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"安卓","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000037,"dictSort":0,"dictLabel":"iOS","dictValue":"ios","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"iOS","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000038,"dictSort":0,"dictLabel":"小程序","dictValue":"xcx","dictType":"sys_device_type","cssClass":"","listClass":"default","isDefault":"N","remark":"小程序","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 16:05:51', 35),
	(92893731665088512, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '116.147.145.16', 'XX XX', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-14 16:06:31', 1),
	(96095108415492096, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_user_gender', '::1', '内网IP', '{"dictType":"sys_user_gender"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000001,"dictSort":1,"dictLabel":"男","dictValue":"0","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"Y","remark":"性别男","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000002,"dictSort":2,"dictLabel":"女","dictValue":"1","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别女","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000003,"dictSort":3,"dictLabel":"未知","dictValue":"2","dictType":"sys_user_gender","cssClass":"","listClass":"","isDefault":"N","remark":"性别未知","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-23 12:07:39', 40),
	(96095108499378176, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_normal_disable', '::1', '内网IP', '{"dictType":"sys_normal_disable"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000006,"dictSort":1,"dictLabel":"正常","dictValue":"0","dictType":"sys_normal_disable","cssClass":"","listClass":"primary","isDefault":"Y","remark":"正常状态","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000007,"dictSort":2,"dictLabel":"停用","dictValue":"1","dictType":"sys_normal_disable","cssClass":"","listClass":"danger","isDefault":"N","remark":"停用状态","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-23 12:07:39', 30),
	(96095123888279552, '字典数据', 1, 'sevencat.ruoyi.sys.controller.SysDictDataController.DictType()', 'GET', 1, 'admin', 1761100000000000001, 1761000000000000103, '研发部门', 'e5cd7e4891bf95d1d19206ce24a7b32e', 'pc', 'Chrome', 'Windows', '/api/system/dict/data/type/sys_yes_no', '::1', '内网IP', '{"dictType":"sys_yes_no"}', '{"code":200,"msg":"success","data":[{"dictCode":1761600000000000012,"dictSort":1,"dictLabel":"是","dictValue":"Y","dictType":"sys_yes_no","cssClass":"","listClass":"primary","isDefault":"Y","remark":"系统默认是","createTime":"2026-09-10 08:18:58"},{"dictCode":1761600000000000013,"dictSort":2,"dictLabel":"否","dictValue":"N","dictType":"sys_yes_no","cssClass":"","listClass":"danger","isDefault":"N","remark":"系统默认否","createTime":"2026-09-10 08:18:58"}]}', 0, NULL, '2026-09-23 12:07:42', 1);

-- 导出  表 rdc.sys_oss 结构
CREATE TABLE IF NOT EXISTS `sys_oss` (
  `oss_id` bigint(20) NOT NULL COMMENT '对象存储主键',
  `file_name` varchar(255) NOT NULL DEFAULT '' COMMENT '文件名',
  `original_name` varchar(255) NOT NULL DEFAULT '' COMMENT '原名',
  `file_suffix` varchar(10) NOT NULL DEFAULT '' COMMENT '文件后缀名',
  `url` varchar(500) NOT NULL COMMENT 'URL地址',
  `ext1` text DEFAULT NULL COMMENT '扩展字段',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `create_by` bigint(20) DEFAULT NULL COMMENT '上传人',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新人',
  `service` varchar(20) NOT NULL DEFAULT 'minio' COMMENT '服务商',
  PRIMARY KEY (`oss_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OSS对象存储表';

-- 正在导出表  rdc.sys_oss 的数据：~2 rows (大约)
INSERT INTO `sys_oss` (`oss_id`, `file_name`, `original_name`, `file_suffix`, `url`, `ext1`, `create_dept`, `create_time`, `create_by`, `update_time`, `update_by`, `service`) VALUES
	(92766936198418432, '2026-09-14/37cd9e783c3e40279af4774426e62e27.jpg', 'Copilot_20260823_104314.jpg', '.jpg', 'http://127.0.0.1:7050/api/s3/ruoyi/2026-09-14/37cd9e783c3e40279af4774426e62e27.jpg', '{"bizType":null,"fileSize":69527,"contentType":"image/jpeg","source":null,"uploadIp":null,"remark":null,"tags":null,"refId":null,"refType":null,"isTemp":null,"md5":null}', 1761000000000000103, '2026-09-14 07:42:41', 1761100000000000001, NULL, NULL, 'minio'),
	(92776847628177408, '2026-09-14/ebeb90dcae8c4d1cb691e594e8f3080c.jpg', '图片合成样例.jpg', '.jpg', 'http://127.0.0.1:7050/api/s3/ruoyi/2026-09-14/ebeb90dcae8c4d1cb691e594e8f3080c.jpg', '{"bizType":null,"fileSize":403056,"contentType":"image/jpeg","source":null,"uploadIp":null,"remark":null,"tags":null,"refId":null,"refType":null,"isTemp":null,"md5":null}', 1761000000000000103, '2026-09-14 08:22:04', 1761100000000000001, NULL, NULL, 'minio');

-- 导出  表 rdc.sys_oss_config 结构
CREATE TABLE IF NOT EXISTS `sys_oss_config` (
  `oss_config_id` bigint(20) NOT NULL COMMENT '主键',
  `config_key` varchar(20) NOT NULL DEFAULT '' COMMENT '配置key',
  `access_key` varchar(255) DEFAULT '' COMMENT 'accessKey',
  `secret_key` varchar(255) DEFAULT '' COMMENT '秘钥',
  `bucket_name` varchar(255) DEFAULT '' COMMENT '桶名称',
  `prefix` varchar(255) DEFAULT '' COMMENT '前缀',
  `endpoint` varchar(255) DEFAULT '' COMMENT '访问站点',
  `domain_url` varchar(255) DEFAULT '' COMMENT '自定义域名',
  `is_https` char(1) DEFAULT 'N' COMMENT '是否https（Y=是,N=否）',
  `region` varchar(255) DEFAULT '' COMMENT '域',
  `access_policy` char(1) NOT NULL DEFAULT '1' COMMENT '桶权限类型(0=private 1=public 2=custom)',
  `status` char(1) DEFAULT 'N' COMMENT '是否默认（Y=是,N=否）',
  `ext1` varchar(255) DEFAULT '' COMMENT '扩展字段',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `remark` varchar(500) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY (`oss_config_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='对象存储配置表';

-- 正在导出表  rdc.sys_oss_config 的数据：~1 rows (大约)
INSERT INTO `sys_oss_config` (`oss_config_id`, `config_key`, `access_key`, `secret_key`, `bucket_name`, `prefix`, `endpoint`, `domain_url`, `is_https`, `region`, `access_policy`, `status`, `ext1`, `create_dept`, `create_by`, `create_time`, `update_by`, `update_time`, `remark`) VALUES
	(1761900000000000001, 'minio', 'ruoyi', 'ruoyi123', 'ruoyi', '', '127.0.0.1:7050/api/minio', '', 'N', '', '1', 'Y', '', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', 1761100000000000001, '2026-09-12 10:53:50', '');

-- 导出  表 rdc.sys_post 结构
CREATE TABLE IF NOT EXISTS `sys_post` (
  `post_id` bigint(20) NOT NULL COMMENT '岗位ID',
  `dept_id` bigint(20) NOT NULL COMMENT '部门id',
  `post_code` varchar(64) NOT NULL COMMENT '岗位编码',
  `post_category` varchar(100) DEFAULT NULL COMMENT '岗位类别编码',
  `post_name` varchar(50) NOT NULL COMMENT '岗位名称',
  `post_sort` int(4) NOT NULL COMMENT '显示顺序',
  `status` char(1) NOT NULL COMMENT '状态（0正常 1停用）',
  `del_flag` char(1) DEFAULT '0' COMMENT '删除标志（0代表存在 1代表删除）',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `remark` varchar(500) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY (`post_id`),
  KEY `idx_sys_post_dept_id` (`dept_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='岗位信息表';

-- 正在导出表  rdc.sys_post 的数据：~4 rows (大约)
INSERT INTO `sys_post` (`post_id`, `dept_id`, `post_code`, `post_category`, `post_name`, `post_sort`, `status`, `del_flag`, `create_dept`, `create_by`, `create_time`, `update_by`, `update_time`, `remark`) VALUES
	(1761200000000000001, 1761000000000000103, 'ceo', NULL, '董事长', 1, '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761200000000000002, 1761000000000000100, 'se', NULL, '项目经理', 2, '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761200000000000003, 1761000000000000100, 'hr', NULL, '人力资源', 3, '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761200000000000004, 1761000000000000100, 'user', NULL, '普通员工', 4, '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '');

-- 导出  表 rdc.sys_role 结构
CREATE TABLE IF NOT EXISTS `sys_role` (
  `role_id` bigint(20) NOT NULL COMMENT '角色ID',
  `role_name` varchar(30) NOT NULL COMMENT '角色名称',
  `role_key` varchar(100) NOT NULL COMMENT '角色权限字符串',
  `role_sort` int(4) NOT NULL COMMENT '显示顺序',
  `data_scope` char(1) DEFAULT '1' COMMENT '数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限 5：仅本人数据权限 6：部门及以下或本人数据权限）',
  `menu_check_strictly` tinyint(1) DEFAULT 1 COMMENT '菜单树选择项是否关联显示',
  `dept_check_strictly` tinyint(1) DEFAULT 1 COMMENT '部门树选择项是否关联显示',
  `status` char(1) NOT NULL COMMENT '角色状态（0正常 1停用）',
  `del_flag` char(1) DEFAULT '0' COMMENT '删除标志（0代表存在 1代表删除）',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `remark` varchar(500) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY (`role_id`),
  KEY `idx_sys_role_create_dept` (`create_dept`),
  KEY `idx_sys_role_create_by` (`create_by`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='角色信息表';

-- 正在导出表  rdc.sys_role 的数据：~3 rows (大约)
INSERT INTO `sys_role` (`role_id`, `role_name`, `role_key`, `role_sort`, `data_scope`, `menu_check_strictly`, `dept_check_strictly`, `status`, `del_flag`, `create_dept`, `create_by`, `create_time`, `update_by`, `update_time`, `remark`) VALUES
	(1761300000000000001, '超级管理员', 'superadmin', 1, '1', 1, 1, '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '超级管理员'),
	(1761300000000000003, '本部门及以下', 'test1', 3, '4', 1, 1, '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, ''),
	(1761300000000000004, '仅本人', 'test2', 4, '5', 1, 1, '0', '0', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', NULL, NULL, '');

-- 导出  表 rdc.sys_role_dept 结构
CREATE TABLE IF NOT EXISTS `sys_role_dept` (
  `role_id` bigint(20) NOT NULL COMMENT '角色ID',
  `dept_id` bigint(20) NOT NULL COMMENT '部门ID',
  PRIMARY KEY (`role_id`,`dept_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='角色和部门关联表';

-- 正在导出表  rdc.sys_role_dept 的数据：~0 rows (大约)

-- 导出  表 rdc.sys_role_menu 结构
CREATE TABLE IF NOT EXISTS `sys_role_menu` (
  `role_id` bigint(20) NOT NULL COMMENT '角色ID',
  `menu_id` bigint(20) NOT NULL COMMENT '菜单ID',
  PRIMARY KEY (`role_id`,`menu_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='角色和菜单关联表';

-- 正在导出表  rdc.sys_role_menu 的数据：~117 rows (大约)
INSERT INTO `sys_role_menu` (`role_id`, `menu_id`) VALUES
	(1761300000000000003, 1761400000000000001),
	(1761300000000000003, 1761400000000000005),
	(1761300000000000003, 1761400000000000100),
	(1761300000000000003, 1761400000000000101),
	(1761300000000000003, 1761400000000000102),
	(1761300000000000003, 1761400000000000103),
	(1761300000000000003, 1761400000000000104),
	(1761300000000000003, 1761400000000000105),
	(1761300000000000003, 1761400000000000106),
	(1761300000000000003, 1761400000000000107),
	(1761300000000000003, 1761400000000000108),
	(1761300000000000003, 1761400000000000118),
	(1761300000000000003, 1761400000000000123),
	(1761300000000000003, 1761400000000000130),
	(1761300000000000003, 1761400000000000131),
	(1761300000000000003, 1761400000000000133),
	(1761300000000000003, 1761400000000000500),
	(1761300000000000003, 1761400000000000501),
	(1761300000000000003, 1761400000000001001),
	(1761300000000000003, 1761400000000001002),
	(1761300000000000003, 1761400000000001003),
	(1761300000000000003, 1761400000000001004),
	(1761300000000000003, 1761400000000001005),
	(1761300000000000003, 1761400000000001006),
	(1761300000000000003, 1761400000000001007),
	(1761300000000000003, 1761400000000001008),
	(1761300000000000003, 1761400000000001009),
	(1761300000000000003, 1761400000000001010),
	(1761300000000000003, 1761400000000001011),
	(1761300000000000003, 1761400000000001012),
	(1761300000000000003, 1761400000000001013),
	(1761300000000000003, 1761400000000001014),
	(1761300000000000003, 1761400000000001015),
	(1761300000000000003, 1761400000000001016),
	(1761300000000000003, 1761400000000001017),
	(1761300000000000003, 1761400000000001018),
	(1761300000000000003, 1761400000000001019),
	(1761300000000000003, 1761400000000001020),
	(1761300000000000003, 1761400000000001021),
	(1761300000000000003, 1761400000000001022),
	(1761300000000000003, 1761400000000001023),
	(1761300000000000003, 1761400000000001024),
	(1761300000000000003, 1761400000000001025),
	(1761300000000000003, 1761400000000001026),
	(1761300000000000003, 1761400000000001027),
	(1761300000000000003, 1761400000000001028),
	(1761300000000000003, 1761400000000001029),
	(1761300000000000003, 1761400000000001030),
	(1761300000000000003, 1761400000000001031),
	(1761300000000000003, 1761400000000001032),
	(1761300000000000003, 1761400000000001033),
	(1761300000000000003, 1761400000000001034),
	(1761300000000000003, 1761400000000001035),
	(1761300000000000003, 1761400000000001036),
	(1761300000000000003, 1761400000000001037),
	(1761300000000000003, 1761400000000001038),
	(1761300000000000003, 1761400000000001039),
	(1761300000000000003, 1761400000000001040),
	(1761300000000000003, 1761400000000001041),
	(1761300000000000003, 1761400000000001042),
	(1761300000000000003, 1761400000000001043),
	(1761300000000000003, 1761400000000001044),
	(1761300000000000003, 1761400000000001045),
	(1761300000000000003, 1761400000000001050),
	(1761300000000000003, 1761400000000001061),
	(1761300000000000003, 1761400000000001062),
	(1761300000000000003, 1761400000000001063),
	(1761300000000000003, 1761400000000001064),
	(1761300000000000003, 1761400000000001065),
	(1761300000000000003, 1761400000000001500),
	(1761300000000000003, 1761400000000001501),
	(1761300000000000003, 1761400000000001502),
	(1761300000000000003, 1761400000000001503),
	(1761300000000000003, 1761400000000001504),
	(1761300000000000003, 1761400000000001505),
	(1761300000000000003, 1761400000000001506),
	(1761300000000000003, 1761400000000001507),
	(1761300000000000003, 1761400000000001508),
	(1761300000000000003, 1761400000000001509),
	(1761300000000000003, 1761400000000001510),
	(1761300000000000003, 1761400000000001511),
	(1761300000000000003, 1761400000000001600),
	(1761300000000000003, 1761400000000001601),
	(1761300000000000003, 1761400000000001602),
	(1761300000000000003, 1761400000000001603),
	(1761300000000000003, 1761400000000001620),
	(1761300000000000003, 1761400000000001621),
	(1761300000000000003, 1761400000000001622),
	(1761300000000000003, 1761400000000001623),
	(1761300000000000003, 1761400000000011616),
	(1761300000000000003, 1761400000000011618),
	(1761300000000000003, 1761400000000011619),
	(1761300000000000003, 1761400000000011622),
	(1761300000000000003, 1761400000000011623),
	(1761300000000000003, 1761400000000011629),
	(1761300000000000003, 1761400000000011632),
	(1761300000000000003, 1761400000000011633),
	(1761300000000000003, 1761400000000011638),
	(1761300000000000003, 1761400000000011639),
	(1761300000000000003, 1761400000000011640),
	(1761300000000000003, 1761400000000011641),
	(1761300000000000003, 1761400000000011642),
	(1761300000000000003, 1761400000000011643),
	(1761300000000000003, 1761400000000011701),
	(1761300000000000004, 1761400000000000005),
	(1761300000000000004, 1761400000000001500),
	(1761300000000000004, 1761400000000001501),
	(1761300000000000004, 1761400000000001502),
	(1761300000000000004, 1761400000000001503),
	(1761300000000000004, 1761400000000001504),
	(1761300000000000004, 1761400000000001505),
	(1761300000000000004, 1761400000000001506),
	(1761300000000000004, 1761400000000001507),
	(1761300000000000004, 1761400000000001508),
	(1761300000000000004, 1761400000000001509),
	(1761300000000000004, 1761400000000001510),
	(1761300000000000004, 1761400000000001511);

-- 导出  表 rdc.sys_social 结构
CREATE TABLE IF NOT EXISTS `sys_social` (
  `id` bigint(20) NOT NULL COMMENT '主键',
  `user_id` bigint(20) NOT NULL COMMENT '用户ID',
  `auth_id` varchar(255) NOT NULL COMMENT '平台+平台唯一id',
  `source` varchar(255) NOT NULL COMMENT '用户来源',
  `open_id` varchar(255) DEFAULT NULL COMMENT '平台编号唯一id',
  `user_name` varchar(30) NOT NULL COMMENT '登录账号',
  `nick_name` varchar(30) DEFAULT '' COMMENT '用户昵称',
  `email` varchar(255) DEFAULT '' COMMENT '用户邮箱',
  `avatar` varchar(500) DEFAULT '' COMMENT '头像地址',
  `access_token` varchar(2000) NOT NULL COMMENT '用户的授权令牌',
  `expire_in` int(11) DEFAULT NULL COMMENT '用户的授权令牌的有效期，部分平台可能没有',
  `refresh_token` varchar(2000) DEFAULT NULL COMMENT '刷新令牌，部分平台可能没有',
  `access_code` varchar(255) DEFAULT NULL COMMENT '平台的授权信息，部分平台可能没有',
  `union_id` varchar(255) DEFAULT NULL COMMENT '用户的 unionid',
  `scope` varchar(255) DEFAULT NULL COMMENT '授予的权限，部分平台可能没有',
  `token_type` varchar(255) DEFAULT NULL COMMENT '个别平台的授权信息，部分平台可能没有',
  `id_token` varchar(2000) DEFAULT NULL COMMENT 'id token，部分平台可能没有',
  `mac_algorithm` varchar(255) DEFAULT NULL COMMENT '小米平台用户的附带属性，部分平台可能没有',
  `mac_key` varchar(255) DEFAULT NULL COMMENT '小米平台用户的附带属性，部分平台可能没有',
  `code` varchar(255) DEFAULT NULL COMMENT '用户的授权code，部分平台可能没有',
  `oauth_token` varchar(255) DEFAULT NULL COMMENT 'Twitter平台用户的附带属性，部分平台可能没有',
  `oauth_token_secret` varchar(255) DEFAULT NULL COMMENT 'Twitter平台用户的附带属性，部分平台可能没有',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `del_flag` char(1) DEFAULT '0' COMMENT '删除标志（0代表存在 1代表删除）',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='社会化关系表';

-- 正在导出表  rdc.sys_social 的数据：~0 rows (大约)

-- 导出  表 rdc.sys_user 结构
CREATE TABLE IF NOT EXISTS `sys_user` (
  `user_id` bigint(20) NOT NULL COMMENT '用户ID',
  `dept_id` bigint(20) DEFAULT NULL COMMENT '部门ID',
  `user_name` varchar(30) NOT NULL COMMENT '用户账号',
  `nick_name` varchar(30) NOT NULL COMMENT '用户昵称',
  `user_type` varchar(10) DEFAULT 'sys_user' COMMENT '用户类型（sys_user系统用户）',
  `email` varchar(50) DEFAULT '' COMMENT '用户邮箱',
  `phone_number` varchar(11) DEFAULT '' COMMENT '手机号码',
  `gender` char(1) DEFAULT '0' COMMENT '用户性别（0男 1女 2未知）',
  `avatar` bigint(20) DEFAULT NULL COMMENT '头像地址',
  `password` varchar(100) DEFAULT '' COMMENT '密码',
  `status` char(1) DEFAULT '0' COMMENT '账号状态（0正常 1停用）',
  `del_flag` char(1) DEFAULT '0' COMMENT '删除标志（0代表存在 1代表删除）',
  `login_ip` varchar(128) DEFAULT '' COMMENT '最后登录IP',
  `login_date` datetime DEFAULT NULL COMMENT '最后登录时间',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建者',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新者',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `remark` varchar(500) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY (`user_id`),
  KEY `idx_sys_user_dept_id` (`dept_id`),
  KEY `idx_sys_user_create_by` (`create_by`),
  KEY `idx_sys_user_user_name` (`user_name`),
  KEY `idx_sys_user_phone` (`phone_number`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='用户信息表';

-- 正在导出表  rdc.sys_user 的数据：~3 rows (大约)
INSERT INTO `sys_user` (`user_id`, `dept_id`, `user_name`, `nick_name`, `user_type`, `email`, `phone_number`, `gender`, `avatar`, `password`, `status`, `del_flag`, `login_ip`, `login_date`, `create_dept`, `create_by`, `create_time`, `update_by`, `update_time`, `remark`) VALUES
	(1761100000000000001, 1761000000000000103, 'admin', '管理员', 'sys_user', 'bastet.wang@aliyun.com', '15888888888', '0', 92227725246468096, '$2a$10$7JB720yubVSZvUI0rEqK/.VqGOZTH.ulu33dHOiBE8ByOhJIrdAu2', '0', '0', '::1', '2026-10-03 11:31:12', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', -1, '2026-09-12 10:07:01', '管理员'),
	(1761100000000000003, 1761000000000000108, 'test', '本部门及以下 密码666666', 'sys_user', '', '', '0', NULL, '$2a$10$b8yUzN0C71sbz.PhNOCgJe.Tu1yWC3RNrTyjSQ8p1W0.aaUXUJ.Ne', '0', '0', '127.0.0.1', '2026-09-10 08:18:58', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', 1761100000000000003, '2026-09-10 08:18:58', NULL),
	(1761100000000000004, 1761000000000000102, 'test1', '仅本人 密码666666', 'sys_user', '', '', '0', NULL, '$2a$10$b8yUzN0C71sbz.PhNOCgJe.Tu1yWC3RNrTyjSQ8p1W0.aaUXUJ.Ne', '0', '0', '127.0.0.1', '2026-09-10 08:18:58', 1761000000000000103, 1761100000000000001, '2026-09-10 08:18:58', 1761100000000000004, '2026-09-10 08:18:58', NULL);

-- 导出  表 rdc.sys_user_post 结构
CREATE TABLE IF NOT EXISTS `sys_user_post` (
  `user_id` bigint(20) NOT NULL COMMENT '用户ID',
  `post_id` bigint(20) NOT NULL COMMENT '岗位ID',
  PRIMARY KEY (`user_id`,`post_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='用户与岗位关联表';

-- 正在导出表  rdc.sys_user_post 的数据：~0 rows (大约)
INSERT INTO `sys_user_post` (`user_id`, `post_id`) VALUES
	(1761100000000000001, 1761200000000000001);

-- 导出  表 rdc.sys_user_role 结构
CREATE TABLE IF NOT EXISTS `sys_user_role` (
  `user_id` bigint(20) NOT NULL COMMENT '用户ID',
  `role_id` bigint(20) NOT NULL COMMENT '角色ID',
  PRIMARY KEY (`user_id`,`role_id`),
  KEY `idx_sys_user_role_rid` (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='用户和角色关联表';

-- 正在导出表  rdc.sys_user_role 的数据：~3 rows (大约)
INSERT INTO `sys_user_role` (`user_id`, `role_id`) VALUES
	(1761100000000000001, 1761300000000000001),
	(1761100000000000003, 1761300000000000003),
	(1761100000000000004, 1761300000000000004);

-- 导出  表 rdc.test_demo 结构
CREATE TABLE IF NOT EXISTS `test_demo` (
  `id` bigint(20) NOT NULL COMMENT '主键',
  `dept_id` bigint(20) DEFAULT NULL COMMENT '部门id',
  `user_id` bigint(20) DEFAULT NULL COMMENT '用户id',
  `order_num` int(11) DEFAULT 0 COMMENT '排序号',
  `test_key` varchar(255) DEFAULT NULL COMMENT 'key键',
  `value` varchar(255) DEFAULT NULL COMMENT '值',
  `version` int(11) DEFAULT 0 COMMENT '版本',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建人',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新人',
  `del_flag` int(11) DEFAULT 0 COMMENT '删除标志',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='测试单表';

-- 正在导出表  rdc.test_demo 的数据：~13 rows (大约)
INSERT INTO `test_demo` (`id`, `dept_id`, `user_id`, `order_num`, `test_key`, `value`, `version`, `create_dept`, `create_time`, `create_by`, `update_time`, `update_by`, `del_flag`) VALUES
	(1762100000000000001, 1761000000000000102, 1761100000000000004, 1, '测试数据权限', '测试', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762100000000000002, 1761000000000000102, 1761100000000000003, 2, '子节点1', '111', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762100000000000003, 1761000000000000102, 1761100000000000003, 3, '子节点2', '222', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762100000000000004, 1761000000000000108, 1761100000000000004, 4, '测试数据', 'demo', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762100000000000005, 1761000000000000108, 1761100000000000003, 13, '子节点11', '1111', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762100000000000006, 1761000000000000108, 1761100000000000003, 12, '子节点22', '2222', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762100000000000007, 1761000000000000108, 1761100000000000003, 11, '子节点33', '3333', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762100000000000008, 1761000000000000108, 1761100000000000003, 10, '子节点44', '4444', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762100000000000009, 1761000000000000108, 1761100000000000003, 9, '子节点55', '5555', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762100000000000010, 1761000000000000108, 1761100000000000003, 8, '子节点66', '6666', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762100000000000011, 1761000000000000108, 1761100000000000003, 7, '子节点77', '7777', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762100000000000012, 1761000000000000108, 1761100000000000003, 6, '子节点88', '8888', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762100000000000013, 1761000000000000108, 1761100000000000003, 5, '子节点99', '9999', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0);

-- 导出  表 rdc.test_tree 结构
CREATE TABLE IF NOT EXISTS `test_tree` (
  `id` bigint(20) NOT NULL COMMENT '主键',
  `parent_id` bigint(20) DEFAULT 0 COMMENT '父id',
  `dept_id` bigint(20) DEFAULT NULL COMMENT '部门id',
  `user_id` bigint(20) DEFAULT NULL COMMENT '用户id',
  `tree_name` varchar(255) DEFAULT NULL COMMENT '值',
  `version` int(11) DEFAULT 0 COMMENT '版本',
  `create_dept` bigint(20) DEFAULT NULL COMMENT '创建部门',
  `create_time` datetime DEFAULT NULL COMMENT '创建时间',
  `create_by` bigint(20) DEFAULT NULL COMMENT '创建人',
  `update_time` datetime DEFAULT NULL COMMENT '更新时间',
  `update_by` bigint(20) DEFAULT NULL COMMENT '更新人',
  `del_flag` int(11) DEFAULT 0 COMMENT '删除标志',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='测试树表';

-- 正在导出表  rdc.test_tree 的数据：~13 rows (大约)
INSERT INTO `test_tree` (`id`, `parent_id`, `dept_id`, `user_id`, `tree_name`, `version`, `create_dept`, `create_time`, `create_by`, `update_time`, `update_by`, `del_flag`) VALUES
	(1762200000000000001, 0, 1761000000000000102, 1761100000000000004, '测试数据权限', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762200000000000002, 1762200000000000001, 1761000000000000102, 1761100000000000003, '子节点1', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762200000000000003, 1762200000000000002, 1761000000000000102, 1761100000000000003, '子节点2', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762200000000000004, 0, 1761000000000000108, 1761100000000000004, '测试树1', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762200000000000005, 1762200000000000004, 1761000000000000108, 1761100000000000003, '子节点11', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762200000000000006, 1762200000000000004, 1761000000000000108, 1761100000000000003, '子节点22', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762200000000000007, 1762200000000000004, 1761000000000000108, 1761100000000000003, '子节点33', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762200000000000008, 1762200000000000005, 1761000000000000108, 1761100000000000003, '子节点44', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762200000000000009, 1762200000000000006, 1761000000000000108, 1761100000000000003, '子节点55', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762200000000000010, 1762200000000000007, 1761000000000000108, 1761100000000000003, '子节点66', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762200000000000011, 1762200000000000007, 1761000000000000108, 1761100000000000003, '子节点77', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762200000000000012, 1762200000000000010, 1761000000000000108, 1761100000000000003, '子节点88', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0),
	(1762200000000000013, 1762200000000000010, 1761000000000000108, 1761100000000000003, '子节点99', 0, 1761000000000000103, '2026-09-10 08:18:58', 1761100000000000001, NULL, NULL, 0);

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
