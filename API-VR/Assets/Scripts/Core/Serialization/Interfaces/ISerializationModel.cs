public interface ISerializationModel
{
    string ToJson();
    void FromJson(string json);
}