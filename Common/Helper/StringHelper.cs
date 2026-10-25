using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Masuit.Tools.Strings;
using Flee.PublicTypes;
using OpenEAP.Extensions;
using Serilog;

namespace OpenEAP.Helper
{
    public class StringHelper
    {
        /// <summary>
        /// 根据分隔符返回前n条数据
        /// </summary>
        /// <param name="content">数据内容</param>
        /// <param name="separator">分隔符</param>
        /// <param name="top">前n条</param>
        /// <param name="isDesc">是否倒序（默认false）</param>
        /// <returns></returns>
        public static List<string> GetTopDataBySeparator(string content, string separator, int top, bool isDesc = false)
        {
            if (string.IsNullOrEmpty(content))
            {
                return new List<string>() { };
            }

            if (string.IsNullOrEmpty(separator))
            {
                throw new ArgumentException("message", nameof(separator));
            }

            var dataArray = content.Split(separator).Where(d => !string.IsNullOrEmpty(d)).ToArray();
            if (isDesc)
            {
                Array.Reverse(dataArray);
            }

            if (top > 0)
            {
                dataArray = dataArray.Take(top).ToArray();
            }

            return dataArray.ToList();
        }

        /// <summary>
        /// 删除字符串中的中文
        /// </summary>
        public static string DeleteChinese(string str)
        {
            string retValue = str;
            if (System.Text.RegularExpressions.Regex.IsMatch(str, @"[\u4e00-\u9fa5]"))
            {
                retValue = string.Empty;
                var strsStrings = str.ToCharArray();
                for (int index = 0; index < strsStrings.Length; index++)
                {
                    if (strsStrings[index] >= 0x4e00 && strsStrings[index] <= 0x9fa5)
                    {
                        continue;
                    }
                    retValue += strsStrings[index];
                }
            }
            return retValue;
        }

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="length">随机数长度</param>
        /// <returns>数字+字母</returns>
        public static string StrRandom(int length)
        {
            char[] constant =
            {
                '0','1','2','3','4','5','6','7','8','9',
                'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
            };

            List<string> result = new List<string>();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                result.Add(constant[random.Next(62)].ToString());
            }

            return string.Join("", result);
        }

        /// <summary>
        /// 模板替换
        /// </summary>
        /// <param name="sourceTemplate"></param>
        /// <param name="dic"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        public static string Template(string sourceTemplate, Dictionary<string, string> dic, bool check = false)
        {
            //var sourceTemplate = "Dear {{name_man}}, {this} is definitely a personalized note to you. Very truly yours, {{sender}}";
            var list = SubstringMultiple(sourceTemplate, "{{", "}}");
            var template = new Template(sourceTemplate);
            dic.Add("DateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dic.Add("Date", DateTime.Now.ToString("yyyy-MM-dd"));
            dic.Add("Time", DateTime.Now.ToString("HH:mm:ss"));
            foreach (var item in list)
            {
                if (dic.ContainsKey(item))
                {
                    template.Set(item, dic[item]);
                }
            }
            // Dictionary<string, object> dic = new Dictionary<string, object> { { "name_man", "mansai" }, { "sender", "Sally" } };
            return template.Render(check);
        }

        /// <summary>
        /// 模板替换
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="sourceTemplate"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        public static string Template(Dictionary<string, string> dic, string sourceTemplate, bool check = false)
        {
            //var sourceTemplate = "Dear {{name_man}}, {this} is definitely a personalized note to you. Very truly yours, {{sender}}";            
            var template = new Template(sourceTemplate);
            foreach (var item in dic)
            {
                template.Set(item.Key, item.Value);
            }
            return template.Render(check);
        }

        /// <summary>
        /// 截取字符串中开始和结束字符串中间的字符串
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="startStr">开始字符串</param>
        /// <param name="endStr">结束字符串</param>
        /// <returns>中间字符串</returns>
        public static string SubstringSingle(string source, string startStr, string endStr)
        {
            Regex rg = new Regex("(?<=(" + startStr + "))[.\\s\\S]*?(?=(" + endStr + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(source).Value;
        }

        /// <summary>
        /// （批量）截取字符串中开始和结束字符串中间的字符串
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="startStr">开始字符串</param>
        /// <param name="endStr">结束字符串</param>
        /// <returns>中间字符串</returns>
        public static List<string> SubstringMultiple(string source, string startStr, string endStr)
        {
            Regex rg = new Regex("(?<=(" + startStr + "))[.\\s\\S]*?(?=([" + endStr + "]))", RegexOptions.Multiline | RegexOptions.Singleline);

            MatchCollection matches = rg.Matches(source);

            List<string> resList = new List<string>();

            foreach (Match item in matches)
                resList.Add(item.Value);

            return resList;
        }

        /// <summary>
        /// 获取逻辑值
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public static bool LogicValue(string template)
        {
            if (string.IsNullOrEmpty(template))
            {
                return false;
            }
            else
            {
                object templateResult = Compute(template);
                return (bool)templateResult;
            }
        }
        static ExpressionContext expressionContext;
        public static ExpressionContext BuildExpressionContext()
        {
            if (expressionContext==null)
            {
                expressionContext = new(new CustomExpressionOwner());//定义表达式的上下文
                expressionContext.Options.OwnerMemberAccess = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;//允许私有成员访问
                expressionContext.Imports.AddType(typeof(Math));//允许表达式使用System.Math的所有静态公共方法
                expressionContext.Imports.AddType(typeof(Convert));//允许表达式使用System.Convert的所有静态公共方法
                expressionContext.Imports.AddType(typeof(Regex));//允许表达式使用System.Text.RegularExpressions.Regex的所有静态公共方法
                expressionContext.Imports.AddType(typeof(CustomExpressionFunction));//JC.AI.Common.Extensions.CustomExpressionFunction的所有自定义静态公共方法
            }
            return expressionContext;
        }
        /// <summary>
        /// 获取逻辑值
        /// </summary>
        /// <param name="formulaString">公式字符串</param>
        /// <returns></returns>
        public static object Compute(string formulaString)
        {
            try
            {
                if (expressionContext==null)
                {
                    BuildExpressionContext();
                }
                formulaString = formulaString.Replace("!=", "<>").Replace("==", "=").Replace("&&", "and").Replace("||", "or");
                IDynamicExpression dynamicExpression = expressionContext.CompileDynamic(formulaString);//创建计算结果为Object的动态表达式，也可以通过CompileGeneric指定具体的类型进行创建
                var result = dynamicExpression.Evaluate();//计算表达式的值
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// 表达式计算
        /// </summary>
        /// <param name="template"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static object Compute(string template, string filter = null)
        {
            try
            {
                object computeResult = new System.Data.DataTable().Compute(template, filter);
                return computeResult;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 表达式计算
        /// </summary>
        /// <param name="table"></param>
        /// <param name="template"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static object TableCompute(System.Data.DataTable table, string template, string filter = null)
        {
            object computeResult = table.Compute(template, filter);
            return computeResult;
        }

        /// <summary>
        /// BCC和校验代码
        /// </summary>
        /// <param name="data">需要校验的数据包</param>
        /// <returns></returns>
        public static byte Get_CheckXor(byte[] data)
        {
            byte CheckCode = 0;
            int len = data.Length;
            for (int i = 0; i < len; i++)
            {
                CheckCode ^= data[i];
            }
            return CheckCode;
        }

        /// <summary>
        /// 字节数组转16进制
        /// </summary>
        /// <param name="comByte"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string ByteToHex(byte[] comByte, int len)
        {
            string returnStr = "";
            if (comByte != null)
            {
                for (int i = 0; i < len; i++)
                {
                    returnStr += comByte[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }

        /// <summary>
        /// 字符转16进制
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");

            byte[] comBuffer = new byte[msg.Length / 2];
            for (int i = 0; i < msg.Length; i += 2)
            {
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            }

            return comBuffer;
        }
        public static byte[] WordByte(string msg,int maxLen)
        {
            byte[] data = new byte[2 + maxLen];   // 2 字节头 + 最大字符区
            data[0] = (byte)maxLen;               // MaxLen
            data[1] = (byte)msg.Length;             // 当前长度
            Array.Copy(Encoding.ASCII.GetBytes(msg), 0, data, 2, msg.Length);
            return data;
        }
        /// <summary>
        /// 高低位转换
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] HexByte(byte[] buffer)
        {
            byte[] temp = new byte[buffer.Length];
            temp[buffer.Length - 1] = buffer[buffer.Length - 1];
            for (int i = 0; i < buffer.Length / 2; i++)
            {
                temp[i * 2] = buffer[i * 2 + 1];
                temp[i * 2 + 1] = buffer[i * 2];
            }
            return temp;
        }
    }
}