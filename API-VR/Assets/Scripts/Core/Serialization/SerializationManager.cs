using UnityEngine;
using System.Collections.Generic;

public class SerializationManager : MonoBehaviour
{
    public static SerializationManager Instance { get; private set; }

    private Dictionary<System.Type, ISerializationService> _serializers = 
        new Dictionary<System.Type, ISerializationService>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeServices();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeServices()
    {
        RegisterService<DrawingModel>(new JsonSerializerService());
        // Registrar otros modelos aquí: RegisterService<OtroModelo>(...)
    }

    public void RegisterService<T>(ISerializationService service) where T : ISerializationModel
    {
        _serializers[typeof(T)] = service;
    }

    public string Serialize<T>(T model) where T : ISerializationModel
    {
        if (_serializers.TryGetValue(typeof(T), out var service))
            return service.Serialize(model);
        throw new System.NotSupportedException($"No serializer registered for type {typeof(T).Name}");
    }

    public T Deserialize<T>(string json) where T : ISerializationModel, new()
    {
        if (_serializers.TryGetValue(typeof(T), out var service))
            return service.Deserialize<T>(json);
        throw new System.NotSupportedException($"No deserializer registered for type {typeof(T).Name}");
    }
}