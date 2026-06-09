using MES.BLL.Interfaces;
using MES.Common;
using MES.DAL.Interfaces;
using MES.Model.Production;
using System;
using System.Collections.Generic;

namespace MES.BLL.Implement
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly IWorkOrderRepository _workOrderRepo;
        public WorkOrderService(IWorkOrderRepository workOrderRepo) { _workOrderRepo = workOrderRepo; }

        public ServiceResult CreateWorkOrder(WorkOrder wo)
        {
            try
            {
                if (wo == null) return ServiceResult.Fail("工单不能为空");
                if (string.IsNullOrWhiteSpace(wo.ItemCode)) return ServiceResult.Fail("物料编码不能为空");
                if (wo.PlanQty <= 0) return ServiceResult.Fail("计划数量必须大于0");
                if (wo.PlanEndTime <= wo.PlanStartTime) return ServiceResult.Fail("计划结束时间必须晚于开始时间");
                wo.WorkOrderNo = CodeGenerator.GenerateWorkOrderNo(); wo.Status = WorkOrderStatus.Created;
                bool ok = _workOrderRepo.Insert(wo);
                if (!ok) return ServiceResult.Fail("工单创建失败，请重试");
                MESLogger.Info("Production", "CreateWorkOrder", $"Created {wo.WorkOrderNo}"); return ServiceResult.Ok(wo.WorkOrderNo);
            }
            catch (Exception ex) { MESLogger.Error("Production", "CreateWorkOrder", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public ServiceResult ReleaseWorkOrder(string workOrderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workOrderNo)) return ServiceResult.Fail("工单号不能为空");
                var wo = _workOrderRepo.GetById(workOrderNo); if (wo == null) return ServiceResult.Fail("工单不存在");
                if (wo.Status != WorkOrderStatus.Created) return ServiceResult.Fail($"工单状态为{wo.Status}，仅Created状态可下达");
                bool ok = _workOrderRepo.UpdateStatus(workOrderNo, WorkOrderStatus.Released); return ok ? ServiceResult.Ok() : ServiceResult.Fail("状态更新失败");
            }
            catch (Exception ex) { MESLogger.Error("Production", "ReleaseWorkOrder", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public ServiceResult ReportOperation(OperationRecord record)
        {
            try
            {
                if (record == null) return ServiceResult.Fail("报工记录不能为空");
                if (string.IsNullOrWhiteSpace(record.WorkOrderNo)) return ServiceResult.Fail("工单号不能为空");
                if (record.GoodQty < 0 || record.ScrapQty < 0) return ServiceResult.Fail("报工数量不能小于0");
                if (record.GoodQty + record.ScrapQty <= 0) return ServiceResult.Fail("报工数量必须大于0");
                var wo = _workOrderRepo.GetById(record.WorkOrderNo); if (wo == null) return ServiceResult.Fail("工单不存在");
                if (wo.Status == WorkOrderStatus.Closed || wo.Status == WorkOrderStatus.Cancelled) return ServiceResult.Fail("工单已关闭或取消，不可报工");
                if (wo.Status != WorkOrderStatus.Released && wo.Status != WorkOrderStatus.InProcess) return ServiceResult.Fail($"工单状态为{wo.Status}，不可报工");
                record.RecordNo = CodeGenerator.GenerateRecordNo();
                if (wo.Status == WorkOrderStatus.Released) { _workOrderRepo.SetActualStartTime(record.WorkOrderNo); _workOrderRepo.UpdateStatus(record.WorkOrderNo, WorkOrderStatus.InProcess); }
                _workOrderRepo.InsertOperationRecord(record); _workOrderRepo.UpdateActualQty(record.WorkOrderNo, record.GoodQty, record.ScrapQty);
                var updated = _workOrderRepo.GetById(record.WorkOrderNo); return ServiceResult.Ok(updated);
            }
            catch (Exception ex) { MESLogger.Error("Production", "ReportOperation", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public ServiceResult CloseWorkOrder(string workOrderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workOrderNo)) return ServiceResult.Fail("工单号不能为空");
                var wo = _workOrderRepo.GetById(workOrderNo); if (wo == null) return ServiceResult.Fail("工单不存在");
                if (wo.Status == WorkOrderStatus.Created) return ServiceResult.Fail("Created状态工单不可关闭");
                if (wo.Status != WorkOrderStatus.Released && wo.Status != WorkOrderStatus.InProcess) return ServiceResult.Fail($"工单状态为{wo.Status}，不可关闭");
                if (wo.ActualQty == 0) return ServiceResult.Fail("实际数量为0，不可关闭");
                bool ok = _workOrderRepo.Close(workOrderNo); return ok ? ServiceResult.Ok() : ServiceResult.Fail("工单关闭失败");
            }
            catch (Exception ex) { MESLogger.Error("Production", "CloseWorkOrder", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public WorkOrder GetWorkOrderDetail(string workOrderNo)
        {
            try { if (string.IsNullOrWhiteSpace(workOrderNo)) return new WorkOrder(); return _workOrderRepo.GetById(workOrderNo) ?? new WorkOrder(); }
            catch (Exception ex) { MESLogger.Error("Production", "GetWorkOrderDetail", ex); return new WorkOrder(); }
        }

        public List<WorkOrder> QueryWorkOrders(WorkOrderQueryParam param)
        {
            try
            {
                if (param == null) return _workOrderRepo.GetAll() ?? new List<WorkOrder>();
                if (param.Status.HasValue) return _workOrderRepo.GetByStatus(param.Status.Value) ?? new List<WorkOrder>();
                if (param.StartDate.HasValue && param.EndDate.HasValue) return _workOrderRepo.GetByDateRange(param.StartDate.Value, param.EndDate.Value) ?? new List<WorkOrder>();
                if (!string.IsNullOrWhiteSpace(param.ItemCode)) return _workOrderRepo.GetByCondition("ItemCode=@ItemCode", new { param.ItemCode }) ?? new List<WorkOrder>();
                return _workOrderRepo.GetAll() ?? new List<WorkOrder>();
            }
            catch (Exception ex) { MESLogger.Error("Production", "QueryWorkOrders", ex); return new List<WorkOrder>(); }
        }
    }
}
