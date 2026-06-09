using MES.Common;
using MES.Model.Quality;
using System.Collections.Generic;

namespace MES.BLL.Interfaces
{
    public interface IQualityService
    {
        ServiceResult CreateInspectionOrder(InspectionOrder order);
        ServiceResult SubmitInspectionResult(string inspectionNo, decimal ngQty, string result);
        ServiceResult CreateNCR(NCReport ncr);
        List<InspectionOrder> GetPendingInspections();
    }
}
