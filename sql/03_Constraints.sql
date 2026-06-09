-- ============================================================
-- MES 生产执行系统 — 03 约束脚本
-- 包含：主键、外键、唯一约束、Check约束、Default约束
-- 前置条件：02_CreateTables.sql 已执行完毕
-- ============================================================

USE MES;
GO

-- ============================================================
-- 主键约束 (Primary Key)
-- ============================================================

ALTER TABLE T_WorkCenter        ADD CONSTRAINT PK_T_WorkCenter        PRIMARY KEY (WorkCenterCode);
ALTER TABLE T_Item              ADD CONSTRAINT PK_T_Item               PRIMARY KEY (ItemCode);
ALTER TABLE T_BOM               ADD CONSTRAINT PK_T_BOM                PRIMARY KEY (BOMCode);
ALTER TABLE T_BOMDetail         ADD CONSTRAINT PK_T_BOMDetail          PRIMARY KEY (BOMDetailId);
ALTER TABLE T_Routing           ADD CONSTRAINT PK_T_Routing            PRIMARY KEY (RoutingCode);
ALTER TABLE T_RoutingOperation  ADD CONSTRAINT PK_T_RoutingOperation   PRIMARY KEY (OperationId);
ALTER TABLE T_WorkOrder         ADD CONSTRAINT PK_T_WorkOrder          PRIMARY KEY (WorkOrderNo);
ALTER TABLE T_OperationRecord   ADD CONSTRAINT PK_T_OperationRecord    PRIMARY KEY (RecordNo);
ALTER TABLE T_MaterialLot       ADD CONSTRAINT PK_T_MaterialLot        PRIMARY KEY (LotNo);
ALTER TABLE T_MaterialTransaction ADD CONSTRAINT PK_T_MaterialTransaction PRIMARY KEY (TransactionNo);
ALTER TABLE T_DefectCode        ADD CONSTRAINT PK_T_DefectCode         PRIMARY KEY (DefectCode);
ALTER TABLE T_InspectionOrder   ADD CONSTRAINT PK_T_InspectionOrder    PRIMARY KEY (InspectionNo);
ALTER TABLE T_NCReport          ADD CONSTRAINT PK_T_NCReport           PRIMARY KEY (NCRNo);
ALTER TABLE T_Equipment         ADD CONSTRAINT PK_T_Equipment          PRIMARY KEY (EquipmentCode);
ALTER TABLE T_MaintenanceOrder  ADD CONSTRAINT PK_T_MaintenanceOrder   PRIMARY KEY (MaintOrderNo);
ALTER TABLE T_EquipmentStatusLog ADD CONSTRAINT PK_T_EquipmentStatusLog PRIMARY KEY (LogId);
ALTER TABLE T_UserAccount       ADD CONSTRAINT PK_T_UserAccount        PRIMARY KEY (UserId);
ALTER TABLE T_OperationLog      ADD CONSTRAINT PK_T_OperationLog       PRIMARY KEY (LogId);
GO
PRINT '主键约束创建完成';
GO

-- ============================================================
-- 外键约束 (Foreign Key)
-- ============================================================

-- BOM 明细 → BOM 头表
ALTER TABLE T_BOMDetail
    ADD CONSTRAINT FK_T_BOMDetail_T_BOM
    FOREIGN KEY (BOMCode) REFERENCES T_BOM(BOMCode);

-- BOM → 物料
ALTER TABLE T_BOM
    ADD CONSTRAINT FK_T_BOM_T_Item
    FOREIGN KEY (ParentItemCode) REFERENCES T_Item(ItemCode);

-- BOM 明细 → 物料（子件）
ALTER TABLE T_BOMDetail
    ADD CONSTRAINT FK_T_BOMDetail_T_Item
    FOREIGN KEY (ChildItemCode) REFERENCES T_Item(ItemCode);

-- 工艺路线 → 物料
ALTER TABLE T_Routing
    ADD CONSTRAINT FK_T_Routing_T_Item
    FOREIGN KEY (ItemCode) REFERENCES T_Item(ItemCode);

-- 工艺路线工序 → 工艺路线
ALTER TABLE T_RoutingOperation
    ADD CONSTRAINT FK_T_RoutingOperation_T_Routing
    FOREIGN KEY (RoutingCode) REFERENCES T_Routing(RoutingCode);

-- 工艺路线工序 → 工作中心
ALTER TABLE T_RoutingOperation
    ADD CONSTRAINT FK_T_RoutingOperation_T_WorkCenter
    FOREIGN KEY (WorkCenterCode) REFERENCES T_WorkCenter(WorkCenterCode);

-- 生产工单 → 物料
ALTER TABLE T_WorkOrder
    ADD CONSTRAINT FK_T_WorkOrder_T_Item
    FOREIGN KEY (ItemCode) REFERENCES T_Item(ItemCode);

-- 工序报工 → 工单
ALTER TABLE T_OperationRecord
    ADD CONSTRAINT FK_T_OperationRecord_T_WorkOrder
    FOREIGN KEY (WorkOrderNo) REFERENCES T_WorkOrder(WorkOrderNo);

-- 工序报工 → 工作中心
ALTER TABLE T_OperationRecord
    ADD CONSTRAINT FK_T_OperationRecord_T_WorkCenter
    FOREIGN KEY (WorkCenterCode) REFERENCES T_WorkCenter(WorkCenterCode);

-- 物料批次 → 物料
ALTER TABLE T_MaterialLot
    ADD CONSTRAINT FK_T_MaterialLot_T_Item
    FOREIGN KEY (ItemCode) REFERENCES T_Item(ItemCode);

-- 物料事务 → 批次
ALTER TABLE T_MaterialTransaction
    ADD CONSTRAINT FK_T_MaterialTransaction_T_MaterialLot
    FOREIGN KEY (LotNo) REFERENCES T_MaterialLot(LotNo);

-- 检验单 → 物料
ALTER TABLE T_InspectionOrder
    ADD CONSTRAINT FK_T_InspectionOrder_T_Item
    FOREIGN KEY (ItemCode) REFERENCES T_Item(ItemCode);

-- NCR → 检验单
ALTER TABLE T_NCReport
    ADD CONSTRAINT FK_T_NCReport_T_InspectionOrder
    FOREIGN KEY (InspectionNo) REFERENCES T_InspectionOrder(InspectionNo);

-- NCR → 缺陷代码
ALTER TABLE T_NCReport
    ADD CONSTRAINT FK_T_NCReport_T_DefectCode
    FOREIGN KEY (DefectCode) REFERENCES T_DefectCode(DefectCode);

-- 设备 → 工作中心
ALTER TABLE T_Equipment
    ADD CONSTRAINT FK_T_Equipment_T_WorkCenter
    FOREIGN KEY (WorkCenterCode) REFERENCES T_WorkCenter(WorkCenterCode);

-- 维保工单 → 设备
ALTER TABLE T_MaintenanceOrder
    ADD CONSTRAINT FK_T_MaintenanceOrder_T_Equipment
    FOREIGN KEY (EquipmentCode) REFERENCES T_Equipment(EquipmentCode);

-- 设备状态流水 → 设备
ALTER TABLE T_EquipmentStatusLog
    ADD CONSTRAINT FK_T_EquipmentStatusLog_T_Equipment
    FOREIGN KEY (EquipmentCode) REFERENCES T_Equipment(EquipmentCode);

GO
PRINT '外键约束创建完成';
GO

-- ============================================================
-- 唯一约束 (Unique)
-- ============================================================

ALTER TABLE T_UserAccount
    ADD CONSTRAINT UQ_T_UserAccount_LoginName
    UNIQUE (LoginName);

-- BOM 同一父件同一版本唯一
ALTER TABLE T_BOM
    ADD CONSTRAINT UQ_T_BOM_ParentItemCode_Version
    UNIQUE (ParentItemCode, BOMVersion);

-- 工艺路线同一物料同一版本唯一
ALTER TABLE T_Routing
    ADD CONSTRAINT UQ_T_Routing_ItemCode_Version
    UNIQUE (ItemCode, RoutingVersion);

-- 同一工艺路线内工序顺序唯一
ALTER TABLE T_RoutingOperation
    ADD CONSTRAINT UQ_T_RoutingOperation_RoutingCode_Seq
    UNIQUE (RoutingCode, OperationSeq);

GO
PRINT '唯一约束创建完成';
GO

-- ============================================================
-- Check 约束
-- ============================================================

-- 工单数量必须大于0
ALTER TABLE T_WorkOrder
    ADD CONSTRAINT CK_T_WorkOrder_PlanQty
    CHECK (PlanQty > 0);

-- 工单实绩数量不能为负
ALTER TABLE T_WorkOrder
    ADD CONSTRAINT CK_T_WorkOrder_ActualQty
    CHECK (ActualQty >= 0);

-- 工单状态枚举
ALTER TABLE T_WorkOrder
    ADD CONSTRAINT CK_T_WorkOrder_Status
    CHECK (Status IN (0, 1, 2, 3, 4, 9));

-- 工单计划结束时间必须晚于开始时间
ALTER TABLE T_WorkOrder
    ADD CONSTRAINT CK_T_WorkOrder_PlanTime
    CHECK (PlanEndTime > PlanStartTime);

-- 物料批次数量不能为负
ALTER TABLE T_MaterialLot
    ADD CONSTRAINT CK_T_MaterialLot_Quantity
    CHECK (Quantity >= 0);

-- 物料批次状态枚举
ALTER TABLE T_MaterialLot
    ADD CONSTRAINT CK_T_MaterialLot_Status
    CHECK (Status IN (0, 1, 2, 9));

-- 检验类型枚举
ALTER TABLE T_InspectionOrder
    ADD CONSTRAINT CK_T_InspectionOrder_Type
    CHECK (InspectionType IN ('IQC', 'IPQC', 'OQC'));

-- 检验结果枚举
ALTER TABLE T_InspectionOrder
    ADD CONSTRAINT CK_T_InspectionOrder_Result
    CHECK (Result IN ('PENDING', 'PASS', 'FAIL', 'CONDITIONAL'));

-- 不合格处置方式枚举
ALTER TABLE T_NCReport
    ADD CONSTRAINT CK_T_NCReport_Disposition
    CHECK (Disposition IN ('REWORK', 'SCRAP', 'USE-AS-IS', 'RETURN'));

-- OEE 范围 0~1
ALTER TABLE T_Equipment
    ADD CONSTRAINT CK_T_Equipment_OEE
    CHECK (OEE >= 0 AND OEE <= 1);

-- 设备状态枚举
ALTER TABLE T_Equipment
    ADD CONSTRAINT CK_T_Equipment_Status
    CHECK (Status IN (0, 1, 2, 3, 9));

-- 维保类型枚举
ALTER TABLE T_MaintenanceOrder
    ADD CONSTRAINT CK_T_MaintenanceOrder_MaintType
    CHECK (MaintType IN ('PM', 'CM', 'EM'));

-- 物料事务类型枚举
ALTER TABLE T_MaterialTransaction
    ADD CONSTRAINT CK_T_MaterialTransaction_Type
    CHECK (TransactionType IN ('RECEIPT', 'ISSUE', 'TRANSFER', 'RETURN', 'ADJUST'));

-- 用户角色枚举
ALTER TABLE T_UserAccount
    ADD CONSTRAINT CK_T_UserAccount_Role
    CHECK (Role IN ('ADMIN', 'OPERATOR', 'INSPECTOR', 'VIEWER'));

GO
PRINT 'Check约束创建完成';
GO

PRINT '== 03_Constraints.sql 执行完毕 ==';
GO
