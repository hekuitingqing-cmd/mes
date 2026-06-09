-- ============================================================
-- MES 生产执行系统 — 04 索引脚本
-- 前置条件：03_Constraints.sql 已执行完毕
-- 说明：课设场景数据量小，索引主要为演示规范性
-- ============================================================

USE MES;
GO

-- 工单：按状态查询（高频：工单列表筛选）
CREATE NONCLUSTERED INDEX IX_T_WorkOrder_Status
    ON T_WorkOrder (Status)
    INCLUDE (WorkOrderNo, ItemCode, PlanQty, ActualQty, PlanStartTime, PlanEndTime);

-- 工单：按物料查询
CREATE NONCLUSTERED INDEX IX_T_WorkOrder_ItemCode
    ON T_WorkOrder (ItemCode, Status);

-- 工单：按计划时间范围查询
CREATE NONCLUSTERED INDEX IX_T_WorkOrder_PlanTime
    ON T_WorkOrder (PlanStartTime, PlanEndTime);

-- 工序报工：按工单查询（高频：查工单实绩）
CREATE NONCLUSTERED INDEX IX_T_OperationRecord_WorkOrderNo
    ON T_OperationRecord (WorkOrderNo);

-- 物料批次：按物料+状态查询（高频：查库存）
CREATE NONCLUSTERED INDEX IX_T_MaterialLot_ItemCode_Status
    ON T_MaterialLot (ItemCode, Status)
    INCLUDE (LotNo, Quantity, LocationCode);

-- 物料批次：按库位查询
CREATE NONCLUSTERED INDEX IX_T_MaterialLot_LocationCode
    ON T_MaterialLot (LocationCode, Status);

-- 物料事务：按批次查询（高频：查流水）
CREATE NONCLUSTERED INDEX IX_T_MaterialTransaction_LotNo
    ON T_MaterialTransaction (LotNo, TransactionTime);

-- 物料事务：按工单查询（查发料记录）
CREATE NONCLUSTERED INDEX IX_T_MaterialTransaction_WorkOrderNo
    ON T_MaterialTransaction (WorkOrderNo)
    WHERE WorkOrderNo IS NOT NULL;

-- 检验单：按工单查询
CREATE NONCLUSTERED INDEX IX_T_InspectionOrder_WorkOrderNo
    ON T_InspectionOrder (WorkOrderNo)
    WHERE WorkOrderNo IS NOT NULL;

-- 检验单：按结果查询（查待检列表）
CREATE NONCLUSTERED INDEX IX_T_InspectionOrder_Result
    ON T_InspectionOrder (Result, InspectionType);

-- 设备状态流水：按设备+时间查询（OEE计算核心）
CREATE NONCLUSTERED INDEX IX_T_EquipmentStatusLog_EquipCode_Time
    ON T_EquipmentStatusLog (EquipmentCode, ChangeTime);

-- 维保工单：按设备查询
CREATE NONCLUSTERED INDEX IX_T_MaintenanceOrder_EquipmentCode
    ON T_MaintenanceOrder (EquipmentCode, Status);

-- 操作日志：按时间+模块查询
CREATE NONCLUSTERED INDEX IX_T_OperationLog_Time_Module
    ON T_OperationLog (OperationTime, Module);

GO
PRINT '== 04_Indexes.sql 执行完毕，共创建13个索引 ==';
GO
