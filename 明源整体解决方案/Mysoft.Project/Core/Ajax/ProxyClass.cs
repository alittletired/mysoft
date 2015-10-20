using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Mysoft.Project.Core
{
    public static class JsonHelper
    {
        static BoolConverter boolConverter = new BoolConverter();
        static IntConverter intConverter = new IntConverter();
        public static string SerializeObject(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        public static object DeserializeObject(string val)
        {

            return Newtonsoft.Json.JsonConvert.DeserializeObject(val);
        }
        public static object DeserializeObject(string val,Type type)
        {

            return Newtonsoft.Json.JsonConvert.DeserializeObject(val, type, boolConverter, intConverter);
        }
        public static T DeserializeObject<T>(string val)
        {

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(val, boolConverter, intConverter);
        }
        public static string ToJson(this object obj) {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
    }

    public class IntConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var val = reader.Value;
            if (val == null) return null;
            if ((val.ToString().Equals("true", StringComparison.InvariantCultureIgnoreCase) || val.ToString().Equals("false", StringComparison.InvariantCultureIgnoreCase)))
            {
                val = val.ToString().Equals("true", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
            }
            return ConverterExtensions.ChangeType(val, objectType);
         
        }

        public override bool CanConvert(Type objectType)
        {
            
            if (objectType.IsPrimitive) return true;
           if( objectType.IsGenericType  && objectType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))){
                NullableConverter nullableConverter = new NullableConverter(objectType);
                if (nullableConverter.UnderlyingType.IsPrimitive)
                    return true;
           }
           return false;
            
          
        }
    }

    public class BoolConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value != null && ((bool)value) ? 1 : 0);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            return reader.Value !=null && reader.Value.ToString() == "1";
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }
    }
  
}
