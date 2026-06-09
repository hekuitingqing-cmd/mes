using Dapper;
using MES.Common;
using MES.DAL.Interfaces;
using MES.Model.Quality;
using System.Collections.Generic;

namespace MES.DAL.Implement
{
    public class InspectionOrderRepository : IInspectionOrderRepository
    {
        private const string Columns = @"InspectionNo, InspectionType, WorkOrderNo, LotNo, ItemCode, SampleQty, NGQty, Result, InspectionTime, InspectorId, Remark, CreateTime, UpdateTime, CreateBy, IsActive";
        public InspectionOrder GetById(string id) { using var conn = DbContext.GetConnection(); return conn.QueryFirstOrDefault<InspectionOrder>($@"SELECT {Columns} FROM T_InspectionOrder WHERE InspectionNo=@Id AND IsActive=1", new { Id = id }); }
        public List<InspectionOrder> GetAll() { using var conn = DbContext.GetConnection(); return conn.Query<InspectionOrder>($@"SELECT {Columns} FROM T_InspectionOrder WHERE IsActive=1").AsList() ?? new List<InspectionOrder>(); }
        public List<InspectionOrder> GetByCondition(string whereClause, object parameters) { using var conn = DbContext.GetConnection(); return conn.Query<InspectionOrder>($@"SELECT {Columns} FROM T_InspectionOrder WHERE IsActive=1 AND ({whereClause})", parameters).AsList() ?? new List<InspectionOrder>(); }
        public bool Insert(InspectionOrder entity) { using var conn = DbContext.GetConnection(); return conn.Execute(@"INSERT INTO T_InspectionOrder (InspectionNo, InspectionType, WorkOrderNo, LotNo, ItemCode, SampleQty, NGQty, Result, InspectionTime, InspectorId, Remark, IsActive, CreateTime, UpdateTime, CreateBy) VALUES (@InspectionNo, @InspectionType, @WorkOrderNo, @LotNo, @ItemCode, @SampleQty, @NGQty, @Result, @InspectionTime, @InspectorId, @Remark, @IsActive, GETDATE(), GETDATE(), @CreateBy)", entity) > 0; }
        public bool Update(InspectionOrder entity) { using var conn = DbContext.GetConnection(); return conn.Execute(@"UPDATE T_InspectionOrder SET InspectionType=@InspectionType, WorkOrderNo=@WorkOrderNo, LotNo=@LotNo, ItemCode=@ItemCode, SampleQty=@SampleQty, NGQty=@NGQty, Result=@Result, InspectionTime=@InspectionTime, InspectorId=@InspectorId, Remark=@Remark, UpdateTime=GETDATE() WHERE InspectionNo=@InspectionNo AND IsActive=1", entity) > 0; }
        public bool Delete(string id) { using var conn = DbContext.GetConnection(); return conn.Execute("UPDATE T_InspectionOrder SET IsActive=0, UpdateTime=GETDATE() WHERE InspectionNo=@Id", new { Id = id }) > 0; }
        public List<InspectionOrder> GetByWorkOrder(string workOrderNo) { using var conn = DbContext.GetConnection(); return conn.Query<InspectionOrder>($@"SELECT {Columns} FROM T_InspectionOrder WHERE WorkOrderNo=@WorkOrderNo AND IsActive=1", new { WorkOrderNo = workOrderNo }).AsList() ?? new List<InspectionOrder>(); }
        public List<InspectionOrder> GetByType(string inspectionType) { using var conn = DbContext.GetConnection(); return conn.Query<InspectionOrder>($@"SELECT {Columns} FROM T_InspectionOrder WHERE InspectionType=@InspectionType AND IsActive=1", new { InspectionType = inspectionType }).AsList() ?? new List<InspectionOrder>(); }
        public List<InspectionOrder> GetPending() { using var conn = DbContext.GetConnection(); return conn.Query<InspectionOrder>($@"SELECT {Columns} FROM T_InspectionOrder WHERE Result=@Result AND IsActive=1", new { Result = "PENDING" }).AsList() ?? new List<InspectionOrder>(); }
        public bool UpdateResult(string inspectionNo, decimal ngQty, string result) { using var conn = DbContext.GetConnection(); return conn.Execute("UPDATE T_InspectionOrder SET NGQty=@NGQty, Result=@Result, InspectionTime=GETDATE(), UpdateTime=GETDATE() WHERE InspectionNo=@InspectionNo AND IsActive=1", new { InspectionNo = inspectionNo, NGQty = ngQty, Result = result }) > 0; }
        public bool InsertNCR(NCReport ncr) { using var conn = DbContext.GetConnection(); return conn.Execute(@"INSERT INTO T_NCReport (NCRNo, InspectionNo, DefectCode, NGQty, Disposition, RootCause, CorrectiveAction, Status, ClosedTime, ClosedBy, IsActive, CreateTime, UpdateTime, CreateBy) VALUES (@NCRNo, @InspectionNo, @DefectCode, @NGQty, @Disposition, @RootCause, @CorrectiveAction, @Status, @ClosedTime, @ClosedBy, @IsActive, GETDATE(), GETDATE(), @CreateBy)", ncr) > 0; }
    }
}
