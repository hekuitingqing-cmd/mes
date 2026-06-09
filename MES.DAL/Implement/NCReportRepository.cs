using Dapper;
using MES.Common;
using MES.Model.Quality;

namespace MES.DAL.Implement
{
    public class NCReportRepository
    {
        public bool Insert(NCReport entity)
        {
            using var conn = DbContext.GetConnection();
            return conn.Execute(@"INSERT INTO T_NCReport (NCRNo, InspectionNo, DefectCode, NGQty, Disposition, RootCause, CorrectiveAction, Status, ClosedTime, ClosedBy, IsActive, CreateTime, UpdateTime, CreateBy) VALUES (@NCRNo, @InspectionNo, @DefectCode, @NGQty, @Disposition, @RootCause, @CorrectiveAction, @Status, @ClosedTime, @ClosedBy, @IsActive, GETDATE(), GETDATE(), @CreateBy)", entity) > 0;
        }
    }
}
