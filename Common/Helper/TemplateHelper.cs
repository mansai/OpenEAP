using Masuit.Tools.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static FreeSql.Internal.GlobalFilter;

namespace OpenEAP.Helper
{
    public class TemplateHelper
    {
        public static string Template(string sourceTemplate, Dictionary<string, object> model, bool disableContentEscaping = false)
        {
            //var sourceTemplate = "Dear {{name_man}}, {this} is definitely a personalized note to you. Very truly yours, {{sender}}";
            //var list = SubstringMultiple(sourceTemplate, "{{", "}}");
            var template = new Masuit.Tools.Strings.Template(sourceTemplate);// Mustachio.Parser.Parse(sourceTemplate, disableContentEscaping);
            // Combine the model with the template to get content:
            //Dictionary<string, object> dic = new Dictionary<string, object>();
            //foreach (var item in list)
            //{
            //    if (!dic.ContainsKey(item) && model[item]!=null)
            //    {
            //        dic.Add(item, model[item].ToString());
            //    }
            //}
            template.Set(model);
            // Dictionary<string, object> dic = new Dictionary<string, object> { { "name_man", "mansai" }, { "sender", "Sally" } };
            return template.Render(disableContentEscaping);//template(dic);
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
    }
}
