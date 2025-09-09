using UnityEngine;
using System;

public class JsonSerializerService : ISerializationService
{
    public string Serialize<T>(T obj) where T : ISerializationModel => obj.ToJson();

    public T Deserialize<T>(string json) where T : ISerializationModel, new()
    {
        if (string.IsNullOrEmpty(json))
        throw new ArgumentException("JSON string cannot be null or empty.");

        var obj = new T();
        try 
        {
            obj.FromJson(json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON into {typeof(T).Name}", ex);
        }
        return obj;
    }
}