using System;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Linq;

public class CommunicationManager : MonoBehaviour
{
    private static CommunicationManager _instance;
    public static CommunicationManager Instance => _instance;

    [Header("Configuración")]
    public string publisherPort = "5555";
    public string subscriberPort = "5556";
    public string addressPrefix = "tcp://";
    public string[] availableTopics = new[] { "ARDrawing" };

    private PublisherSocket _publisher;
    private SubscriberSocket _subscriber;
    private Thread _subscriberThread;
    private bool _isRunning = false;
    private bool _isQuitting = false;

    // Diccionario para mantener los callbacks por topic
    private readonly Dictionary<string, Action<string>> _messageCallbacks = new Dictionary<string, Action<string>>();

    // Lista de IPs conocidas
    private List<string> _knownIPs = new List<string>();

    /// <summary>
    /// Indica si NetMQ fue inicializado correctamente y esta listo para enviar/recibir
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Evento que se dispara cuando NetMQ queda listo (IsRunning == true)
    /// </summary>
    public event Action OnReady;

    private readonly Queue<Action> _mainThreadActions = new Queue<Action>();
    private readonly object _queueLock = new object();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        InitializeNetMQ();
    }
    
    private void Update()
    {
        // Procesar todas las acciones pendientes en el hilo principal
        lock (_queueLock)
        {
            while (_mainThreadActions.Count > 0)
            {
                try
                {
                    var action = _mainThreadActions.Dequeue();
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[CommunicationManager] Error ejecutando acción en main thread: {e}");
                }
            }
        }
    }

    private void EnqueueOnMainThread(Action action)
    {
        lock (_queueLock)
        {
            _mainThreadActions.Enqueue(action);
        }
    }

    private void InitializeNetMQ()
    {
        try
        {
            AsyncIO.ForceDotNet.Force();
            Debug.Log($"[NetMQ] Inicializando - PUB:{publisherPort} SUB:{subscriberPort}");

            // Inicializar Publisher
            _publisher = new PublisherSocket();
            _publisher.Options.SendHighWatermark = 1000;
            _publisher.Bind($"{addressPrefix}*:{publisherPort}");
            Debug.Log($"[NetMQ] Publisher bind en {addressPrefix}*:{publisherPort}");

            // Inicializar Subscriber
            _subscriber = new SubscriberSocket();
            _subscriber.Options.ReceiveHighWatermark = 1000;

            // Suscribirse a todos los topics inicialmente (permitira recibir cuando haya conexiones)
            foreach (var topic in availableTopics)
            {
                _subscriber.Subscribe(topic);
                Debug.Log($"[NetMQ] Suscrito al topic: {topic}");
            }

            _isRunning = true;

            // Lanzar hilo receptor
            _subscriberThread = new Thread(ReceiveMessages) { IsBackground = true };
            _subscriberThread.Start();

            Debug.Log("[CommunicationManager] Comunicacion inicializada correctamente");
            OnReady?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[CommunicationManager] Error al inicializar: {e.Message}");
            Shutdown();
        }
    }

    /// <summary>
    /// Actualiza la lista de IPs remotas y conecta el subscriber a los publishers remotos.
    /// </summary>
    public void UpdateKnownIPs(List<string> ips)
    {
        Debug.Log($"[Network] IPs recibidas: {string.Join(", ", ips)}");
        if (!_isRunning) return;
        
        lock (_knownIPs)
        {
            var localIP = GetLocalIP();
            var newIPs = ips.Except(_knownIPs).Where(ip => ip != localIP && !string.IsNullOrEmpty(ip)).ToList();
            Debug.Log($"[Network] Nuevas IPs a conectar: {string.Join(", ", newIPs)}");

            foreach (var ip in newIPs)
            {
                try
                {
                    string remotePubAddress = $"{addressPrefix}{ip}:{publisherPort}";
                    Debug.Log($"[Network] Intentando conectar a: {remotePubAddress}");
                    _subscriber.Connect(remotePubAddress);
                    _knownIPs.Add(ip);
                    Debug.Log($"[NetMQ] Conexion exitosa a: {remotePubAddress}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Network] Error conectando a {ip}: {e.Message}");
                }
            }
        }        
    }

    /// <summary>
    /// Metodo de diagnostico que envia periodicamente un mensaje de prueba y se subscribe a ARDrawing.
    /// Util para pruebas manuales.
    /// </summary>
    public void TestPublishSubscribe()
    {
        // Enviar mensaje de prueba cada 5 segundos
        InvokeRepeating(nameof(SendTestMessage), 2f, 5f);

        // Verificar recepcion
        SubscribeToTopic("ARDrawing", (msg) => {
            Debug.Log($"[TEST] Mensaje recibido: {msg}");
        });
    }

    public void SendTestMessage()
    {
        if (!_isRunning || _publisher == null) 
        {
            Debug.LogWarning("[CommunicationManager] SendTestMessage ignorado porque NetMQ no está listo.");
            return;
        }

        try
        {
            string testMsg = $"TEST|{DateTime.Now.Ticks}|{GetLocalIP()}";
            PublishMessage("ARDrawing", testMsg);
            Debug.Log($"[CommunicationManager] Mensaje de prueba enviado: {testMsg}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CommunicationManager] Error al enviar mensaje de prueba: {e.Message}");
        }
    }

    private string GetLocalIP()
    {
        try
        {
            var multicastType = Type.GetType("MulticastDiscovery");
            var multicast = FindObjectOfType(multicastType);
            if (multicast != null)
            {
                var field = multicastType.GetField("_localIP");
                if (field != null)
                {
                    var value = field.GetValue(multicast) as string;
                    return value ?? "";
                }
            }
        }
        catch { }
        // fallback simple
        return "";
    }

    public void PublishMessage(string topic, string message)
    {
        if (!_isRunning || _publisher == null)
        {
            Debug.LogWarning("[CommunicationManager] Intento de publicar antes de que NetMQ esté listo.");
            return;
        }

        try
        {
            _publisher.SendMoreFrame(topic).SendFrame(message);
            Debug.Log($"[CommunicationManager] Mensaje publicado -> Tópico: {topic}, Mensaje: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CommunicationManager] Error al publicar mensaje: {e.Message}");
        }
    }

    public void SubscribeToTopic(string topic, Action<string> callback)
    {
        if (!_isRunning || _subscriber == null)
        {
            Debug.LogWarning("[CommunicationManager] Suscripción ignorada porque NetMQ no está listo.");
            return;
        }

        try
        {
            if (!_messageCallbacks.ContainsKey(topic))
            {
                _messageCallbacks[topic] = null;
                _subscriber.Subscribe(topic);
            }
            _messageCallbacks[topic] += callback;
            Debug.Log($"[CommunicationManager] Callback añadido para tópico {topic}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CommunicationManager] Error al suscribirse al topic: {e.Message}");
        }
    }

    public void UnsubscribeFromTopic(string topic, Action<string> callback)
    {
        if (_subscriber == null) return;

        try
        {
            if (_messageCallbacks.ContainsKey(topic))
            {
                _messageCallbacks[topic] -= callback;
                if (_messageCallbacks[topic] == null)
                {
                    _messageCallbacks.Remove(topic);
                    try { _subscriber.Unsubscribe(topic); } catch { }
                }
            }
            Debug.Log($"[CommunicationManager] Callback removido para tópico {topic}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CommunicationManager] Error al desuscribirse del topic: {e.Message}");
        }
    }

    private void ReceiveMessages()
    {
        Debug.Log("[NetMQ] Hilo de recepción iniciado");
        while (_isRunning)
        {
            try
            {
                string topic = _subscriber.ReceiveFrameString();
                string message = _subscriber.ReceiveFrameString();

                Debug.Log($"[NetMQ-Raw] Topic: {topic} | Message: {message}");

                // Encolar la llamada al callback para que se ejecute en el hilo principal
                EnqueueOnMainThread(() =>
                {
                    if (_messageCallbacks.TryGetValue(topic, out Action<string> callback))
                    {
                        callback?.Invoke(message);
                    }
                });
            }
            catch (Exception e)
            {
                if (_isRunning)
                    Debug.LogError($"[NetMQ] Error en recepción: {e.Message}");
            }
        }
        Debug.Log("[NetMQ] Hilo de recepción finalizado");
    }

    private void OnDestroy()
    {
        if (!_isQuitting) // Solo si no es por cierre de aplicación
        {
            ForceCleanup();
        }
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
        ForceCleanup();
    }

    private void ForceCleanup()
    {
        _isRunning = false;

        // Intentar desconectar endpoints conocidos
        try
        {
            foreach (var ip in _knownIPs)
            {
                try { _subscriber?.Disconnect($"{addressPrefix}{ip}:{publisherPort}"); } catch { }
            }
            
            try { _publisher?.Close(); } catch { }
            try { _subscriber?.Close(); } catch { }

            NetMQConfig.Cleanup(false);
            Debug.Log("[NetMQ] ForceCleanup ejecutado");
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetMQ] Error en ForceCleanup: {e.Message}");
        }
    }

    public void Shutdown()
    {
        _isRunning = false;

        // Forzar cierre de sockets
        try
        {
            try { _publisher?.Close(); } catch { }
            try { _subscriber?.Close(); } catch { }
            NetMQConfig.Cleanup(block: true);
            Debug.Log("[NetMQ] Sockets y hilos cerrados correctamente");
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetMQ] Error en Shutdown: {e.Message}");
        }
    }
}