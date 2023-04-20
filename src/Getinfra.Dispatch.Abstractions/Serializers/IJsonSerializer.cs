namespace Getinfra.Dispatch.Abstractions.Serializers
{
    public interface IJsonSerializer
    {
        string Serialize(object value);

        T Deserialize<T>(string value);

        T Deserialize<T>(byte[] data);
    }
}
