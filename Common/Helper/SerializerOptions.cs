using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace OpenEAP.Helper
{
    public static class SerializerOptions
    {
        public static JsonSerializerOptions JsonSerializerOptions
        {
            get
            {
                var _jsonSerializerOptions = new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),//序列化所有语言集而不进行转义
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,//忽略循环引用
                    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                    PropertyNamingPolicy = null

                };
                _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                return _jsonSerializerOptions;
            }
        }
    }
}
