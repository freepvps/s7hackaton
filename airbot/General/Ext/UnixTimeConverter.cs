using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace airbot.General.Ext
{
    public class UnixTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        /// <summary>
        /// Преобразует DateTime в Unix таймстемп.
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <returns></returns>
        public static long ToUnixTime(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        /// <summary>
        /// Преобразует Unix таймстемп в DateTime.
        /// </summary>
        /// <param name="timeStamp">Таймстемп</param>
        /// <returns></returns>
        public static DateTime ToDateTime(long timeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(timeStamp);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var t = long.Parse(reader.Value.ToString());
            return ToDateTime(t);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ToUnixTime((DateTime)value));
        }
    }
}
