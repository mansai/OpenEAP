using System;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace OpenEAP.Helper
{
    public class XmlConverter
    {
        /// <summary>
        /// 创建xml序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToXml<T>(T obj, ref bool bSuccess)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                serializer.Serialize(writer, obj);

                string xml = writer.ToString();
                writer.Close();
                writer.Dispose();
                bSuccess = true;
                return xml;
            }
            catch (Exception ex)
            {
                bSuccess = false;
                return ex.Message;
            }
        }

        /// <summary>
        /// xml反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T GetT<T>(string xml, ref bool bSuccess)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StreamReader reader = new StreamReader(xml);

                T res = (T)serializer.Deserialize(reader);
                reader.Close();
                reader.Dispose();
                bSuccess = true;
                return res;
            }
            catch (Exception)
            {
                bSuccess = false;
                return default;
            }
        }
    }
}