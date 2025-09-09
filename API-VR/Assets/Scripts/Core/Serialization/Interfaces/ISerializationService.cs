public interface ISerializationService
{
    string Serialize<T>(T obj) where T : ISerializationModel;
    T Deserialize<T>(string json) where T : ISerializationModel, new();
}