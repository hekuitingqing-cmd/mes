using System;

namespace MES.Model.Equipment
{
    public class Equipment
    {
        public string EquipmentCode { get; set; }
        public string EquipmentName { get; set; }
        public string EquipmentType { get; set; }
        public string WorkCenterCode { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? LastMaintDate { get; set; }
        public DateTime? NextMaintDate { get; set; }
        public EquipmentStatus Status { get; set; } = EquipmentStatus.Idle;
        public decimal OEE { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
