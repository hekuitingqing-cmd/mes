using MES.Common;
using MES.Model.Equipment;
using System;

namespace MES.BLL.Interfaces
{
    public interface IEquipmentService
    {
        ServiceResult UpdateEquipmentStatus(string equipmentCode, EquipmentStatus newStatus, string operatorId);
        ServiceResult CreateMaintenanceOrder(MaintenanceOrder mo);
        ServiceResult CompleteMaintenanceOrder(string maintOrderNo, string result, string technicianId);
        decimal CalculateOEE(string equipmentCode, DateTime start, DateTime end);
    }
}
