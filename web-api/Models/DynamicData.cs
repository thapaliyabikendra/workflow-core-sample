using Newtonsoft.Json.Linq;

namespace ACMS.WebApi.Models;

public class DynamicData 
{
    public Dictionary<string, object> Storage { get; set; } = new Dictionary<string, object>();

    public object this[string propertyName]
    {
        get => Storage.TryGetValue(propertyName, out var value) ? value : default;
        set => Storage[propertyName] = value;
    }

    public JObject ToJObject(string propertyName)
    {
        // Check if the property exists and if it's a JObject
        if (Storage.ContainsKey(propertyName) && Storage[propertyName] is JObject jObject)
        {
            return jObject;
        }

        // Return an empty JObject if the property is not found or cannot be converted to JObject
        return new JObject();
    }
}
