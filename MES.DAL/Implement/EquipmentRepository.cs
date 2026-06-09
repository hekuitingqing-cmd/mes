using Dapper;
using MES.Common;
using MES.DAL.Interfaces;
using MES.Model.Equipment;
using System;
using System.Collections.Generic;

namespace MES.DAL.Implement
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private const string Columns = @"EquipmentCode, EquipmentName, EquipmentType, WorkCenterCode, Model, Manufacturer, PurchaseDate, LastMaintDate, NextMaintDate, Status, OEE, CreateTime, UpdateTime, CreateBy, IsActive";
        public Equipment GetById(string id) { using var conn = DbContext.GetConnection(); return conn.QueryFirstOrDefault<Equipment>($@"SELECT {Columns} FROM T_Equipment WHERE EquipmentCode=@Id AND IsActive=1", new { Id = id }); }
        public List<Equipment> GetAll() { using var conn = DbContext.GetConnection(); return conn.Query<Equipment>($@"SELECT {Columns} FROM T_Equipment WHERE IsActive=1").AsList() ?? new List<Equipment>(); }
        public List<Equipment> GetByCondition(string whereClause, object parameters) { using var conn = DbContext.GetConnection(); return conn.Query<Equipment>($@"SELECT {Columns} FROM T_Equipment WHERE IsActive=1 AND ({whereClause})", parameters).AsList() ?? new List<Equipment>(); }
        public bool Insert(Equipment entity) { using var conn = DbContext.GetConnection(); return conn.Execute(@"INSERT INTO T_Equipment (EquipmentCode, EquipmentName, EquipmentType, WorkCenterCode, Model, Manufacturer, PurchaseDate, LastMaintDate, NextMaintDate, Status, OEE, IsActive, CreateTime, UpdateTime, CreateBy) VALUES (@EquipmentCode, @EquipmentName, @EquipmentType, @WorkCenterCode, @Model, @Manufacturer, @PurchaseDate, @LastMaintDate, @NextMaintDate, @Status, @OEE, @IsActive, GETDATE(), GETDATE(), @CreateBy)", new { entity.EquipmentCode, entity.EquipmentName, entity.EquipmentType, entity.WorkCenterCode, entity.Model, entity.Manufacturer, entity.PurchaseDate, entity.LastMaintDate, entity.NextMaintDate, Status = (int)entity.Status, entity.OEE, entity.IsActive, entity.CreateBy }) > 0; }
        public bool Update(Equipment entity) { using var conn = DbContext.GetConnection(); return conn.Execute(@"UPDATE T_Equipment SET EquipmentName=@EquipmentName, EquipmentType=@EquipmentType, WorkCenterCode=@WorkCenterCode, Model=@Model, Manufacturer=@Manufacturer, PurchaseDate=@PurchaseDate, LastMaintDate=@LastMaintDate, NextMaintDate=@NextMaintDate, Status=@Status, OEE=@OEE, UpdateTime=GETDATE() WHERE EquipmentCode=@EquipmentCode AND IsActive=1", new { entity.EquipmentCode, entity.EquipmentName, entity.EquipmentType, entity.WorkCenterCode, entity.Model, entity.Manufacturer, entity.PurchaseDate, entity.LastMaintDate, entity.NextMaintDate, Status = (int)entity.Status, entity.OEE }) > 0; }
        public bool Delete(string id) { using var conn = DbContext.GetConnection(); return conn.Execute("UPDATE T_Equipment SET IsActive=0, UpdateTime=GETDATE() WHERE EquipmentCode=@Id", new { Id = id }) > 0; }
        public List<Equipment> GetByStatus(EquipmentStatus status) { using var conn = DbContext.GetConnection(); return conn.Query<Equipment>($@"SELECT {Columns} FROM T_Equipment WHERE Status=@Status AND IsActive=1", new { Status = (int)status }).AsList() ?? new List<Equipment>(); }
        public bool UpdateStatus(string equipmentCode, EquipmentStatus status) => UpdateStatus(equipmentCode, status, "SYSTEM");
        public bool UpdateStatus(string equipmentCode, EquipmentStatus status, string operatorId)
        {
            using var conn = DbContext.GetConnection(); conn.Open(); using var tx = conn.BeginTransaction();
            try
            {
                int fromStatus = conn.QueryFirstOrDefault<int>("SELECT Status FROM T_Equipment WHERE EquipmentCode=@EquipmentCode AND IsActive=1", new { EquipmentCode = equipmentCode }, tx);
                conn.Execute("UPDATE T_Equipment SET Status=@NewStatus, UpdateTime=GETDATE() WHERE EquipmentCode=@EquipmentCode AND IsActive=1", new { EquipmentCode = equipmentCode, NewStatus = (int)status }, tx);
                conn.Execute(@"INSERT INTO T_EquipmentStatusLog (EquipmentCode, FromStatus, ToStatus, ChangeTime, DurationMinutes, Operator, IsActive, CreateTime, UpdateTime, CreateBy) VALUES (@EquipmentCode, @FromStatus, @NewStatus, GETDATE(), 0, @Operator, 1, GETDATE(), GETDATE(), 'SYSTEM')", new { EquipmentCode = equipmentCode, FromStatus = fromStatus, NewStatus = (int)status, Operator = operatorId }, tx);
                tx.Commit(); return true;
            }
            catch { tx.Rollback(); throw; }
        }
        public bool UpdateOEE(string equipmentCode, decimal oee) { using var conn = DbContext.GetConnection(); return conn.Execute("UPDATE T_Equipment SET OEE=@OEE, UpdateTime=GETDATE() WHERE EquipmentCode=@EquipmentCode AND IsActive=1", new { EquipmentCode = equipmentCode, OEE = oee }) > 0; }
        public List<MaintenanceOrder> GetMaintenanceOrders(string equipmentCode) { using var conn = DbContext.GetConnection(); return conn.Query<MaintenanceOrder>(@"SELECT MaintOrderNo, EquipmentCode, MaintType, Description, PlanDate, ActualDate, MaintResult, DowntimeMinutes, TechnicianId, Status, CreateTime, UpdateTime, CreateBy, IsActive FROM T_MaintenanceOrder WHERE EquipmentCode=@EquipmentCode AND IsActive=1", new { EquipmentCode = equipmentCode }).AsList() ?? new List<MaintenanceOrder>(); }
        public List<EquipmentStatusLog> GetStatusLogs(string equipmentCode, DateTime start, DateTime end) { using var conn = DbContext.GetConnection(); return conn.Query<EquipmentStatusLog>(@"SELECT LogId, EquipmentCode, FromStatus, ToStatus, ChangeTime, DurationMinutes, Operator, IsActive, CreateTime, UpdateTime, CreateBy FROM T_EquipmentStatusLog WHERE EquipmentCode=@EquipmentCode AND ChangeTime>=@Start AND ChangeTime<=@End", new { EquipmentCode = equipmentCode, Start = start, End = end }).AsList() ?? new List<EquipmentStatusLog>(); }
        public bool InsertMaintenanceOrder(MaintenanceOrder order) { using var conn = DbContext.GetConnection(); return conn.Execute(@"INSERT INTO T_MaintenanceOrder (MaintOrderNo, EquipmentCode, MaintType, Description, PlanDate, ActualDate, MaintResult, DowntimeMinutes, TechnicianId, Status, IsActive, CreateTime, UpdateTime, CreateBy) VALUES (@MaintOrderNo, @EquipmentCode, @MaintType, @Description, @PlanDate, @ActualDate, @MaintResult, @DowntimeMinutes, @TechnicianId, @Status, @IsActive, GETDATE(), GETDATE(), @CreateBy)", order) > 0; }
        public bool CompleteMaintenanceOrder(string maintOrderNo, string result, string technicianId) { using var conn = DbContext.GetConnection(); return conn.Execute("UPDATE T_MaintenanceOrder SET MaintResult=@Result, TechnicianId=@TechnicianId, ActualDate=GETDATE(), Status=2, UpdateTime=GETDATE() WHERE MaintOrderNo=@MaintOrderNo AND IsActive=1", new { MaintOrderNo = maintOrderNo, Result = result, TechnicianId = technicianId }) > 0; }
    }
}
