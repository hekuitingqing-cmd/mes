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
        bool InsertOperationRecord(OperationRecord record);
        bool SetActualStartTime(string workOrderNo);
        bool Close(string workOrderNo);
    }
}
