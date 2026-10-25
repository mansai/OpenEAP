namespace OpenEAP.Helper
{
    public class GlobalHelper
    {

        /// <summary>
        /// 当前用户id
        /// </summary>
        public static long UserId = 1;
        public static string BaseUrl = string.Empty;
        public static string UserName = string.Empty;
        public static string PassWord = string.Empty;
        public static long LocaltionId;
        public static long LineId;
        public static long EquipmentId;
        public static long ProcessesId;
        public static string Language;
        public static string DateFormatEn = "dd/MM/yyyy HH:mm:ss";
        public static string DateFormatCn = "yyyy-MM-dd HH:mm:ss";
        public static ushort VID = 0;
        public static bool RunSequential = true;//立库顺序运行
       /// <summary>
       /// 对接表
       /// </summary>
       public static byte[] Excel;
        /// <summary>
        /// PLC扫描周期
        /// </summary>
        public static int CycleTime = 500;
        /// <summary>
        /// 有效
        /// </summary>
        public static string Y = "Y";

        /// <summary>
        /// 无效
        /// </summary>
        public static string N = "N";

        public static object WhereEnable = new { Enable = Y };

        public static object WhereDisable = new { Enable = N };

        /// <summary>
        /// 字符串颠倒
        /// </summary>
        public static bool StringReverse = false;

        /// <summary>
        /// 高低位颠倒
        /// </summary>
        public static bool BitReverse = false;
        /// <summary>
        /// 自动启动
        /// </summary>
        public static bool IsRun = false;
    }
}
