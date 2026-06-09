-- ============================================================
-- MES 生产执行系统 — 完整数据库建表脚本
-- 适用工具：SQL Server Management Studio (SSMS)
-- 执行顺序：01_CreateDatabase → 本文件 → 03_Constraints → ...
-- 说明：所有表含公共字段 CreateTime/UpdateTime/CreateBy/IsActive
-- ============================================================

USE MES;
GO

-- ============================================================
-- SECTION 1: 基础数据（Master Data）
-- 依赖关系：无外键依赖，最先创建
-- ============================================================

-- 1.1 工作中心
IF OBJECT_ID('T_WorkCenter', 'U') IS NULL
CREATE TABLE T_WorkCenter (
    WorkCenterCode  NVARCHAR(20)  NOT NULL,          -- 工作中心编码，如 WC-CNC-001
    WorkCenterName  NVARCHAR(100) NOT NULL,          -- 工作中心名称
    WorkCenterType  NVARCHAR(50)  NOT NULL,          -- 类型：MACHINING/ASSEMBLY/INSPECTION
    Description     NVARCHAR(500) NULL,              -- 描述
    Capacity        DECIMAL(10,2) NOT NULL DEFAULT 0,-- 产能（件/小时）
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- 1.2 物料主数据
IF OBJECT_ID('T_Item', 'U') IS NULL
CREATE TABLE T_Item (
    ItemCode        NVARCHAR(50)  NOT NULL,          -- 物料编码，如 ITEM-RM-001
    ItemName        NVARCHAR(200) NOT NULL,          -- 物料名称
    ItemType        NVARCHAR(50)  NOT NULL,          -- 类型：RAW/SEMI/FINISHED
    UOM             NVARCHAR(20)  NOT NULL,          -- 计量单位：PCS/KG/M
    Spec            NVARCHAR(200) NULL,              -- 规格型号
    SafetyStock     DECIMAL(14,4) NOT NULL DEFAULT 0,-- 安全库存
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- 1.3 BOM 物料清单（头表）
IF OBJECT_ID('T_BOM', 'U') IS NULL
CREATE TABLE T_BOM (
    BOMCode         NVARCHAR(50)  NOT NULL,          -- BOM编码
    BOMName         NVARCHAR(200) NOT NULL,
    ParentItemCode  NVARCHAR(50)  NOT NULL,          -- 父件物料编码（成品/半成品）
    BOMVersion      NVARCHAR(20)  NOT NULL DEFAULT '1.0',
    EffectiveDate   DATE          NOT NULL,          -- 生效日期
    ExpiryDate      DATE          NULL,              -- 失效日期（NULL表示长期有效）
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- 1.4 BOM 明细（子件清单）
IF OBJECT_ID('T_BOMDetail', 'U') IS NULL
CREATE TABLE T_BOMDetail (
    BOMDetailId     INT           NOT NULL IDENTITY(1,1),
    BOMCode         NVARCHAR(50)  NOT NULL,          -- 关联 T_BOM.BOMCode
    ChildItemCode   NVARCHAR(50)  NOT NULL,          -- 子件物料编码
    Quantity        DECIMAL(14,4) NOT NULL,          -- 用量
    UOM             NVARCHAR(20)  NOT NULL,
    ScrapRate       DECIMAL(5,4)  NOT NULL DEFAULT 0,-- 损耗率，如 0.02 = 2%
    Seq             INT           NOT NULL DEFAULT 0,-- 序号
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- 1.5 工艺路线（头表）
IF OBJECT_ID('T_Routing', 'U') IS NULL
CREATE TABLE T_Routing (
    RoutingCode     NVARCHAR(50)  NOT NULL,          -- 工艺路线编码
    RoutingName     NVARCHAR(200) NOT NULL,
    ItemCode        NVARCHAR(50)  NOT NULL,          -- 适用物料
    RoutingVersion  NVARCHAR(20)  NOT NULL DEFAULT '1.0',
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- 1.6 工艺路线工序明细
IF OBJECT_ID('T_RoutingOperation', 'U') IS NULL
CREATE TABLE T_RoutingOperation (
    OperationId     INT           NOT NULL IDENTITY(1,1),
    RoutingCode     NVARCHAR(50)  NOT NULL,          -- 关联 T_Routing.RoutingCode
    OperationSeq    INT           NOT NULL,          -- 工序顺序：10/20/30...
    OperationCode   NVARCHAR(50)  NOT NULL,          -- 工序编码，如 OP-WELD-001
    OperationName   NVARCHAR(200) NOT NULL,
    WorkCenterCode  NVARCHAR(20)  NOT NULL,          -- 关联 T_WorkCenter
    StdCycleTime    DECIMAL(10,2) NOT NULL DEFAULT 0,-- 标准节拍（秒）
    SetupTime       DECIMAL(10,2) NOT NULL DEFAULT 0,-- 准备时间（分钟）
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- ============================================================
-- SECTION 2: 生产管理（Production）
-- 依赖：T_Item, T_Routing, T_WorkCenter
-- ============================================================

-- 2.1 生产工单
IF OBJECT_ID('T_WorkOrder', 'U') IS NULL
CREATE TABLE T_WorkOrder (
    WorkOrderNo     NVARCHAR(30)  NOT NULL,          -- 工单号，如 WO-20260608-001
    ItemCode        NVARCHAR(50)  NOT NULL,          -- 生产物料，关联 T_Item
    BOMCode         NVARCHAR(50)  NULL,              -- 使用的BOM
    RoutingCode     NVARCHAR(50)  NULL,              -- 使用的工艺路线
    PlanQty         DECIMAL(14,4) NOT NULL,          -- 计划数量
    ActualQty       DECIMAL(14,4) NOT NULL DEFAULT 0,-- 实绩数量
    ScrapQty        DECIMAL(14,4) NOT NULL DEFAULT 0,-- 报废数量
    PlanStartTime   DATETIME      NOT NULL,          -- 计划开始
    PlanEndTime     DATETIME      NOT NULL,          -- 计划结束
    ActualStartTime DATETIME      NULL,              -- 实际开始
    ActualEndTime   DATETIME      NULL,              -- 实际结束
    -- Status: 0=Created 1=Released 2=InProcess 3=Completed 4=Closed 9=Cancelled
    Status          TINYINT       NOT NULL DEFAULT 0,
    Remark          NVARCHAR(500) NULL,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- 2.2 工序报工记录
IF OBJECT_ID('T_OperationRecord', 'U') IS NULL
CREATE TABLE T_OperationRecord (
    RecordNo        NVARCHAR(30)  NOT NULL,          -- 报工单号
    WorkOrderNo     NVARCHAR(30)  NOT NULL,          -- 关联工单
    OperationCode   NVARCHAR(50)  NOT NULL,          -- 工序编码
    WorkCenterCode  NVARCHAR(20)  NOT NULL,          -- 工作中心
    GoodQty         DECIMAL(14,4) NOT NULL DEFAULT 0,-- 合格数
    ScrapQty        DECIMAL(14,4) NOT NULL DEFAULT 0,-- 报废数
    ReportTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    OperatorId      NVARCHAR(50)  NOT NULL,          -- 操作员工号
    Remark          NVARCHAR(500) NULL,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- ============================================================
-- SECTION 3: 物料管理（Material）
-- 依赖：T_Item, T_WorkOrder
-- ============================================================

-- 3.1 物料批次（库存）
IF OBJECT_ID('T_MaterialLot', 'U') IS NULL
CREATE TABLE T_MaterialLot (
    LotNo           NVARCHAR(30)  NOT NULL,          -- 批次号，如 LOT-20260608-001
    ItemCode        NVARCHAR(50)  NOT NULL,          -- 关联 T_Item
    Quantity        DECIMAL(14,4) NOT NULL DEFAULT 0,-- 当前库存数量
    LocationCode    NVARCHAR(50)  NOT NULL,          -- 库位编码，如 WH-A-01-01
    -- Status: 0=Available 1=Reserved 2=Frozen 9=Consumed
    Status          TINYINT       NOT NULL DEFAULT 0,
    ReceiveTime     DATETIME      NOT NULL DEFAULT GETDATE(),
    ExpiryDate      DATE          NULL,              -- 有效期（原材料）
    Remark          NVARCHAR(500) NULL,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- 3.2 物料事务流水
IF OBJECT_ID('T_MaterialTransaction', 'U') IS NULL
CREATE TABLE T_MaterialTransaction (
    TransactionNo   NVARCHAR(30)  NOT NULL,          -- 事务流水号，如 TXN-20260608-001
    LotNo           NVARCHAR(30)  NOT NULL,          -- 关联批次
    ItemCode        NVARCHAR(50)  NOT NULL,          -- 冗余存储，方便查询
    -- TransactionType: RECEIPT=收料 ISSUE=发料 TRANSFER=移库 RETURN=退料 ADJUST=调整
    TransactionType NVARCHAR(20)  NOT NULL,
    Quantity        DECIMAL(14,4) NOT NULL,          -- 正数=入，负数=出
    FromLocation    NVARCHAR(50)  NULL,              -- 来源库位
    ToLocation      NVARCHAR(50)  NULL,              -- 目标库位
    WorkOrderNo     NVARCHAR(30)  NULL,              -- 关联工单（发料/退料时有值）
    TransactionTime DATETIME      NOT NULL DEFAULT GETDATE(),
    OperatorId      NVARCHAR(50)  NOT NULL,
    Remark          NVARCHAR(500) NULL,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- ============================================================
-- SECTION 4: 质量管理（Quality）
-- 依赖：T_WorkOrder, T_MaterialLot, T_Item
-- ============================================================

-- 4.1 缺陷代码字典
IF OBJECT_ID('T_DefectCode', 'U') IS NULL
CREATE TABLE T_DefectCode (
    DefectCode      NVARCHAR(20)  NOT NULL,          -- 缺陷编码，如 DEF-DIM-001
    DefectName      NVARCHAR(200) NOT NULL,          -- 缺陷名称
    DefectCategory  NVARCHAR(50)  NOT NULL,          -- 缺陷分类：DIMENSION/APPEARANCE/FUNCTION
    Description     NVARCHAR(500) NULL,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- 4.2 检验单
IF OBJECT_ID('T_InspectionOrder', 'U') IS NULL
CREATE TABLE T_InspectionOrder (
    InspectionNo    NVARCHAR(30)  NOT NULL,          -- 检验单号，如 INS-20260608-001
    -- InspectionType: IQC=来料检验 IPQC=过程检验 OQC=出货检验
    InspectionType  NVARCHAR(10)  NOT NULL,
    WorkOrderNo     NVARCHAR(30)  NULL,              -- 关联工单（IPQC/OQC时有值）
    LotNo           NVARCHAR(30)  NULL,              -- 关联批次（IQC时有值）
    ItemCode        NVARCHAR(50)  NOT NULL,          -- 被检物料
    SampleQty       DECIMAL(14,4) NOT NULL,          -- 抽样数量
    NGQty           DECIMAL(14,4) NOT NULL DEFAULT 0,-- 不合格数量
    -- Result: PENDING=待检 PASS=合格 FAIL=不合格 CONDITIONAL=让步接收
    Result          NVARCHAR(20)  NOT NULL DEFAULT 'PENDING',
    InspectionTime  DATETIME      NULL,              -- 检验完成时间
    InspectorId     NVARCHAR(50)  NULL,              -- 检验员
    Remark          NVARCHAR(500) NULL,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- 4.3 不合格品报告（NCR）
IF OBJECT_ID('T_NCReport', 'U') IS NULL
CREATE TABLE T_NCReport (
    NCRNo           NVARCHAR(30)  NOT NULL,          -- NCR号，如 NCR-20260608-001
    InspectionNo    NVARCHAR(30)  NOT NULL,          -- 关联检验单
    DefectCode      NVARCHAR(20)  NOT NULL,          -- 缺陷编码
    NGQty           DECIMAL(14,4) NOT NULL,          -- 不合格数量
    -- Disposition: REWORK=返工 SCRAP=报废 USE-AS-IS=让步接收 RETURN=退货
    Disposition     NVARCHAR(20)  NOT NULL,
    RootCause       NVARCHAR(1000) NULL,             -- 原因分析
    CorrectiveAction NVARCHAR(1000) NULL,            -- 纠正措施
    -- Status: 0=Open 1=InProcess 2=Closed
    Status          TINYINT       NOT NULL DEFAULT 0,
    ClosedTime      DATETIME      NULL,
    ClosedBy        NVARCHAR(50)  NULL,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- ============================================================
-- SECTION 5: 设备管理（Equipment）
-- 依赖：T_WorkCenter
-- ============================================================

-- 5.1 设备台账
IF OBJECT_ID('T_Equipment', 'U') IS NULL
CREATE TABLE T_Equipment (
    EquipmentCode   NVARCHAR(30)  NOT NULL,          -- 设备编码，如 EQ-CNC-001
    EquipmentName   NVARCHAR(200) NOT NULL,          -- 设备名称
    EquipmentType   NVARCHAR(50)  NOT NULL,          -- 类型：CNC/ROBOT/CONVEYOR/PRESS
    WorkCenterCode  NVARCHAR(20)  NOT NULL,          -- 所属工作中心
    Model           NVARCHAR(100) NULL,              -- 型号
    Manufacturer    NVARCHAR(100) NULL,              -- 制造商
    PurchaseDate    DATE          NULL,              -- 购置日期
    LastMaintDate   DATE          NULL,              -- 上次保养日期
    NextMaintDate   DATE          NULL,              -- 下次保养日期
    -- Status: 0=Idle 1=Running 2=Fault 3=Maintenance 9=Offline
    Status          TINYINT       NOT NULL DEFAULT 0,
    OEE             DECIMAL(5,4)  NOT NULL DEFAULT 0,-- 综合设备效率（0~1）
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- 5.2 维保工单
IF OBJECT_ID('T_MaintenanceOrder', 'U') IS NULL
CREATE TABLE T_MaintenanceOrder (
    MaintOrderNo    NVARCHAR(30)  NOT NULL,          -- 维保工单号，如 MO-20260608-001
    EquipmentCode   NVARCHAR(30)  NOT NULL,          -- 关联设备
    -- MaintType: PM=预防性保养 CM=纠正性维修 EM=紧急抢修
    MaintType       NVARCHAR(10)  NOT NULL,
    Description     NVARCHAR(500) NULL,              -- 故障描述/保养内容
    PlanDate        DATE          NOT NULL,          -- 计划日期
    ActualDate      DATE          NULL,              -- 实际完成日期
    MaintResult     NVARCHAR(500) NULL,              -- 维保结果
    DowntimeMinutes INT           NOT NULL DEFAULT 0,-- 停机时长（分钟）
    TechnicianId    NVARCHAR(50)  NULL,              -- 维修技术员
    -- Status: 0=Planned 1=InProcess 2=Completed 9=Cancelled
    Status          TINYINT       NOT NULL DEFAULT 0,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- 5.3 设备状态流水（用于OEE计算）
IF OBJECT_ID('T_EquipmentStatusLog', 'U') IS NULL
CREATE TABLE T_EquipmentStatusLog (
    LogId           INT           NOT NULL IDENTITY(1,1),
    EquipmentCode   NVARCHAR(30)  NOT NULL,          -- 关联设备
    FromStatus      TINYINT       NOT NULL,          -- 变更前状态
    ToStatus        TINYINT       NOT NULL,          -- 变更后状态
    ChangeTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    DurationMinutes INT           NULL,              -- 上一状态持续时长（分钟）
    Operator        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM',
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- ============================================================
-- SECTION 6: 系统表
-- ============================================================

-- 6.1 用户账号
IF OBJECT_ID('T_UserAccount', 'U') IS NULL
CREATE TABLE T_UserAccount (
    UserId          NVARCHAR(50)  NOT NULL,          -- 工号，如 EMP-001
    UserName        NVARCHAR(100) NOT NULL,          -- 姓名
    LoginName       NVARCHAR(50)  NOT NULL,          -- 登录名
    PasswordHash    NVARCHAR(200) NOT NULL,          -- 密码哈希（MD5/SHA256）
    -- Role: ADMIN=管理员 OPERATOR=操作员 INSPECTOR=检验员 VIEWER=只读
    Role            NVARCHAR(20)  NOT NULL DEFAULT 'OPERATOR',
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdateTime      DATETIME      NOT NULL DEFAULT GETDATE(),
    CreateBy        NVARCHAR(50)  NOT NULL DEFAULT 'SYSTEM'
);
GO

-- 6.2 操作日志
IF OBJECT_ID('T_OperationLog', 'U') IS NULL
CREATE TABLE T_OperationLog (
    LogId           BIGINT        NOT NULL IDENTITY(1,1),
    UserId          NVARCHAR(50)  NOT NULL,
    Module          NVARCHAR(50)  NOT NULL,          -- 模块：Production/Material/Quality/Equipment
    Action          NVARCHAR(100) NOT NULL,          -- 动作：CreateWorkOrder/IssueMateria等
    Detail          NVARCHAR(2000) NULL,             -- 详情（可存JSON）
    Result          NVARCHAR(10)  NOT NULL DEFAULT 'SUCCESS', -- SUCCESS/FAIL
    OperationTime   DATETIME      NOT NULL DEFAULT GETDATE(),
    ClientIP        NVARCHAR(50)  NULL
);
GO

PRINT '== 建表完成，共创建15张表 ==';
PRINT '基础数据: T_WorkCenter, T_Item, T_BOM, T_BOMDetail, T_Routing, T_RoutingOperation';
PRINT '生产管理: T_WorkOrder, T_OperationRecord';
PRINT '物料管理: T_MaterialLot, T_MaterialTransaction';
PRINT '质量管理: T_DefectCode, T_InspectionOrder, T_NCReport';
PRINT '设备管理: T_Equipment, T_MaintenanceOrder, T_EquipmentStatusLog';
PRINT '系统表: T_UserAccount, T_OperationLog';
GO
