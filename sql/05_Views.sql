-- ============================================================
-- MES 生产执行系统 — 05 视图脚本
-- 前置条件：04_Indexes.sql 已执行完毕
-- ============================================================

USE MES;
GO

-- ============================================================
-- V_WorkOrderSummary — 工单汇总视图
-- 用途：工单列表界面，含物料名称、完工率
-- ============================================================
CREATE OR ALTER VIEW V_WorkOrderSummary AS
SELECT
    wo.WorkOrderNo,
    wo.ItemCode,
    i.ItemName,
    wo.PlanQty,
    wo.ActualQty,
    wo.ScrapQty,
    wo.PlanStartTime,
    wo.PlanEndTime,
    wo.ActualStartTime,
    wo.ActualEndTime,
    wo.Status,
    CASE wo.Status
        WHEN 0 THEN N'已创建'
        WHEN 1 THEN N'已下达'
        WHEN 2 THEN N'生产中'
        WHEN 3 THEN N'已完工'
        WHEN 4 THEN N'已关闭'
        WHEN 9 THEN N'已取消'
        ELSE N'未知'
    END AS StatusName,
    -- 完工率（百分比）
    CASE WHEN wo.PlanQty > 0
         THEN CAST(wo.ActualQty * 100.0 / wo.PlanQty AS DECIMAL(5,2))
         ELSE 0
    END AS CompletionRate,
    wo.CreateTime,
    wo.CreateBy
FROM T_WorkOrder wo
LEFT JOIN T_Item i ON wo.ItemCode = i.ItemCode
WHERE wo.IsActive = 1;
GO

-- ============================================================
-- V_StockSummary — 库存汇总视图
-- 用途：库存查询界面，按物料汇总库存
-- ============================================================
CREATE OR ALTER VIEW V_StockSummary AS
SELECT
    ml.ItemCode,
    i.ItemName,
    i.UOM,
    ml.LocationCode,
    SUM(ml.Quantity) AS TotalQty,
    COUNT(ml.LotNo)  AS LotCount,
    -- 安全库存预警
    CASE WHEN SUM(ml.Quantity) < i.SafetyStock THEN 1 ELSE 0 END AS BelowSafety
FROM T_MaterialLot ml
LEFT JOIN T_Item i ON ml.ItemCode = i.ItemCode
WHERE ml.IsActive = 1
  AND ml.Status = 0   -- 仅可用状态
GROUP BY ml.ItemCode, i.ItemName, i.UOM, ml.LocationCode, i.SafetyStock;
GO

-- ============================================================
-- V_InspectionSummary — 检验单汇总视图
-- 用途：检验列表界面，含不良率
-- ============================================================
CREATE OR ALTER VIEW V_InspectionSummary AS
SELECT
    ins.InspectionNo,
    ins.InspectionType,
    ins.WorkOrderNo,
    ins.LotNo,
    ins.ItemCode,
    i.ItemName,
    ins.SampleQty,
    ins.NGQty,
    CASE WHEN ins.SampleQty > 0
         THEN CAST(ins.NGQty * 100.0 / ins.SampleQty AS DECIMAL(5,2))
         ELSE 0
    END AS NGRate,
    ins.Result,
    ins.InspectionTime,
    ins.InspectorId,
    ins.CreateTime
FROM T_InspectionOrder ins
LEFT JOIN T_Item i ON ins.ItemCode = i.ItemCode
WHERE ins.IsActive = 1;
GO

-- ============================================================
-- V_EquipmentStatus — 设备状态视图
-- 用途：设备状态看板
-- ============================================================
CREATE OR ALTER VIEW V_EquipmentStatus AS
SELECT
    eq.EquipmentCode,
    eq.EquipmentName,
    eq.EquipmentType,
    eq.WorkCenterCode,
    wc.WorkCenterName,
    eq.Status,
    CASE eq.Status
        WHEN 0 THEN N'空闲'
        WHEN 1 THEN N'运行中'
        WHEN 2 THEN N'故障'
        WHEN 3 THEN N'维保中'
        WHEN 9 THEN N'离线'
        ELSE N'未知'
    END AS StatusName,
    CASE eq.Status
        WHEN 0 THEN 'Gray'
        WHEN 1 THEN 'Green'
        WHEN 2 THEN 'Red'
        WHEN 3 THEN 'Orange'
        WHEN 9 THEN 'DarkGray'
        ELSE 'White'
    END AS StatusColor,         -- 供UI直接使用的颜色标识
    eq.OEE,
    CAST(eq.OEE * 100 AS DECIMAL(5,2)) AS OEEPercent,
    eq.LastMaintDate,
    eq.NextMaintDate
FROM T_Equipment eq
LEFT JOIN T_WorkCenter wc ON eq.WorkCenterCode = wc.WorkCenterCode
WHERE eq.IsActive = 1;
GO

PRINT '== 05_Views.sql 执行完毕，共创建4个视图 ==';
PRINT 'V_WorkOrderSummary, V_StockSummary, V_InspectionSummary, V_EquipmentStatus';
GO
