using Newtonsoft.Json;
using PixelCrushers;

namespace Project.Runtime.Scripts.SaveSystem
{
    public class JsonNetDataSerializer : DataSerializer
    {
        public override string Serialize(object data)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            return JsonConvert.SerializeObject(data, settings);
        }

        public override T Deserialize<T>(string s, T data = default(T))
        {
            return JsonConvert.DeserializeObject<T>(s);
        }
    }
}