using System;

namespace MES.Model.Master
{
    public class Routing
    {
        public string RoutingCode { get; set; }
        public string RoutingName { get; set; }
        public string ItemCode { get; set; }
        public string RoutingVersion { get; set; } = "1.0";
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = "SYSTEM";
        public bool IsActive { get; set; } = true;
    }
}
