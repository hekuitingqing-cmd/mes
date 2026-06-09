using System;

namespace MES.Model.Material
{
    public class MaterialTransaction
    {
        public string TransactionNo { get; set; }
        public string LotNo { get; set; }
        public string ItemCode { get; set; }
        public string TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public string WorkOrderNo { get; set; }
        public DateTime TransactionTime { get; set; } = DateTime.Now;
        public string OperatorId { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
