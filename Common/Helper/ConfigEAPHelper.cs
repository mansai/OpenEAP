using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Helper
{
    public static class ConfigEAPHelper
    {
        /// <summary>
        /// 报警代码计时,
        /// </summary>
        public static DateTime Alarm;
        public static DateTime AlarmHistory;        
        /// <summary>
        /// 秒
        /// </summary>
        public static int AlarmTime = 2;
        //机台运行时间
        public static int RunStatus = 0;//机台状态码
        public static int RunCount;//运行秒
        public static int DownCount;//停机秒
        public static int ReadyCount;//待机秒
        public static DateTime RunTime;//辅助计算
        public static string IpWindowName;//
        public static void GetCount(int status)
        {
            if (status != RunStatus)
            {

                RunStatus = status;
                var run = DateTime.Now.Subtract(RunTime).Seconds;
                RunTime = DateTime.Now;
                switch (status)
                {
                    //ready
                    case 0:
                        ReadyCount += run;
                        break;
                    //down
                    case 1:
                        DownCount += run;
                        break;
                    //run:
                    case 2:
                        RunCount += run;
                        break;
                    default: break;
                }
            }
        }
        public static void RestCount(int status)
        {
            RunStatus = 0;//机台状态码
            RunCount = 0;//运行秒
            DownCount = 0;//停机秒
            ReadyCount = 0;//待机秒
            DateTime RunTime = DateTime.Now;//辅助计算
        }
    }
}
