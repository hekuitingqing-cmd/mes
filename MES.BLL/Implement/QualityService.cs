using MES.BLL.Interfaces;
using MES.Common;
using MES.DAL.Interfaces;
using MES.Model.Quality;
using System;
using System.Collections.Generic;

namespace MES.BLL.Implement
{
    public class QualityService : IQualityService
    {
        private readonly IInspectionOrderRepository _inspectionRepo;
        public QualityService(IInspectionOrderRepository inspectionRepo) { _inspectionRepo = inspectionRepo; }

        public ServiceResult CreateInspectionOrder(InspectionOrder order)
        {
            try
            {
                if (order == null) return ServiceResult.Fail("检验单不能为空");
                if (string.IsNullOrWhiteSpace(order.InspectionType)) return ServiceResult.Fail("检验类型不能为空");
                if (string.IsNullOrWhiteSpace(order.ItemCode)) return ServiceResult.Fail("物料编码不能为空");
                if (order.SampleQty <= 0) return ServiceResult.Fail("抽样数量必须大于0");
                order.InspectionNo = CodeGenerator.GenerateInspectionNo(); order.Result = "PENDING";
                bool ok = _inspectionRepo.Insert(order); return ok ? ServiceResult.Ok(order.InspectionNo) : ServiceResult.Fail("检验单创建失败，请重试");
            }
            catch (Exception ex) { MESLogger.Error("Quality", "CreateInspectionOrder", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public ServiceResult SubmitInspectionResult(string inspectionNo, decimal ngQty, string result)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(inspectionNo)) return ServiceResult.Fail("检验单号不能为空");
                if (ngQty < 0) return ServiceResult.Fail("不合格数不能小于0");
                if (string.IsNullOrWhiteSpace(result)) return ServiceResult.Fail("检验结果不能为空");
                var order = _inspectionRepo.GetById(inspectionNo); if (order == null) return ServiceResult.Fail("检验单不存在");
                if (order.Result != "PENDING") return ServiceResult.Fail("检验单已提交，不可重复提交");
                if (ngQty > order.SampleQty) return ServiceResult.Fail("不合格数不能超过抽样数");
                bool ok = _inspectionRepo.UpdateResult(inspectionNo, ngQty, result); if (!ok) return ServiceResult.Fail("检验结果提交失败");
                if (result == "FAIL")
                {
                    var ncr = new NCReport { NCRNo = CodeGenerator.GenerateNCRNo(), InspectionNo = inspectionNo, DefectCode = "UNKNOWN", NGQty = ngQty, Disposition = "SCRAP", Status = 0 };
                    _inspectionRepo.InsertNCR(ncr);
                }
                return ServiceResult.Ok();
            }
            catch (Exception ex) { MESLogger.Error("Quality", "SubmitInspectionResult", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public ServiceResult CreateNCR(NCReport ncr)
        {
            try
            {
                if (ncr == null) return ServiceResult.Fail("NCR不能为空");
                if (string.IsNullOrWhiteSpace(ncr.InspectionNo)) return ServiceResult.Fail("检验单号不能为空");
                if (ncr.NGQty <= 0) return ServiceResult.Fail("不合格数量必须大于0");
                ncr.NCRNo = CodeGenerator.GenerateNCRNo(); bool ok = _inspectionRepo.InsertNCR(ncr); return ok ? ServiceResult.Ok(ncr.NCRNo) : ServiceResult.Fail("NCR创建失败，请重试");
            }
            catch (Exception ex) { MESLogger.Error("Quality", "CreateNCR", ex); return ServiceResult.Fail("系统错误，请联系管理员"); }
        }

        public List<InspectionOrder> GetPendingInspections()
        {
            try { return _inspectionRepo.GetPending() ?? new List<InspectionOrder>(); }
            catch (Exception ex) { MESLogger.Error("Quality", "GetPendingInspections", ex); return new List<InspectionOrder>(); }
        }
    }
}
