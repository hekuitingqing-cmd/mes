using System;

namespace MES.Model.Quality
{
    public class InspectionOrder
    {
        public string InspectionNo { get; set; }
        public string InspectionType { get; set; }
        public string WorkOrderNo { get; set; }
        public string LotNo { get; set; }
        public string ItemCode { get; set; }
        public decimal SampleQty { get; set; }
        public decimal NGQty { get; set; }
        public string Result { get; set; } = "PENDING";
        public DateTime? InspectionTime { get; set; }
        public string InspectorId { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
