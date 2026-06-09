using MES.Model.Quality;
using System.Collections.Generic;

namespace MES.DAL.Interfaces
{
    public interface IInspectionOrderRepository : IRepository<InspectionOrder>
    {
        List<InspectionOrder> GetByWorkOrder(string workOrderNo);
        List<InspectionOrder> GetByType(string inspectionType);
        List<InspectionOrder> GetPending();
        bool UpdateResult(string inspectionNo, decimal ngQty, string result);
        bool InsertNCR(NCReport ncr);
    }
}
