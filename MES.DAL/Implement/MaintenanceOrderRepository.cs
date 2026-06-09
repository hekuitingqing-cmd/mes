using Dapper;
using MES.Common;
using MES.Model.Equipment;

namespace MES.DAL.Implement
{
    public class MaintenanceOrderRepository
    {
        public bool Insert(MaintenanceOrder entity)
        {
            using var conn = DbContext.GetConnection();
            return conn.Execute(@"INSERT INTO T_MaintenanceOrder (MaintOrderNo, EquipmentCode, MaintType, Description, PlanDate, ActualDate, MaintResult, DowntimeMinutes, TechnicianId, Status, IsActive, CreateTime, UpdateTime, CreateBy) VALUES (@MaintOrderNo, @EquipmentCode, @MaintType, @Description, @PlanDate, @ActualDate, @MaintResult, @DowntimeMinutes, @TechnicianId, @Status, @IsActive, GETDATE(), GETDATE(), @CreateBy)", entity) > 0;
        }
    }
}
