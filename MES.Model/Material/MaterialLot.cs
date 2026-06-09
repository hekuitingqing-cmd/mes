using System;

namespace MES.Model.Material
{
    public class MaterialLot
    {
        public string LotNo { get; set; }
        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public string LocationCode { get; set; }
        public LotStatus Status { get; set; } = LotStatus.Available;
        public DateTime ReceiveTime { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
