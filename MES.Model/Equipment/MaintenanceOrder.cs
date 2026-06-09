using System;

namespace MES.Model.Equipment
{
    public class MaintenanceOrder
    {
        public string MaintOrderNo { get; set; }
        public string EquipmentCode { get; set; }
        public string MaintType { get; set; }
        public string Description { get; set; }
        public DateTime PlanDate { get; set; }
        public DateTime? ActualDate { get; set; }
        public string MaintResult { get; set; }
        public int DowntimeMinutes { get; set; }
        public string TechnicianId { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
