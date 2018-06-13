using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace AFBus
{
    public class JSONSerializer : ISerializeMessages
    {
        public object Deserialize(string input) 
        {
            var deserialized = JsonConvert.DeserializeObject(input, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
                                
            });

            return deserialized;
        }

        public object Deserialize(string input, Type type)
        {
            var deserialized = JsonConvert.DeserializeObject(input, type);

            return deserialized;
        }

        public string Serialize<T>(T input)
        {
            return JsonConvert.SerializeObject(input, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
            });
        }
    }
}
