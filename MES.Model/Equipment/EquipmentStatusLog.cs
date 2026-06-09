using System;

namespace MES.Model.Equipment
{
    public class EquipmentStatusLog
    {
        public int LogId { get; set; }
        public string EquipmentCode { get; set; }
        public EquipmentStatus FromStatus { get; set; }
        public EquipmentStatus ToStatus { get; set; }
        public DateTime ChangeTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Operator { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
    }
}
