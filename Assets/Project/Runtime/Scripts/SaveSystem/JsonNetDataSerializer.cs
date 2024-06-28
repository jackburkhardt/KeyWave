using Newtonsoft.Json;
using PixelCrushers;

namespace Project.Runtime.Scripts.SaveSystem
{
    public class JsonNetDataSerializer : DataSerializer
    {
        public override string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public override T Deserialize<T>(string s, T data = default(T))
        {
            return JsonConvert.DeserializeObject<T>(s);
        }
    }
}