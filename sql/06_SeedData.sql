-- ============================================================
-- MES 生产执行系统 — 06 基础测试数据
-- 前置条件：05_Views.sql 已执行完毕
-- 说明：模拟一个简单的机械加工场景（通用，与具体设备无关）
-- ============================================================

USE MES;
GO

-- ============================================================
-- 用户账号
-- ============================================================
INSERT INTO T_UserAccount (UserId, UserName, LoginName, PasswordHash, Role)
VALUES
('EMP-001', N'管理员',   'admin',    '21232F297A57A5A743894A0E4A801FC3', 'ADMIN'),    -- admin
('EMP-002', N'张三',     'zhangsan', 'E10ADC3949BA59ABBE56E057F20F883E', 'OPERATOR'), -- 123456
('EMP-003', N'李四',     'lisi',     'E10ADC3949BA59ABBE56E057F20F883E', 'OPERATOR'),
('EMP-004', N'王质检',   'wangqj',   'E10ADC3949BA59ABBE56E057F20F883E', 'INSPECTOR'),
('EMP-005', N'赵维修',   'zhaowx',   'E10ADC3949BA59ABBE56E057F20F883E', 'OPERATOR');
GO

-- ============================================================
-- 工作中心
-- ============================================================
INSERT INTO T_WorkCenter (WorkCenterCode, WorkCenterName, WorkCenterType, Description, Capacity)
VALUES
('WC-MACH-001', N'数控加工中心1', 'MACHINING',  N'CNC车铣复合加工', 20.0),
('WC-MACH-002', N'数控加工中心2', 'MACHINING',  N'CNC铣削加工',     20.0),
('WC-ASSY-001', N'装配工位A',     'ASSEMBLY',   N'手工装配工位',     15.0),
('WC-INSP-001', N'质检工位',      'INSPECTION', N'在线检验工位',     30.0),
('WC-WELD-001', N'焊接工位',      'MACHINING',  N'氩弧焊工位',       10.0);
GO

-- ============================================================
-- 物料主数据
-- ============================================================
INSERT INTO T_Item (ItemCode, ItemName, ItemType, UOM, Spec, SafetyStock)
VALUES
-- 原材料
('RM-STEEL-001', N'45号钢棒料 Φ50',   'RAW',      'KG',  'Φ50×1000mm',     100.0),
('RM-ALUM-001',  N'铝合金板 6061-T6', 'RAW',      'KG',  '1000×2000×10mm', 50.0),
('RM-BOLT-M8',   N'M8内六角螺栓',     'RAW',      'PCS', 'M8×30 8.8级',    500.0),
-- 半成品
('WIP-SHAFT-001', N'传动轴半成品',     'SEMI',     'PCS', 'L=250mm',        10.0),
('WIP-COVER-001', N'端盖半成品',       'SEMI',     'PCS', 'Φ80×15mm',       5.0),
-- 成品
('FG-ASSY-001',   N'传动轴总成',       'FINISHED', 'PCS', 'P/N: MES-001',   0.0);
GO

-- ============================================================
-- BOM
-- ============================================================
INSERT INTO T_BOM (BOMCode, BOMName, ParentItemCode, BOMVersion, EffectiveDate)
VALUES
('BOM-ASSY-001', N'传动轴总成BOM', 'FG-ASSY-001', '1.0', '2026-01-01');

INSERT INTO T_BOMDetail (BOMCode, ChildItemCode, Quantity, UOM, ScrapRate, Seq)
VALUES
('BOM-ASSY-001', 'WIP-SHAFT-001', 1,    'PCS', 0.02, 10),
('BOM-ASSY-001', 'WIP-COVER-001', 2,    'PCS', 0.02, 20),
('BOM-ASSY-001', 'RM-BOLT-M8',   4,    'PCS', 0.00, 30);
GO

-- ============================================================
-- 工艺路线
-- ============================================================
INSERT INTO T_Routing (RoutingCode, RoutingName, ItemCode, RoutingVersion)
VALUES
('RT-SHAFT-001', N'传动轴加工工艺', 'WIP-SHAFT-001', '1.0'),
('RT-ASSY-001',  N'总成装配工艺',   'FG-ASSY-001',   '1.0');

INSERT INTO T_RoutingOperation (RoutingCode, OperationSeq, OperationCode, OperationName, WorkCenterCode, StdCycleTime, SetupTime)
VALUES
-- 传动轴加工
('RT-SHAFT-001', 10, 'OP-TURN-001',  N'粗车',     'WC-MACH-001', 120, 15),
('RT-SHAFT-001', 20, 'OP-MILL-001',  N'铣键槽',   'WC-MACH-002', 90,  10),
('RT-SHAFT-001', 30, 'OP-INSP-001',  N'中间检验', 'WC-INSP-001', 30,  5),
-- 总成装配
('RT-ASSY-001',  10, 'OP-ASSY-001',  N'装配',     'WC-ASSY-001', 180, 20),
('RT-ASSY-001',  20, 'OP-INSP-002',  N'成品检验', 'WC-INSP-001', 60,  5);
GO

-- ============================================================
-- 设备
-- ============================================================
INSERT INTO T_Equipment (EquipmentCode, EquipmentName, EquipmentType, WorkCenterCode,
                          Model, Manufacturer, PurchaseDate, LastMaintDate, NextMaintDate, Status, OEE)
VALUES
('EQ-CNC-001', N'数控车铣复合机床1', 'CNC',    'WC-MACH-001', 'VMC-850', N'某数控机床厂', '2023-06-01', '2026-05-01', '2026-08-01', 0, 0.82),
('EQ-CNC-002', N'数控铣床2',         'CNC',    'WC-MACH-002', 'VMC-650', N'某数控机床厂', '2023-06-01', '2026-04-15', '2026-07-15', 1, 0.78),
('EQ-WLD-001', N'氩弧焊机1',         'WELDER', 'WC-WELD-001', 'TIG-300', N'某焊接设备厂', '2022-01-01', '2026-03-01', '2026-06-01', 0, 0.75),
('EQ-CMM-001', N'三坐标测量仪',      'CMM',    'WC-INSP-001', 'CRYSTA',  N'某测量仪器厂', '2024-01-01', '2026-05-15', '2026-08-15', 0, 0.90);
GO

-- ============================================================
-- 缺陷代码
-- ============================================================
INSERT INTO T_DefectCode (DefectCode, DefectName, DefectCategory, Description)
VALUES
('DEF-DIM-001', N'尺寸超差',   'DIMENSION',   N'加工尺寸超出公差范围'),
('DEF-APP-001', N'表面划伤',   'APPEARANCE',  N'表面存在划痕或磕碰'),
('DEF-APP-002', N'毛刺未去除', 'APPEARANCE',  N'加工后毛刺未清除干净'),
('DEF-FUN-001', N'功能不良',   'FUNCTION',    N'装配后功能测试不通过'),
('DEF-DIM-002', N'形位公差超差', 'DIMENSION', N'圆度/圆柱度/平行度超差');
GO

-- ============================================================
-- 示例生产工单（演示用）
-- ============================================================
INSERT INTO T_WorkOrder (WorkOrderNo, ItemCode, BOMCode, RoutingCode,
                          PlanQty, PlanStartTime, PlanEndTime, Status)
VALUES
('WO-20260608-001', 'FG-ASSY-001', 'BOM-ASSY-001', 'RT-ASSY-001',
 50, '2026-06-09 08:00:00', '2026-06-13 17:00:00', 1);  -- Status=Released
GO

-- ============================================================
-- 示例物料批次（库存）
-- ============================================================
INSERT INTO T_MaterialLot (LotNo, ItemCode, Quantity, LocationCode, Status, ReceiveTime)
VALUES
('LOT-20260601-001', 'RM-STEEL-001', 200.0, 'WH-A-01-01', 0, '2026-06-01 09:00:00'),
('LOT-20260601-002', 'RM-ALUM-001',  80.0,  'WH-A-01-02', 0, '2026-06-01 09:00:00'),
('LOT-20260601-003', 'RM-BOLT-M8',   1000,  'WH-A-02-01', 0, '2026-06-01 09:00:00'),
('LOT-20260605-001', 'WIP-SHAFT-001',60,    'WH-B-01-01', 0, '2026-06-05 14:00:00'),
('LOT-20260605-002', 'WIP-COVER-001',120,   'WH-B-01-02', 0, '2026-06-05 14:00:00');
GO

PRINT '== 06_SeedData.sql 执行完毕 ==';
PRINT '已插入：5个用户、5个工作中心、6种物料、1条BOM、2条工艺路线、4台设备、5种缺陷代码';
PRINT '示例工单：WO-20260608-001（状态：已下达）';
PRINT '示例库存：5个批次';
GO
