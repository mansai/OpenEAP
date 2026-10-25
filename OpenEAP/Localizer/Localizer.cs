using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using OpenEAP.Helper;
using OpenEAP.Model;

namespace OpenEAP.Localizer
{
    public class Localizer : INotifyPropertyChanged
    {
        private const string IndexerName = "Item";
        private const string IndexerArrayName = "Item[]";
        private Dictionary<string, string> m_Strings = null;

        public bool LoadLanguage(string language)
        {
            Language = language;

            //Uri uri = new Uri($"avares://JC.EAP.Studio/Assets/i18n/{language}.json");
            //if (AssetLoader.Exists(uri))
            //{
            //    using (StreamReader sr = new StreamReader(AssetLoader.Open(uri), Encoding.UTF8))
            //    {
            //        m_Strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
            //    }
            //    Invalidate();

            //    return true;
            //}
            m_Strings = DbHelp.fsql.Select<ResourcesEntity>().Where(t => t.Culture == language).ToDictionary<string, string>(t => t.Key, t => t.Value);
            var option = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping// JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs)
            };
            var json = System.Text.Json.JsonSerializer.Serialize(m_Strings, option);
            if (m_Strings != null)
            {
                Invalidate();
                return true;
            }
            return false;
        }

        public string Language { get; private set; }

        public string this[string key]
        {
            get
            {
                if (m_Strings != null && m_Strings.TryGetValue(key, out string res))
                {
                    res = res ?? key;
                    return res.Replace("\\n", "\n");
                }
                //return $"{Language}:{key}";
#if DEBUG
                var checkedKey = DbHelp.fsql.Select<ResourcesEntity>().Any(t => t.Key == key);
                if (!checkedKey)
                {
                    var lans = DbHelp.fsql.Select<LocalizationEntity>().WhereDynamic(GlobalHelper.WhereEnable).ToList();
                    foreach (var item in lans)
                    {
                        ResourcesEntity resourcesEntity = new ResourcesEntity
                        {
                            Culture = item.Culture,
                            Key = key,
                            Value = key
                        };
                        DbHelp.fsql.Insert(resourcesEntity).ExecuteAffrows();
                    }
                }
#endif
                return key;
            }
        }

        public static Localizer Instance { get; set; } = new Localizer();
        public event PropertyChangedEventHandler PropertyChanged;

        public void Invalidate()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
        }
    }
}
