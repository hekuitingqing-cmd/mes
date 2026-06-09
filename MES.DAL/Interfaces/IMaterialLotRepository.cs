using MES.Model.Material;
using System.Collections.Generic;

namespace MES.DAL.Interfaces
{
    public interface IMaterialLotRepository : IRepository<MaterialLot>
    {
        List<MaterialLot> GetByItemCode(string itemCode);
        List<MaterialLot> GetByLocation(string locationCode);
        bool UpdateQuantity(string lotNo, decimal newQuantity);
        bool UpdateStatus(string lotNo, LotStatus newStatus);
        decimal GetTotalStock(string itemCode);
        bool InsertTransaction(MaterialTransaction transaction);
        bool IssueMaterial(MaterialLot lot, string workOrderNo, decimal qty, string operatorId);
        bool TransferMaterial(MaterialLot lot, string targetLocation, string operatorId);
    }
}
