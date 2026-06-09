using MES.Model.Equipment;
using System;
using System.Collections.Generic;

namespace MES.DAL.Interfaces
{
    public interface IEquipmentRepository : IRepository<Equipment>
    {
        List<Equipment> GetByStatus(EquipmentStatus status);
        bool UpdateStatus(string equipmentCode, EquipmentStatus status);
        bool UpdateStatus(string equipmentCode, EquipmentStatus status, string operatorId);
        bool UpdateOEE(string equipmentCode, decimal oee);
        List<MaintenanceOrder> GetMaintenanceOrders(string equipmentCode);
        List<EquipmentStatusLog> GetStatusLogs(string equipmentCode, DateTime start, DateTime end);
        bool InsertMaintenanceOrder(MaintenanceOrder order);
        bool CompleteMaintenanceOrder(string maintOrderNo, string result, string technicianId);
    }
}
