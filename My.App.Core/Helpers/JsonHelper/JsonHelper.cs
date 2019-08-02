using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace My.App.Core
{
    public class JsonHelper
    {
        private static readonly JsonConverter[] JavaScriptConverters = new JsonConverter[1] { new DateTimeConverter() };

        public static string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data, JsonHelper.JavaScriptConverters);
        }

        public static string Serialize(object data, bool ignoreNull)
        {
            if (ignoreNull)
                return JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            return Serialize(data);
        }

        public static object Deserialize(string json, Type targetType)
        {
            return JsonConvert.DeserializeObject(json, targetType);
        }

        public static T Deserialize<T>(string json)
        {
            return !string.IsNullOrWhiteSpace(json) ? JsonConvert.DeserializeObject<T>(json) : default(T);
        }
    }
}
