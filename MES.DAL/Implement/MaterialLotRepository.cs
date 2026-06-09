using Dapper;
using MES.Common;
using MES.DAL.Interfaces;
using MES.Model.Material;
using System.Collections.Generic;

namespace MES.DAL.Implement
{
    public class MaterialLotRepository : IMaterialLotRepository
    {
        private const string LotColumns = @"LotNo, ItemCode, Quantity, LocationCode, Status, ReceiveTime, ExpiryDate, Remark, CreateTime, UpdateTime, CreateBy, IsActive";

        public MaterialLot GetById(string id) { using var conn = DbContext.GetConnection(); return conn.QueryFirstOrDefault<MaterialLot>($@"SELECT {LotColumns} FROM T_MaterialLot WHERE LotNo=@Id AND IsActive=1", new { Id = id }); }
        public List<MaterialLot> GetAll() { using var conn = DbContext.GetConnection(); return conn.Query<MaterialLot>($@"SELECT {LotColumns} FROM T_MaterialLot WHERE IsActive=1").AsList() ?? new List<MaterialLot>(); }
        public List<MaterialLot> GetByCondition(string whereClause, object parameters) { using var conn = DbContext.GetConnection(); return conn.Query<MaterialLot>($@"SELECT {LotColumns} FROM T_MaterialLot WHERE IsActive=1 AND ({whereClause})", parameters).AsList() ?? new List<MaterialLot>(); }
        public bool Insert(MaterialLot entity) { using var conn = DbContext.GetConnection(); return conn.Execute(@"INSERT INTO T_MaterialLot (LotNo, ItemCode, Quantity, LocationCode, Status, ReceiveTime, ExpiryDate, Remark, IsActive, CreateTime, UpdateTime, CreateBy) VALUES (@LotNo, @ItemCode, @Quantity, @LocationCode, @Status, @ReceiveTime, @ExpiryDate, @Remark, @IsActive, GETDATE(), GETDATE(), @CreateBy)", new { entity.LotNo, entity.ItemCode, entity.Quantity, entity.LocationCode, Status = (int)entity.Status, entity.ReceiveTime, entity.ExpiryDate, entity.Remark, entity.IsActive, entity.CreateBy }) > 0; }
        public bool Update(MaterialLot entity) { using var conn = DbContext.GetConnection(); return conn.Execute(@"UPDATE T_MaterialLot SET ItemCode=@ItemCode, Quantity=@Quantity, LocationCode=@LocationCode, Status=@Status, ReceiveTime=@ReceiveTime, ExpiryDate=@ExpiryDate, Remark=@Remark, UpdateTime=GETDATE() WHERE LotNo=@LotNo AND IsActive=1", new { entity.LotNo, entity.ItemCode, entity.Quantity, entity.LocationCode, Status = (int)entity.Status, entity.ReceiveTime, entity.ExpiryDate, entity.Remark }) > 0; }
        public bool Delete(string id) { using var conn = DbContext.GetConnection(); return conn.Execute("UPDATE T_MaterialLot SET IsActive=0, UpdateTime=GETDATE() WHERE LotNo=@Id", new { Id = id }) > 0; }
        public List<MaterialLot> GetByItemCode(string itemCode) { using var conn = DbContext.GetConnection(); return conn.Query<MaterialLot>($@"SELECT {LotColumns} FROM T_MaterialLot WHERE ItemCode=@ItemCode AND IsActive=1", new { ItemCode = itemCode }).AsList() ?? new List<MaterialLot>(); }
        public List<MaterialLot> GetByLocation(string locationCode) { using var conn = DbContext.GetConnection(); return conn.Query<MaterialLot>($@"SELECT {LotColumns} FROM T_MaterialLot WHERE LocationCode=@LocationCode AND IsActive=1", new { LocationCode = locationCode }).AsList() ?? new List<MaterialLot>(); }
        public bool UpdateQuantity(string lotNo, decimal newQuantity) { using var conn = DbContext.GetConnection(); return conn.Execute("UPDATE T_MaterialLot SET Quantity=@NewQuantity, UpdateTime=GETDATE() WHERE LotNo=@LotNo AND IsActive=1", new { LotNo = lotNo, NewQuantity = newQuantity }) > 0; }
        public bool UpdateStatus(string lotNo, LotStatus newStatus) { using var conn = DbContext.GetConnection(); return conn.Execute("UPDATE T_MaterialLot SET Status=@Status, UpdateTime=GETDATE() WHERE LotNo=@LotNo AND IsActive=1", new { LotNo = lotNo, Status = (int)newStatus }) > 0; }
        public decimal GetTotalStock(string itemCode) { using var conn = DbContext.GetConnection(); return conn.ExecuteScalar<decimal>("SELECT ISNULL(SUM(Quantity), 0) FROM T_MaterialLot WHERE ItemCode=@ItemCode AND Status=0 AND IsActive=1", new { ItemCode = itemCode }); }
        public bool InsertTransaction(MaterialTransaction transaction) { using var conn = DbContext.GetConnection(); return InsertTransactionInternal(conn, null, transaction) > 0; }

        public bool IssueMaterial(MaterialLot lot, string workOrderNo, decimal qty, string operatorId)
        {
            using var conn = DbContext.GetConnection(); conn.Open(); using var tx = conn.BeginTransaction();
            try
            {
                decimal newQty = lot.Quantity - qty;
                conn.Execute("UPDATE T_MaterialLot SET Quantity=@NewQuantity, UpdateTime=GETDATE() WHERE LotNo=@LotNo AND IsActive=1", new { lot.LotNo, NewQuantity = newQty }, tx);
                if (newQty == 0) conn.Execute("UPDATE T_MaterialLot SET Status=@Status, UpdateTime=GETDATE() WHERE LotNo=@LotNo AND IsActive=1", new { lot.LotNo, Status = (int)LotStatus.Consumed }, tx);
                var txn = new MaterialTransaction { TransactionNo = CodeGenerator.GenerateTransactionNo(), LotNo = lot.LotNo, ItemCode = lot.ItemCode, TransactionType = "ISSUE", Quantity = -qty, FromLocation = lot.LocationCode, ToLocation = null, WorkOrderNo = workOrderNo, OperatorId = operatorId, Remark = "Issue material" };
                InsertTransactionInternal(conn, tx, txn); tx.Commit(); return true;
            }
            catch { tx.Rollback(); throw; }
        }

        public bool TransferMaterial(MaterialLot lot, string targetLocation, string operatorId)
        {
            using var conn = DbContext.GetConnection(); conn.Open(); using var tx = conn.BeginTransaction();
            try
            {
                conn.Execute("UPDATE T_MaterialLot SET LocationCode=@TargetLocation, UpdateTime=GETDATE() WHERE LotNo=@LotNo AND IsActive=1", new { lot.LotNo, TargetLocation = targetLocation }, tx);
                var txn = new MaterialTransaction { TransactionNo = CodeGenerator.GenerateTransactionNo(), LotNo = lot.LotNo, ItemCode = lot.ItemCode, TransactionType = "TRANSFER", Quantity = 0m, FromLocation = lot.LocationCode, ToLocation = targetLocation, WorkOrderNo = null, OperatorId = operatorId, Remark = "Transfer material" };
                InsertTransactionInternal(conn, tx, txn); tx.Commit(); return true;
            }
            catch { tx.Rollback(); throw; }
        }

        private static int InsertTransactionInternal(System.Data.IDbConnection conn, System.Data.IDbTransaction tx, MaterialTransaction transaction)
            => conn.Execute(@"INSERT INTO T_MaterialTransaction (TransactionNo, LotNo, ItemCode, TransactionType, Quantity, FromLocation, ToLocation, WorkOrderNo, TransactionTime, OperatorId, Remark, IsActive, CreateTime, UpdateTime, CreateBy) VALUES (@TransactionNo, @LotNo, @ItemCode, @TransactionType, @Quantity, @FromLocation, @ToLocation, @WorkOrderNo, @TransactionTime, @OperatorId, @Remark, @IsActive, GETDATE(), GETDATE(), @CreateBy)", transaction, tx);
    }
}
