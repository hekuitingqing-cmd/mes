using System;

namespace MES.Model.Quality
{
    public class NCReport
    {
        public string NCRNo { get; set; }
        public string InspectionNo { get; set; }
        public string DefectCode { get; set; }
        public decimal NGQty { get; set; }
        public string Disposition { get; set; }
        public string RootCause { get; set; }
        public string CorrectiveAction { get; set; }
        public int Status { get; set; }
        public DateTime? ClosedTime { get; set; }
        public string ClosedBy { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
