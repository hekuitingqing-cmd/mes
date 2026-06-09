using MES.Common;
using MES.Model.Material;
using System.Collections.Generic;

namespace MES.BLL.Interfaces
{
    public interface IMaterialService
    {
        ServiceResult ReceiveMaterial(MaterialLot lot);
        ServiceResult IssueMaterial(string lotNo, string workOrderNo, decimal qty, string operatorId);
        ServiceResult TransferMaterial(string lotNo, string targetLocation, string operatorId);
        List<MaterialLot> QueryStock(string itemCode);
    }
}
