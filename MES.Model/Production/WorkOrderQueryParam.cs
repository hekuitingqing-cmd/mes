using System;

namespace MES.Model.Production
{
    public class WorkOrderQueryParam
    {
        public WorkOrderStatus? Status { get; set; }
        public string ItemCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
