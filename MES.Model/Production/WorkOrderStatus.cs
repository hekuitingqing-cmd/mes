namespace MES.Model.Production
{
    public enum WorkOrderStatus
    {
        Created = 0,
        Released = 1,
        InProcess = 2,
        Completed = 3,
        Closed = 4,
        Cancelled = 9
    }
}
