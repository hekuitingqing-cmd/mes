using MES.Common;
using MES.Model.Production;
using System.Collections.Generic;

namespace MES.BLL.Interfaces
{
    public interface IWorkOrderService
    {
        ServiceResult CreateWorkOrder(WorkOrder wo);
        ServiceResult ReleaseWorkOrder(string workOrderNo);
        ServiceResult ReportOperation(OperationRecord record);
        ServiceResult CloseWorkOrder(string workOrderNo);
        WorkOrder GetWorkOrderDetail(string workOrderNo);
        List<WorkOrder> QueryWorkOrders(WorkOrderQueryParam param);
    }
}
