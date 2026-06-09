using System;

namespace MES.Model.Master
{
    public class RoutingOperation
    {
        public int OperationId { get; set; }
        public string RoutingCode { get; set; }
        public int OperationSeq { get; set; }
        public string OperationCode { get; set; }
        public string OperationName { get; set; }
        public string WorkCenterCode { get; set; }
        public decimal StdCycleTime { get; set; }
        public decimal SetupTime { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
