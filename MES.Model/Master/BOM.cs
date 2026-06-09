using System;

namespace MES.Model.Master
{
    public class BOM
    {
        public string BOMCode { get; set; }
        public string BOMName { get; set; }
        public string ParentItemCode { get; set; }
        public string BOMVersion { get; set; } = "1.0";
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
