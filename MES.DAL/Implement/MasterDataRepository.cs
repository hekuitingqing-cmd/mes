using Dapper;
using MES.Common;
using MES.Model.Master;
using System.Collections.Generic;

namespace MES.DAL.Implement
{
    public class MasterDataRepository
    {
        public List<Item> GetItems()
        {
            using var conn = DbContext.GetConnection();
            return conn.Query<Item>("SELECT ItemCode, ItemName, ItemType, UOM, Spec, SafetyStock, CreateTime, UpdateTime, CreateBy, IsActive FROM T_Item WHERE IsActive=1").AsList() ?? new List<Item>();
        }

        public List<WorkCenter> GetWorkCenters()
        {
            using var conn = DbContext.GetConnection();
            return conn.Query<WorkCenter>("SELECT WorkCenterCode, WorkCenterName, WorkCenterType, Description, Capacity, CreateTime, UpdateTime, CreateBy, IsActive FROM T_WorkCenter WHERE IsActive=1").AsList() ?? new List<WorkCenter>();
        }
    }
}
