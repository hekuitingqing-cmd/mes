# MES System — Codex Build Instructions

> **READ THIS ENTIRE FILE BEFORE WRITING ANY CODE.**
> Follow every rule exactly. Do not infer, improvise, or skip steps.

---

## Project Overview

- **Solution**: `MES.sln`
- **Language**: C# (.NET 8)
- **Config**: appsettings.json (NOT App.config)
- **SqlClient**: Microsoft.Data.SqlClient (NOT System.Data.SqlClient)
- **Database**: SQL Server (via Dapper)
- **Pattern**: 3-Layer Architecture — UI → BLL → DAL
- **UI**: WinForms — **DO NOT generate any UI files in this task**

---

## Solution Structure & Project References

```
MES.sln
├── MES.Model       → references: NONE
├── MES.Common      → references: NONE
├── MES.DAL         → references: MES.Model, MES.Common
├── MES.BLL         → references: MES.DAL, MES.Model, MES.Common
└── MES.UI          → references: MES.BLL, MES.Model  ← DO NOT TOUCH
```

**Hard rules on references:**
- `MES.UI` must NEVER reference `MES.DAL` directly
- `MES.BLL` must NEVER use `SqlConnection` or any DAL concrete class directly
- `MES.DAL` must NEVER contain business logic (no status validation, no quantity checks)

---

## NuGet Packages

`MES.DAL` requires:
```xml
<PackageReference Include="Dapper" Version="2.1.28" />
<PackageReference Include="System.Data.SqlClient" Version="4.8.6" />
```

No other NuGet packages. Do not add Entity Framework.

---

## Namespace Rules (STRICT — do not deviate)

```
MES.Model.Master        → WorkCenter, Item, BOM, BOMDetail, Routing, RoutingOperation
MES.Model.Production    → WorkOrder, WorkOrderStatus (enum), OperationRecord, WorkOrderQueryParam
MES.Model.Material      → MaterialLot, LotStatus (enum), MaterialTransaction
MES.Model.Quality       → InspectionOrder, NCReport
MES.Model.Equipment     → Equipment, EquipmentStatus (enum), MaintenanceOrder, EquipmentStatusLog

MES.Common              → CodeGenerator, MESLogger, ServiceResult, DbContext

MES.DAL.Interfaces      → IRepository<T>, IWorkOrderRepository,
                           IMaterialLotRepository, IInspectionOrderRepository,
                           IEquipmentRepository
MES.DAL.Implement       → WorkOrderRepository, OperationRecordRepository,
                           MaterialLotRepository, MaterialTransactionRepository,
                           InspectionOrderRepository, NCReportRepository,
                           EquipmentRepository, MaintenanceOrderRepository,
                           MasterDataRepository

MES.BLL.Interfaces      → IWorkOrderService, IMaterialService,
                           IQualityService, IEquipmentService
MES.BLL.Implement       → WorkOrderService, MaterialService,
                           QualityService, EquipmentService
```

---

## App.config — Connection String

Place in both `MES.UI/App.config` and `MES.DAL/App.config`:

```xml
<configuration>
  <connectionStrings>
    <add name="MESConn"
         connectionString="Server=localhost;Database=MES;User Id=sa;Password=sa123456;TrustServerCertificate=True;"
         providerName="System.Data.SqlClient"/>
  </connectionStrings>
</configuration>
```

---

## Build Order — MANDATORY SEQUENCE

Complete each step fully and ensure it **compiles with zero errors** before proceeding.

```
STEP 1  →  MES.Model        (all entity classes + enums)
STEP 2  →  MES.Common       (ServiceResult, CodeGenerator, MESLogger, DbContext)
STEP 3  →  MES.DAL/Interfaces   (IRepository<T> + 4 module interfaces)
STEP 4  →  MES.BLL/Interfaces   (4 service interfaces)
STEP 5  →  MES.DAL/Implement    (all Repository implementations)
STEP 6  →  MES.BLL/Implement    (all Service implementations)
STEP 7  →  MES.UI           ← SKIP entirely — do not create any file under MES.UI
```

---

## STEP 1 — MES.Model

### Naming conventions for all entities
- All primary key fields: type `string`, not `int`
- All quantity fields: type `decimal`, NOT `float` or `double`
- All datetime fields: type `DateTime` (nullable `DateTime?` where specified)
- All status fields: use the corresponding `enum` type
- Every entity must include these 4 audit fields:

```csharp
public DateTime CreateTime { get; set; } = DateTime.Now;
public DateTime UpdateTime { get; set; } = DateTime.Now;
public string   CreateBy   { get; set; } = "SYSTEM";
public bool     IsActive   { get; set; } = true;
```

### MES.Model.Master

**WorkCenter.cs**
```csharp
namespace MES.Model.Master
{
    public class WorkCenter
    {
        public string   WorkCenterCode { get; set; }
        public string   WorkCenterName { get; set; }
        public string   WorkCenterType { get; set; }  // MACHINING / ASSEMBLY / INSPECTION
        public string   Description    { get; set; }
        public decimal  Capacity       { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

**Item.cs**
```csharp
namespace MES.Model.Master
{
    public class Item
    {
        public string  ItemCode    { get; set; }
        public string  ItemName    { get; set; }
        public string  ItemType    { get; set; }  // RAW / SEMI / FINISHED
        public string  UOM         { get; set; }
        public string  Spec        { get; set; }
        public decimal SafetyStock { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

**BOM.cs**
```csharp
namespace MES.Model.Master
{
    public class BOM
    {
        public string    BOMCode        { get; set; }
        public string    BOMName        { get; set; }
        public string    ParentItemCode { get; set; }
        public string    BOMVersion     { get; set; } = "1.0";
        public DateTime  EffectiveDate  { get; set; }
        public DateTime? ExpiryDate     { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

**BOMDetail.cs**
```csharp
namespace MES.Model.Master
{
    public class BOMDetail
    {
        public int     BOMDetailId   { get; set; }
        public string  BOMCode       { get; set; }
        public string  ChildItemCode { get; set; }
        public decimal Quantity      { get; set; }
        public string  UOM           { get; set; }
        public decimal ScrapRate     { get; set; }
        public int     Seq           { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

**Routing.cs**
```csharp
namespace MES.Model.Master
{
    public class Routing
    {
        public string RoutingCode    { get; set; }
        public string RoutingName    { get; set; }
        public string ItemCode       { get; set; }
        public string RoutingVersion { get; set; } = "1.0";
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

**RoutingOperation.cs**
```csharp
namespace MES.Model.Master
{
    public class RoutingOperation
    {
        public int     OperationId    { get; set; }
        public string  RoutingCode    { get; set; }
        public int     OperationSeq   { get; set; }
        public string  OperationCode  { get; set; }
        public string  OperationName  { get; set; }
        public string  WorkCenterCode { get; set; }
        public decimal StdCycleTime   { get; set; }
        public decimal SetupTime      { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

### MES.Model.Production

**WorkOrderStatus.cs**
```csharp
namespace MES.Model.Production
{
    public enum WorkOrderStatus
    {
        Created   = 0,
        Released  = 1,
        InProcess = 2,
        Completed = 3,
        Closed    = 4,
        Cancelled = 9
    }
}
```

**WorkOrder.cs**
```csharp
namespace MES.Model.Production
{
    public class WorkOrder
    {
        public string          WorkOrderNo     { get; set; }
        public string          ItemCode        { get; set; }
        public string          BOMCode         { get; set; }
        public string          RoutingCode     { get; set; }
        public decimal         PlanQty         { get; set; }
        public decimal         ActualQty       { get; set; }
        public decimal         ScrapQty        { get; set; }
        public DateTime        PlanStartTime   { get; set; }
        public DateTime        PlanEndTime     { get; set; }
        public DateTime?       ActualStartTime { get; set; }
        public DateTime?       ActualEndTime   { get; set; }
        public WorkOrderStatus Status          { get; set; } = WorkOrderStatus.Created;
        public string          Remark          { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

**WorkOrderQueryParam.cs**
```csharp
using System;

namespace MES.Model.Production
{
    public class WorkOrderQueryParam
    {
        public WorkOrderStatus? Status    { get; set; }
        public string           ItemCode  { get; set; }
        public DateTime?        StartDate { get; set; }
        public DateTime?        EndDate   { get; set; }
    }
}
```

**OperationRecord.cs**
```csharp
using System;

namespace MES.Model.Production
{
    public class OperationRecord
    {
        public string   RecordNo       { get; set; }
        public string   WorkOrderNo    { get; set; }
        public string   OperationCode  { get; set; }
        public string   WorkCenterCode { get; set; }
        public decimal  GoodQty        { get; set; }
        public decimal  ScrapQty       { get; set; }
        public DateTime ReportTime     { get; set; } = DateTime.Now;
        public string   OperatorId     { get; set; }
        public string   Remark         { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

### MES.Model.Material

**LotStatus.cs**
```csharp
namespace MES.Model.Material
{
    public enum LotStatus
    {
        Available = 0,
        Reserved  = 1,
        Frozen    = 2,
        Consumed  = 9
    }
}
```

**MaterialLot.cs**
```csharp
using System;

namespace MES.Model.Material
{
    public class MaterialLot
    {
        public string    LotNo        { get; set; }
        public string    ItemCode     { get; set; }
        public decimal   Quantity     { get; set; }
        public string    LocationCode { get; set; }
        public LotStatus Status       { get; set; } = LotStatus.Available;
        public DateTime  ReceiveTime  { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate   { get; set; }
        public string    Remark       { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

**MaterialTransaction.cs**
```csharp
using System;

namespace MES.Model.Material
{
    public class MaterialTransaction
    {
        public string   TransactionNo   { get; set; }
        public string   LotNo           { get; set; }
        public string   ItemCode        { get; set; }
        // RECEIPT / ISSUE / TRANSFER / RETURN / ADJUST
        public string   TransactionType { get; set; }
        public decimal  Quantity        { get; set; }  // positive=in, negative=out
        public string   FromLocation    { get; set; }
        public string   ToLocation      { get; set; }
        public string   WorkOrderNo     { get; set; }
        public DateTime TransactionTime { get; set; } = DateTime.Now;
        public string   OperatorId      { get; set; }
        public string   Remark          { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

### MES.Model.Quality

**InspectionOrder.cs**
```csharp
using System;

namespace MES.Model.Quality
{
    public class InspectionOrder
    {
        public string    InspectionNo   { get; set; }
        // IQC / IPQC / OQC
        public string    InspectionType { get; set; }
        public string    WorkOrderNo    { get; set; }
        public string    LotNo          { get; set; }
        public string    ItemCode       { get; set; }
        public decimal   SampleQty      { get; set; }
        public decimal   NGQty          { get; set; }
        // PENDING / PASS / FAIL / CONDITIONAL
        public string    Result         { get; set; } = "PENDING";
        public DateTime? InspectionTime { get; set; }
        public string    InspectorId    { get; set; }
        public string    Remark         { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

**NCReport.cs**
```csharp
using System;

namespace MES.Model.Quality
{
    public class NCReport
    {
        public string    NCRNo            { get; set; }
        public string    InspectionNo     { get; set; }
        public string    DefectCode       { get; set; }
        public decimal   NGQty            { get; set; }
        // REWORK / SCRAP / USE-AS-IS / RETURN
        public string    Disposition      { get; set; }
        public string    RootCause        { get; set; }
        public string    CorrectiveAction { get; set; }
        // 0=Open 1=InProcess 2=Closed
        public int       Status           { get; set; }
        public DateTime? ClosedTime       { get; set; }
        public string    ClosedBy         { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

### MES.Model.Equipment

**EquipmentStatus.cs**
```csharp
namespace MES.Model.Equipment
{
    public enum EquipmentStatus
    {
        Idle        = 0,
        Running     = 1,
        Fault       = 2,
        Maintenance = 3,
        Offline     = 9
    }
}
```

**Equipment.cs**
```csharp
using System;

namespace MES.Model.Equipment
{
    public class Equipment
    {
        public string          EquipmentCode  { get; set; }
        public string          EquipmentName  { get; set; }
        public string          EquipmentType  { get; set; }
        public string          WorkCenterCode { get; set; }
        public string          Model          { get; set; }
        public string          Manufacturer   { get; set; }
        public DateTime?       PurchaseDate   { get; set; }
        public DateTime?       LastMaintDate  { get; set; }
        public DateTime?       NextMaintDate  { get; set; }
        public EquipmentStatus Status         { get; set; } = EquipmentStatus.Idle;
        public decimal         OEE            { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

**EquipmentStatusLog.cs**
```csharp
using System;

namespace MES.Model.Equipment
{
    public class EquipmentStatusLog
    {
        public int             LogId           { get; set; }
        public string          EquipmentCode   { get; set; }
        public EquipmentStatus FromStatus      { get; set; }
        public EquipmentStatus ToStatus        { get; set; }
        public DateTime        ChangeTime      { get; set; }
        public int             DurationMinutes { get; set; }
        public string          Operator        { get; set; }
        public bool            IsActive        { get; set; } = true;
        public DateTime        CreateTime      { get; set; } = DateTime.Now;
        public DateTime        UpdateTime      { get; set; } = DateTime.Now;
        public string          CreateBy        { get; set; } = "SYSTEM";
    }
}
```

**MaintenanceOrder.cs**
```csharp
using System;

namespace MES.Model.Equipment
{
    public class MaintenanceOrder
    {
        public string    MaintOrderNo    { get; set; }
        public string    EquipmentCode   { get; set; }
        // PM / CM / EM
        public string    MaintType       { get; set; }
        public string    Description     { get; set; }
        public DateTime  PlanDate        { get; set; }
        public DateTime? ActualDate      { get; set; }
        public string    MaintResult     { get; set; }
        public int       DowntimeMinutes { get; set; }
        public string    TechnicianId    { get; set; }
        // 0=Planned 1=InProcess 2=Completed 9=Cancelled
        public int       Status          { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string   CreateBy   { get; set; } = "SYSTEM";
        public bool     IsActive   { get; set; } = true;
    }
}
```

---

## STEP 2 — MES.Common

### ServiceResult.cs
```csharp
namespace MES.Common
{
    public class ServiceResult
    {
        public bool   Success { get; set; }
        public string Message { get; set; }
        public object Data    { get; set; }

        public static ServiceResult Ok(object data = null)
            => new ServiceResult { Success = true, Data = data };

        public static ServiceResult Fail(string message)
            => new ServiceResult { Success = false, Message = message };
    }
}
```

### CodeGenerator.cs
```csharp
using System;

namespace MES.Common
{
    public static class CodeGenerator
    {
        private static readonly object _lock = new object();
        private static int    _seqCounter = 0;
        private static string _lastDate   = "";

        public static string Generate(string prefix)
        {
            lock (_lock)
            {
                string today = DateTime.Now.ToString("yyyyMMdd");
                if (today != _lastDate) { _seqCounter = 0; _lastDate = today; }
                _seqCounter++;
                return $"{prefix}-{today}-{_seqCounter:D3}";
            }
        }

        public static string GenerateWorkOrderNo()   => Generate("WO");
        public static string GenerateLotNo()         => Generate("LOT");
        public static string GenerateInspectionNo()  => Generate("INS");
        public static string GenerateNCRNo()         => Generate("NCR");
        public static string GenerateMaintOrderNo()  => Generate("MO");
        public static string GenerateTransactionNo() => Generate("TXN");
        public static string GenerateRecordNo()      => Generate("OPR");
    }
}
```

### MESLogger.cs
```csharp
using System;
using System.IO;

namespace MES.Common
{
    public static class MESLogger
    {
        private static readonly object _lock = new object();

        public static void Info(string module, string action, string detail)
            => Write("INFO", module, action, detail);

        public static void Error(string module, string action, Exception ex)
            => Write("ERROR", module, action, ex?.ToString());

        private static void Write(string level, string module, string action, string detail)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] [{module}] [{action}] {detail}";
            Console.WriteLine(line);
            lock (_lock)
            {
                string path = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    $"mes_log_{DateTime.Now:yyyyMMdd}.txt");
                File.AppendAllText(path, line + Environment.NewLine);
            }
        }
    }
}
```

### DbContext.cs
```csharp
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MES.Common
{
    public static class DbContext
    {
        private static readonly string _connStr =
            ConfigurationManager.ConnectionStrings["MESConn"].ConnectionString;

        public static IDbConnection GetConnection()
            => new SqlConnection(_connStr);
    }
}
```

---

## STEP 3 — MES.DAL/Interfaces

### IRepository.cs
```csharp
using System.Collections.Generic;

namespace MES.DAL.Interfaces
{
    public interface IRepository<T> where T : class
    {
        T       GetById(string id);
        List<T> GetAll();
        List<T> GetByCondition(string whereClause, object parameters);
        bool    Insert(T entity);
        bool    Update(T entity);
        bool    Delete(string id);  // soft delete only
    }
}
```

### IWorkOrderRepository.cs
```csharp
using MES.Model.Production;
using System;
using System.Collections.Generic;

namespace MES.DAL.Interfaces
{
    public interface IWorkOrderRepository : IRepository<WorkOrder>
    {
        List<WorkOrder> GetByStatus(WorkOrderStatus status);
        List<WorkOrder> GetByDateRange(DateTime start, DateTime end);
        bool UpdateStatus(string workOrderNo, WorkOrderStatus newStatus);
        bool UpdateActualQty(string workOrderNo, decimal goodQty, decimal scrapQty);
    }
}
```

### IMaterialLotRepository.cs
```csharp
using MES.Model.Material;
using System.Collections.Generic;

namespace MES.DAL.Interfaces
{
    public interface IMaterialLotRepository : IRepository<MaterialLot>
    {
        List<MaterialLot> GetByItemCode(string itemCode);
        List<MaterialLot> GetByLocation(string locationCode);
        bool    UpdateQuantity(string lotNo, decimal newQuantity);
        bool    UpdateStatus(string lotNo, LotStatus newStatus);
        decimal GetTotalStock(string itemCode);
    }
}
```

### IInspectionOrderRepository.cs
```csharp
using MES.Model.Quality;
using System.Collections.Generic;

namespace MES.DAL.Interfaces
{
    public interface IInspectionOrderRepository : IRepository<InspectionOrder>
    {
        List<InspectionOrder> GetByWorkOrder(string workOrderNo);
        List<InspectionOrder> GetByType(string inspectionType);
        List<InspectionOrder> GetPending();
        bool UpdateResult(string inspectionNo, decimal ngQty, string result);
    }
}
```

### IEquipmentRepository.cs
```csharp
using MES.Model.Equipment;
using System;
using System.Collections.Generic;

namespace MES.DAL.Interfaces
{
    public interface IEquipmentRepository : IRepository<Equipment>
    {
        List<Equipment>          GetByStatus(EquipmentStatus status);
        bool                     UpdateStatus(string equipmentCode, EquipmentStatus status);
        bool                     UpdateOEE(string equipmentCode, decimal oee);
        List<MaintenanceOrder>   GetMaintenanceOrders(string equipmentCode);
        List<EquipmentStatusLog> GetStatusLogs(string equipmentCode, DateTime start, DateTime end);
    }
}
```

---

## STEP 4 — MES.BLL/Interfaces

### IWorkOrderService.cs
```csharp
using MES.Common;
using MES.Model.Production;
using System.Collections.Generic;

namespace MES.BLL.Interfaces
{
    public interface IWorkOrderService
    {
        ServiceResult   CreateWorkOrder(WorkOrder wo);       // Data = WorkOrderNo
        ServiceResult   ReleaseWorkOrder(string workOrderNo);
        ServiceResult   ReportOperation(OperationRecord record); // Data = updated WorkOrder
        ServiceResult   CloseWorkOrder(string workOrderNo);
        WorkOrder       GetWorkOrderDetail(string workOrderNo);
        List<WorkOrder> QueryWorkOrders(WorkOrderQueryParam param);
    }
}
```

### IMaterialService.cs
```csharp
using MES.Common;
using MES.Model.Material;
using System.Collections.Generic;

namespace MES.BLL.Interfaces
{
    public interface IMaterialService
    {
        ServiceResult     ReceiveMaterial(MaterialLot lot);                                          // Data = LotNo
        ServiceResult     IssueMaterial(string lotNo, string workOrderNo, decimal qty, string operatorId);
        ServiceResult     TransferMaterial(string lotNo, string targetLocation, string operatorId);
        List<MaterialLot> QueryStock(string itemCode);  // null = all
    }
}
```

### IQualityService.cs
```csharp
using MES.Common;
using MES.Model.Quality;
using System.Collections.Generic;

namespace MES.BLL.Interfaces
{
    public interface IQualityService
    {
        ServiceResult         CreateInspectionOrder(InspectionOrder order);                          // Data = InspectionNo
        ServiceResult         SubmitInspectionResult(string inspectionNo, decimal ngQty, string result);
        ServiceResult         CreateNCR(NCReport ncr);                                               // Data = NCRNo
        List<InspectionOrder> GetPendingInspections();
    }
}
```

### IEquipmentService.cs
```csharp
using MES.Common;
using MES.Model.Equipment;
using System;

namespace MES.BLL.Interfaces
{
    public interface IEquipmentService
    {
        ServiceResult UpdateEquipmentStatus(string equipmentCode, EquipmentStatus newStatus, string operatorId);
        ServiceResult CreateMaintenanceOrder(MaintenanceOrder mo);      // Data = MaintOrderNo
        ServiceResult CompleteMaintenanceOrder(string maintOrderNo, string result, string technicianId);
        decimal       CalculateOEE(string equipmentCode, DateTime start, DateTime end);
    }
}
```

---

## STEP 5 — MES.DAL/Implement

### Rules for ALL Repository implementations

1. Use `DbContext.GetConnection()` — do not accept IDbConnection in constructor
2. Every SELECT must include `WHERE IsActive = 1` (except log tables)
3. `Delete(string id)` must execute: `UPDATE ... SET IsActive=0, UpdateTime=GETDATE() WHERE [PK]=@id`
4. Use `@ParamName` parameters always — no string concatenation in SQL
5. Enum fields: cast to `(int)` on write, Dapper maps `int` back; use explicit cast on read if needed
6. Multi-step writes (e.g. update lot + insert transaction) must use `IDbTransaction`

### Key SQL patterns

**WorkOrderRepository — UpdateActualQty**
```sql
UPDATE T_WorkOrder
SET ActualQty  = ActualQty  + @GoodQty,
    ScrapQty   = ScrapQty   + @ScrapQty,
    UpdateTime = GETDATE()
WHERE WorkOrderNo = @WorkOrderNo AND IsActive = 1
```

**MaterialLotRepository — GetTotalStock**
```sql
SELECT ISNULL(SUM(Quantity), 0)
FROM T_MaterialLot
WHERE ItemCode = @ItemCode AND Status = 0 AND IsActive = 1
```

**EquipmentRepository — UpdateStatus (must also insert log row)**
```sql
-- Step 1: get current status
SELECT Status FROM T_Equipment WHERE EquipmentCode = @EquipmentCode

-- Step 2: update equipment
UPDATE T_Equipment
SET Status = @NewStatus, UpdateTime = GETDATE()
WHERE EquipmentCode = @EquipmentCode AND IsActive = 1

-- Step 3: insert status log
INSERT INTO T_EquipmentStatusLog
    (EquipmentCode, FromStatus, ToStatus, ChangeTime, Operator, IsActive, CreateTime, UpdateTime, CreateBy)
VALUES
    (@EquipmentCode, @FromStatus, @NewStatus, GETDATE(), @Operator, 1, GETDATE(), GETDATE(), 'SYSTEM')
```

---

## STEP 6 — MES.BLL/Implement

### Rules for ALL Service implementations

1. Constructor receives repository interfaces only — never instantiate concrete repositories
2. Every public method: wrap in `try/catch(Exception ex)`, call `MESLogger.Error(...)`, return `ServiceResult.Fail("系统错误，请联系管理员")`
3. Validate inputs before any DB call — return `ServiceResult.Fail(reason)` on violation
4. Never return `null`

### WorkOrderService — state machine (STRICT)

```
CreateWorkOrder   → Status = Created(0)
ReleaseWorkOrder  → Created(0)   → Released(1)    FAIL if Status != Created
ReportOperation   → Released(1)  → InProcess(2)   on first report only
                    InProcess stays InProcess      on subsequent reports
                    Sets ActualStartTime on first report
CloseWorkOrder    → Released(1) or InProcess(2) → Closed(4)
                    FAIL if ActualQty == 0
```

Forbidden — always return `ServiceResult.Fail`:
- Release a Completed/Closed/Cancelled order
- Close a Created order
- Report on a Closed or Cancelled order

### MaterialService.IssueMaterial — exact validation sequence

```
1. Load lot by lotNo
2. If not found or IsActive=false  → Fail("批次不存在")
3. If lot.Status != Available(0)   → Fail("批次状态不可用")
4. If lot.Quantity < qty           → Fail($"库存不足，当前库存: {lot.Quantity}")
5. Begin transaction
6. newQty = lot.Quantity - qty
7. UpdateQuantity(lotNo, newQty)
8. If newQty == 0: UpdateStatus(lotNo, Consumed)
9. Insert MaterialTransaction: type=ISSUE, quantity=-qty
10. Commit → return Ok()
```

### QualityService.SubmitInspectionResult — exact validation sequence

```
1. Load order by inspectionNo
2. If not found                → Fail("检验单不存在")
3. If Result != "PENDING"      → Fail("检验单已提交，不可重复提交")
4. If ngQty > SampleQty        → Fail("不合格数不能超过抽样数")
5. UpdateResult(inspectionNo, ngQty, result)
6. If result == "FAIL": CreateNCR with Disposition=SCRAP, Status=0
7. Return Ok()
```

### EquipmentService.CalculateOEE — algorithm

```
1. GetStatusLogs(equipmentCode, start, end)
2. runningMinutes = sum of DurationMinutes where ToStatus == Running(1)
3. totalMinutes   = (end - start).TotalMinutes
4. If totalMinutes <= 0: return 0
5. oee = (decimal)(runningMinutes / totalMinutes)
6. oee = Math.Max(0, Math.Min(1, oee))
7. UpdateOEE(equipmentCode, oee)
8. Return oee
```

---

## ABSOLUTE PROHIBITIONS

- DO NOT generate any file under `MES.UI/`
- DO NOT use `SELECT *` anywhere
- DO NOT use `DELETE FROM` — always soft delete with `IsActive = 0`
- DO NOT use `float` or `double` for quantity fields — use `decimal`
- DO NOT hardcode connection strings in `.cs` files
- DO NOT add Entity Framework or any ORM other than Dapper
- DO NOT omit `IsActive = 1` filter on any SELECT query
- DO NOT call DAL concrete classes from BLL — use interfaces only
- DO NOT throw exceptions to the caller — catch and return ServiceResult

---

## Verification Checklist

After STEP 1: `MES.Model` → 0 errors, 0 warnings  
After STEP 2: `MES.Common` → 0 errors  
After STEP 3: `MES.DAL` interfaces → 0 errors  
After STEP 4: `MES.BLL` interfaces → 0 errors  
After STEP 5: Full solution → 0 errors  
After STEP 6: Full solution → 0 errors  
