using System;

namespace MES.Common
{
    public static class CodeGenerator
    {
        private static readonly object _lock = new object();
        private static int _seqCounter = 0;
        private static string _lastDate = "";

        public static string Generate(string prefix)
        {
            lock (_lock)
            {
                string today = DateTime.Now.ToString("yyyyMMdd");
                if (today != _lastDate)
                {
                    _seqCounter = 0;
                    _lastDate = today;
                }

                _seqCounter++;
                return $"{prefix}-{today}-{_seqCounter:D3}";
            }
        }

        public static string GenerateWorkOrderNo() => Generate("WO");
        public static string GenerateLotNo() => Generate("LOT");
        public static string GenerateInspectionNo() => Generate("INS");
        public static string GenerateNCRNo() => Generate("NCR");
        public static string GenerateMaintOrderNo() => Generate("MO");
        public static string GenerateTransactionNo() => Generate("TXN");
        public static string GenerateRecordNo() => Generate("OPR");
    }
}
