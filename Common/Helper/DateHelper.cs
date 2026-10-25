using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Helper
{
    public static class DateHelper
    {
        /// <summary>
        /// 获取当前时间时间戳
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetTimeStamp(this DateTime time)
        {
            TimeSpan ts = time.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        public static DateTime StampToDateTime(string time)
        {
            time = time.Substring(0, 10);
            double timestamp = Convert.ToInt64(time);
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateTime = dateTime.AddSeconds(timestamp).ToLocalTime();
            return dateTime;
        }

        public static string TimeSubTract(DateTime time1, DateTime time2)
        {
            TimeSpan subTract = time1.Subtract(time2);
            return $"{subTract.Days} 天 {subTract.Hours} 时 {subTract.Minutes} 分 ";
        }

        public static bool CheckMinHours(decimal? hour)
        {
            decimal? checkHours = hour / 0.5m;
            int outHours;
            int.TryParse(checkHours.ToString(), out outHours);
            if (checkHours < 1 || outHours == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取两个时间的间隔
        /// </summary>
        /// <param name="interval">间隔</param>
        /// <param name="dt1">时间1</param>
        /// <param name="dt2">时间2</param>
        /// <returns>
        /// 天，时，分，秒
        /// </returns>
        public static double DateDiff(string interval, DateTime dt1, DateTime dt2, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            var ts = dt2 - Convert.ToDateTime(dt1);
            string dateDiffResult;
            switch (interval.ToUpper())
            {
                case "Y":
                case "YEAR":                   
                    dateDiffResult = (dt2.Year - dt1.Year).ToString("N1");
                    break;
                case "MM":
                case "MONTH":
                    dateDiffResult = ((dt2.Year - dt1.Year) * 12 + (dt2.Month - dt1.Month)).ToString("N1");
                    break;
                case "WK":
                case "WEEK":
                    var weeks = ts.Days / 7;

                    #region [需要检查是否还有额外的一周要计算]
                    {
                        var remainingDays = ts.Days % 7;
                        var cal = CultureInfo.InvariantCulture.Calendar;
                        var d1WeekNo = cal.GetWeekOfYear(dt1, CalendarWeekRule.FirstFullWeek, startOfWeek);
                        var d1PlusRemainingWeekNo = cal.GetWeekOfYear(dt1.AddDays(remainingDays), CalendarWeekRule.FirstFullWeek, startOfWeek);
                        if (d1WeekNo != d1PlusRemainingWeekNo) weeks++;
                    }
                    #endregion

                    dateDiffResult = weeks.ToString();
                    break;
                case "D":
                case "DAY":
                    dateDiffResult = ts.TotalDays.ToString("N1");
                    break;
                case "H":
                case "HOUR":
                    dateDiffResult = ts.TotalHours.ToString("N1");
                    break;
                case "M":
                case "MINUTE":
                    dateDiffResult = ts.TotalMinutes.ToString("N1");
                    break;
                case "S":
                case "SECOND":
                default:
                    dateDiffResult = ts.TotalSeconds.ToString("N1");
                    break;
            }
            return Convert.ToDouble(dateDiffResult);
        }
    }
}
