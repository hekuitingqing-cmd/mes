using MES.BLL.Interfaces;
using MES.Common;
using MES.DAL.Interfaces;
using MES.Model.Material;
using System;
using System.Collections.Generic;

namespace MES.BLL.Implement
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialLotRepository _materialLotRepo;
        public MaterialService(IMaterialLotRepository materialLotRepo) { _materialLotRepo = materialLotRepo; }

        public ServiceResult ReceiveMaterial(MaterialLot lot)
        {
            try
            {
                if (lot == null) return ServiceResult.Fail("批次不能为空");
                if (string.IsNullOrWhiteSpace(lot.ItemCode)) return ServiceResult.Fail("物料编码不能为空");
                if (lot.Quantity <= 0) return ServiceResult.Fail("接收数量必须大于0");
                if (string.IsNullOrWhiteSpace(lot.LocationCode)) return ServiceResult.Fail("库位不能为空");
                lot.LotNo = CodeGenerator.GenerateLotNo(); lot.Status = LotStatus.Available;
                bool ok = _materialLotRepo.Insert(lot); return ok ? ServiceResult.Ok(lot.LotNo) : ServiceResult.Fail("收料失败，请重试");
            }
            catch (Exception ex) { MESLogger.Error("Material", "ReceiveMaterial", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public ServiceResult IssueMaterial(string lotNo, string workOrderNo, decimal qty, string operatorId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lotNo)) return ServiceResult.Fail("批次号不能为空");
                if (string.IsNullOrWhiteSpace(workOrderNo)) return ServiceResult.Fail("工单号不能为空");
                if (qty <= 0) return ServiceResult.Fail("发料数量必须大于0");
                var lot = _materialLotRepo.GetById(lotNo);
                if (lot == null) return ServiceResult.Fail("批次不存在");
                if (!lot.IsActive) return ServiceResult.Fail("批次不存在");
                if (lot.Status != LotStatus.Available) return ServiceResult.Fail("批次状态不可用");
                if (lot.Quantity < qty) return ServiceResult.Fail($"库存不足，当前库存: {lot.Quantity}");
                bool ok = _materialLotRepo.IssueMaterial(lot, workOrderNo, qty, operatorId); return ok ? ServiceResult.Ok() : ServiceResult.Fail("发料失败，请重试");
            }
            catch (Exception ex) { MESLogger.Error("Material", "IssueMaterial", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public ServiceResult TransferMaterial(string lotNo, string targetLocation, string operatorId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lotNo)) return ServiceResult.Fail("批次号不能为空");
                if (string.IsNullOrWhiteSpace(targetLocation)) return ServiceResult.Fail("目标库位不能为空");
                var lot = _materialLotRepo.GetById(lotNo); if (lot == null || !lot.IsActive) return ServiceResult.Fail("批次不存在");
                bool ok = _materialLotRepo.TransferMaterial(lot, targetLocation, operatorId); return ok ? ServiceResult.Ok() : ServiceResult.Fail("移库失败，请重试");
            }
            catch (Exception ex) { MESLogger.Error("Material", "TransferMaterial", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public List<MaterialLot> QueryStock(string itemCode)
        {
            try { return string.IsNullOrWhiteSpace(itemCode) ? (_materialLotRepo.GetAll() ?? new List<MaterialLot>()) : (_materialLotRepo.GetByItemCode(itemCode) ?? new List<MaterialLot>()); }
            catch (Exception ex) { MESLogger.Error("Material", "QueryStock", ex); return new List<MaterialLot>(); }
        }
    }
}
