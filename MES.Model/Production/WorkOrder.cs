using System;

namespace MES.Model.Production
{
    public class WorkOrder
    {
        public string WorkOrderNo { get; set; }
        public string ItemCode { get; set; }
        public string BOMCode { get; set; }
        public string RoutingCode { get; set; }
        public decimal PlanQty { get; set; }
        public decimal ActualQty { get; set; }
        public decimal ScrapQty { get; set; }
        public DateTime PlanStartTime { get; set; }
        public DateTime PlanEndTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Created;
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
