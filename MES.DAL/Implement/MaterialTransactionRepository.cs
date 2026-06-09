using Dapper;
using MES.Common;
using MES.Model.Material;
using System.Collections.Generic;

namespace MES.DAL.Implement
{
    public class MaterialTransactionRepository
    {
        public bool Insert(MaterialTransaction entity)
        {
            using var conn = DbContext.GetConnection();
            return conn.Execute(@"INSERT INTO T_MaterialTransaction (TransactionNo, LotNo, ItemCode, TransactionType, Quantity, FromLocation, ToLocation, WorkOrderNo, TransactionTime, OperatorId, Remark, IsActive, CreateTime, UpdateTime, CreateBy) VALUES (@TransactionNo, @LotNo, @ItemCode, @TransactionType, @Quantity, @FromLocation, @ToLocation, @WorkOrderNo, @TransactionTime, @OperatorId, @Remark, @IsActive, GETDATE(), GETDATE(), @CreateBy)", entity) > 0;
        }

        public List<MaterialTransaction> GetByLot(string lotNo)
        {
            using var conn = DbContext.GetConnection();
            return conn.Query<MaterialTransaction>(@"SELECT TransactionNo, LotNo, ItemCode, TransactionType, Quantity, FromLocation, ToLocation, WorkOrderNo, TransactionTime, OperatorId, Remark, CreateTime, UpdateTime, CreateBy, IsActive FROM T_MaterialTransaction WHERE LotNo=@LotNo AND IsActive=1", new { LotNo = lotNo }).AsList() ?? new List<MaterialTransaction>();
        }
    }
}
