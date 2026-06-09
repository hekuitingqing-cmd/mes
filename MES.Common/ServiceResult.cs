namespace MES.Common
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public static ServiceResult Ok(object data = null)
            => new ServiceResult { Success = true, Data = data };

        public static ServiceResult Fail(string message)
            => new ServiceResult { Success = false, Message = message };
    }
}
