using System;

namespace MES.Model.Production
{
    public class OperationRecord
    {
        public string RecordNo { get; set; }
        public string WorkOrderNo { get; set; }
        public string OperationCode { get; set; }
        public string WorkCenterCode { get; set; }
        public decimal GoodQty { get; set; }
        public decimal ScrapQty { get; set; }
        public DateTime ReportTime { get; set; } = DateTime.Now;
        public string OperatorId { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
