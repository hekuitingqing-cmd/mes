using MES.BLL.Interfaces;
using MES.Common;
using MES.DAL.Interfaces;
using MES.Model.Equipment;
using System;
using System.Linq;

namespace MES.BLL.Implement
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IEquipmentRepository _equipmentRepo;
        public EquipmentService(IEquipmentRepository equipmentRepo) { _equipmentRepo = equipmentRepo; }

        public ServiceResult UpdateEquipmentStatus(string equipmentCode, EquipmentStatus newStatus, string operatorId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(equipmentCode)) return ServiceResult.Fail("设备编码不能为空");
                var eq = _equipmentRepo.GetById(equipmentCode); if (eq == null) return ServiceResult.Fail("设备不存在");
                bool ok = _equipmentRepo.UpdateStatus(equipmentCode, newStatus, operatorId); return ok ? ServiceResult.Ok() : ServiceResult.Fail("设备状态更新失败");
            }
            catch (Exception ex) { MESLogger.Error("Equipment", "UpdateEquipmentStatus", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public ServiceResult CreateMaintenanceOrder(MaintenanceOrder mo)
        {
            try
            {
                if (mo == null) return ServiceResult.Fail("维保工单不能为空");
                if (string.IsNullOrWhiteSpace(mo.EquipmentCode)) return ServiceResult.Fail("设备编码不能为空");
                if (string.IsNullOrWhiteSpace(mo.MaintType)) return ServiceResult.Fail("维保类型不能为空");
                mo.MaintOrderNo = CodeGenerator.GenerateMaintOrderNo(); mo.Status = 0;
                bool ok = _equipmentRepo.InsertMaintenanceOrder(mo); return ok ? ServiceResult.Ok(mo.MaintOrderNo) : ServiceResult.Fail("维保工单创建失败，请重试");
            }
            catch (Exception ex) { MESLogger.Error("Equipment", "CreateMaintenanceOrder", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public ServiceResult CompleteMaintenanceOrder(string maintOrderNo, string result, string technicianId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maintOrderNo)) return ServiceResult.Fail("维保工单号不能为空");
                if (string.IsNullOrWhiteSpace(result)) return ServiceResult.Fail("维保结果不能为空");
                if (string.IsNullOrWhiteSpace(technicianId)) return ServiceResult.Fail("维修技术员不能为空");
                bool ok = _equipmentRepo.CompleteMaintenanceOrder(maintOrderNo, result, technicianId); return ok ? ServiceResult.Ok() : ServiceResult.Fail("维保工单完成失败");
            }
            catch (Exception ex) { MESLogger.Error("Equipment", "CompleteMaintenanceOrder", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public decimal CalculateOEE(string equipmentCode, DateTime start, DateTime end)
        {
            try
            {
                var logs = _equipmentRepo.GetStatusLogs(equipmentCode, start, end);
                int runningMinutes = logs.Where(x => x.ToStatus == EquipmentStatus.Running).Sum(x => x.DurationMinutes);
                decimal totalMinutes = (decimal)(end - start).TotalMinutes;
                if (totalMinutes <= 0) return 0m;
                decimal oee = runningMinutes / totalMinutes;
                oee = Math.Clamp(oee, 0m, 1m);
                _equipmentRepo.UpdateOEE(equipmentCode, oee);
                return oee;
            }
            catch (Exception ex) { MESLogger.Error("Equipment", "CalculateOEE", ex); return 0m; }
        }
    }
}
