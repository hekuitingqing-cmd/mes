using Dapper;
using MES.Common;
using MES.Model.Production;
using System.Collections.Generic;

namespace MES.DAL.Implement
{
    public class OperationRecordRepository
    {
        public bool Insert(OperationRecord entity)
        {
            using var conn = DbContext.GetConnection();
            return conn.Execute(@"INSERT INTO T_OperationRecord (RecordNo, WorkOrderNo, OperationCode, WorkCenterCode, GoodQty, ScrapQty, ReportTime, OperatorId, Remark, IsActive, CreateTime, UpdateTime, CreateBy) VALUES (@RecordNo, @WorkOrderNo, @OperationCode, @WorkCenterCode, @GoodQty, @ScrapQty, @ReportTime, @OperatorId, @Remark, @IsActive, GETDATE(), GETDATE(), @CreateBy)", entity) > 0;
        }

        public List<OperationRecord> GetByWorkOrder(string workOrderNo)
        {
            using var conn = DbContext.GetConnection();
            return conn.Query<OperationRecord>(@"SELECT RecordNo, WorkOrderNo, OperationCode, WorkCenterCode, GoodQty, ScrapQty, ReportTime, OperatorId, Remark, CreateTime, UpdateTime, CreateBy, IsActive FROM T_OperationRecord WHERE WorkOrderNo=@WorkOrderNo AND IsActive=1", new { WorkOrderNo = workOrderNo }).AsList() ?? new List<OperationRecord>();
        }
    }
}
