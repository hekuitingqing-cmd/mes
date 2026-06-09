using System;

namespace MES.Model.Master
{
    public class BOMDetail
    {
        public int BOMDetailId { get; set; }
        public string BOMCode { get; set; }
        public string ChildItemCode { get; set; }
        public decimal Quantity { get; set; }
        public string UOM { get; set; }
        public decimal ScrapRate { get; set; }
        public int Seq { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
