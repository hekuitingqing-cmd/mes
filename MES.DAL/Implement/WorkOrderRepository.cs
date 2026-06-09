using Dapper;
using MES.Common;
using MES.DAL.Interfaces;
using MES.Model.Production;
using System;
using System.Collections.Generic;

namespace MES.DAL.Implement
{
    public class WorkOrderRepository : IWorkOrderRepository
    {
        private const string Columns = @"WorkOrderNo, ItemCode, BOMCode, RoutingCode, PlanQty, ActualQty, ScrapQty, PlanStartTime, PlanEndTime, ActualStartTime, ActualEndTime, Status, Remark, CreateTime, UpdateTime, CreateBy, IsActive";

        public WorkOrder GetById(string id)
        {
            using var conn = DbContext.GetConnection();
            return conn.QueryFirstOrDefault<WorkOrder>($@"SELECT {Columns} FROM T_WorkOrder WHERE WorkOrderNo = @Id AND IsActive = 1", new { Id = id });
        }

        public List<WorkOrder> GetAll()
        {
            using var conn = DbContext.GetConnection();
            return conn.Query<WorkOrder>($@"SELECT {Columns} FROM T_WorkOrder WHERE IsActive = 1").AsList() ?? new List<WorkOrder>();
        }

        public List<WorkOrder> GetByCondition(string whereClause, object parameters)
        {
            using var conn = DbContext.GetConnection();
            return conn.Query<WorkOrder>($@"SELECT {Columns} FROM T_WorkOrder WHERE IsActive = 1 AND ({whereClause})", parameters).AsList() ?? new List<WorkOrder>();
        }

        public bool Insert(WorkOrder entity)
        {
            using var conn = DbContext.GetConnection();
            return conn.Execute(@"INSERT INTO T_WorkOrder (WorkOrderNo, ItemCode, BOMCode, RoutingCode, PlanQty, ActualQty, ScrapQty, PlanStartTime, PlanEndTime, ActualStartTime, ActualEndTime, Status, Remark, IsActive, CreateTime, UpdateTime, CreateBy)
VALUES (@WorkOrderNo, @ItemCode, @BOMCode, @RoutingCode, @PlanQty, @ActualQty, @ScrapQty, @PlanStartTime, @PlanEndTime, @ActualStartTime, @ActualEndTime, @Status, @Remark, @IsActive, GETDATE(), GETDATE(), @CreateBy)",
                new { entity.WorkOrderNo, entity.ItemCode, entity.BOMCode, entity.RoutingCode, entity.PlanQty, entity.ActualQty, entity.ScrapQty, entity.PlanStartTime, entity.PlanEndTime, entity.ActualStartTime, entity.ActualEndTime, Status = (int)entity.Status, entity.Remark, entity.IsActive, entity.CreateBy }) > 0;
        }

        public bool Update(WorkOrder entity)
        {
            using var conn = DbContext.GetConnection();
            return conn.Execute(@"UPDATE T_WorkOrder SET ItemCode=@ItemCode, BOMCode=@BOMCode, RoutingCode=@RoutingCode, PlanQty=@PlanQty, ActualQty=@ActualQty, ScrapQty=@ScrapQty, PlanStartTime=@PlanStartTime, PlanEndTime=@PlanEndTime, ActualStartTime=@ActualStartTime, ActualEndTime=@ActualEndTime, Status=@Status, Remark=@Remark, UpdateTime=GETDATE() WHERE WorkOrderNo=@WorkOrderNo AND IsActive=1",
                new { entity.WorkOrderNo, entity.ItemCode, entity.BOMCode, entity.RoutingCode, entity.PlanQty, entity.ActualQty, entity.ScrapQty, entity.PlanStartTime, entity.PlanEndTime, entity.ActualStartTime, entity.ActualEndTime, Status = (int)entity.Status, entity.Remark }) > 0;
        }

        public bool Delete(string id)
        {
            using var conn = DbContext.GetConnection();
            return conn.Execute("UPDATE T_WorkOrder SET IsActive=0, UpdateTime=GETDATE() WHERE WorkOrderNo=@Id", new { Id = id }) > 0;
        }

        public List<WorkOrder> GetByStatus(WorkOrderStatus status)
        {
            using var conn = DbContext.GetConnection();
            return conn.Query<WorkOrder>($@"SELECT {Columns} FROM T_WorkOrder WHERE Status=@Status AND IsActive=1", new { Status = (int)status }).AsList() ?? new List<WorkOrder>();
        }

        public List<WorkOrder> GetByDateRange(DateTime start, DateTime end)
        {
            using var conn = DbContext.GetConnection();
            return conn.Query<WorkOrder>($@"SELECT {Columns} FROM T_WorkOrder WHERE PlanStartTime>=@Start AND PlanEndTime<=@End AND IsActive=1", new { Start = start, End = end }).AsList() ?? new List<WorkOrder>();
        }

        public bool UpdateStatus(string workOrderNo, WorkOrderStatus newStatus)
        {
            using var conn = DbContext.GetConnection();
            return conn.Execute("UPDATE T_WorkOrder SET Status=@Status, UpdateTime=GETDATE() WHERE WorkOrderNo=@WorkOrderNo AND IsActive=1", new { Status = (int)newStatus, WorkOrderNo = workOrderNo }) > 0;
        }

        public bool UpdateActualQty(string workOrderNo, decimal goodQty, decimal scrapQty)
        {
            using var conn = DbContext.GetConnection();
            return conn.Execute(@"UPDATE T_WorkOrder SET ActualQty=ActualQty+@GoodQty, ScrapQty=ScrapQty+@ScrapQty, UpdateTime=GETDATE() WHERE WorkOrderNo=@WorkOrderNo AND IsActive=1", new { WorkOrderNo = workOrderNo, GoodQty = goodQty, ScrapQty = scrapQty }) > 0;
        }

        public bool InsertOperationRecord(OperationRecord record)
        {
            using var conn = DbContext.GetConnection();
            return conn.Execute(@"INSERT INTO T_OperationRecord (RecordNo, WorkOrderNo, OperationCode, WorkCenterCode, GoodQty, ScrapQty, ReportTime, OperatorId, Remark, IsActive, CreateTime, UpdateTime, CreateBy)
VALUES (@RecordNo, @WorkOrderNo, @OperationCode, @WorkCenterCode, @GoodQty, @ScrapQty, @ReportTime, @OperatorId, @Remark, @IsActive, GETDATE(), GETDATE(), @CreateBy)", record) > 0;
        }

        public bool SetActualStartTime(string workOrderNo)
        {
            using var conn = DbContext.GetConnection();
            return conn.Execute("UPDATE T_WorkOrder SET ActualStartTime=GETDATE(), UpdateTime=GETDATE() WHERE WorkOrderNo=@WorkOrderNo AND ActualStartTime IS NULL AND IsActive=1", new { WorkOrderNo = workOrderNo }) > 0;
        }

        public bool Close(string workOrderNo)
        {
            using var conn = DbContext.GetConnection();
            return conn.Execute("UPDATE T_WorkOrder SET Status=@Status, ActualEndTime=GETDATE(), UpdateTime=GETDATE() WHERE WorkOrderNo=@WorkOrderNo AND IsActive=1", new { Status = (int)WorkOrderStatus.Closed, WorkOrderNo = workOrderNo }) > 0;
        }
    }
}
