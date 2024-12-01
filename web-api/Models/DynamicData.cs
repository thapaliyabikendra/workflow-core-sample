namespace ACMS.WebApi.Models;

public class DynamicData
{
    public Dictionary<string, object> Storage { get; set; } = new Dictionary<string, object>();

    public object this[string propertyName]
    {
        get => Storage.TryGetValue(propertyName, out var value) ? value : null;
        set => Storage[propertyName] = value;
    }
}
