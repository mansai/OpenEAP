using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Extensions
{
    /// <summary>
    /// 自定义函数
    /// </summary>
    public class CustomExpressionFunction
    {
        /// <summary>
        /// 可变参数求和
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static double Sum(params double[] args)
        {
            double res = 0;
            foreach (var item in args)
            {
                res += item;
            }
            return res;
        }

        public static float StDev(params float[] arrData) //计算标准偏差
        {
            float xSum = 0F;
            float xAvg = 0F;
            float sSum = 0F;
            float tmpStDev = 0F;
            int arrNum = arrData.Length;
            for (int i = 0; i < arrNum; i++)
            {
                xSum += arrData[i];
            }
            xAvg = xSum / arrNum;
            for (int j = 0; j < arrNum; j++)
            {
                sSum += ((arrData[j] - xAvg) * (arrData[j] - xAvg));
            }
            tmpStDev = Convert.ToSingle(Math.Sqrt((sSum / (arrNum - 1))).ToString());
            return tmpStDev;
        }

        public static double StDev(params double[] arrData) //计算标准偏差
        {
            double xSum = 0;
            double xAvg = 0;
            double sSum = 0;
            double tmpStDev = 0;
            int arrNum = arrData.Length;
            for (int i = 0; i < arrNum; i++)
            {
                xSum += arrData[i];
            }
            xAvg = xSum / arrNum;
            for (int j = 0; j < arrNum; j++)
            {
                sSum += ((arrData[j] - xAvg) * (arrData[j] - xAvg));
            }
            tmpStDev = Convert.ToSingle(Math.Sqrt((sSum / (arrNum - 1))).ToString());
            return tmpStDev;
        }

        public static double Average(params double[] arrData)
        {
            var xSum = Sum(arrData);
            return xSum / arrData.Length;
        }        
    }
}
