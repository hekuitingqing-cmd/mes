using System;

namespace MES.Model.Master
{
    public class WorkCenter
    {
        public string WorkCenterCode { get; set; }
        public string WorkCenterName { get; set; }
        public string WorkCenterType { get; set; }
        public string Description { get; set; }
        public decimal Capacity { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
