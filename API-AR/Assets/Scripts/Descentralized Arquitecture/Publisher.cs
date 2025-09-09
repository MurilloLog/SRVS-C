using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

public class Publisher : MonoBehaviour
{
    [Tooltip("Topics disponibles para publicación")]
    public string[] allowableTopics = new[] { "TopicA", "TopicB", "All" };

    [Tooltip("Dirección y puerto para binding (ej: tcp://192.168.0.97:5555)")]
    public string connectionString = "tcp://192.168.0.97:5555";

    [Tooltip("Tópico seleccionado para publicación")]
    public string selectedTopic = "TopicA";

    [Tooltip("Intervalo en ms para envío automático")]
    public int autoSendInterval = 50;

    [Tooltip("Valor máximo para envío automático")]
    public int maxAutoValue = 100;

    private PublisherSocket pubSocket;
    private Thread autoSendThread;
    private bool isRunning = false;
    private bool autoSending = false;

    [Header("Configuración Dinámica")]
    [Tooltip("Referencia al script MulticastDiscovery")]
    public MulticastDiscovery discoveryService;

    [Tooltip("Prefijo para la dirección (ej: tcp://)")]
    public string addressPrefix = "tcp://";

    [Tooltip("Puerto para conexión")]
    public string port = "5555";
    private List<string> currentEndpoints = new List<string>();

    void Start()
    {
        InitializePublisher();
    }

    void InitializePublisher()
    {
        try
        {
            AsyncIO.ForceDotNet.Force();
            pubSocket = new PublisherSocket();
            pubSocket.Options.SendHighWatermark = 1000;
            pubSocket.Bind(connectionString);
            isRunning = true;

            Debug.Log($"Publisher iniciado en {connectionString} para tópico: {selectedTopic}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al iniciar publisher: {e.Message}");
        }
    }

    // Método para enviar mensajes desde otros scripts
    public new void SendMessage(string message)
    {
        if (!isRunning || pubSocket == null) return;

        string topic = selectedTopic == "All" ? "" : selectedTopic;

        try
        {
            pubSocket.SendMoreFrame(topic).SendFrame(message);
            Debug.Log($"Mensaje enviado -> Tópico: {selectedTopic}, Mensaje: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al enviar mensaje: {e.Message}");
            Reconnect();
        }
    }

    // Método para iniciar envío automático
    public void StartAutoSend()
    {
        if (autoSending) return;

        autoSending = true;
        autoSendThread = new Thread(() => AutoSendValues(selectedTopic, maxAutoValue, autoSendInterval));
        autoSendThread.Start();
    }

    // Método para detener envío automático
    public void StopAutoSend()
    {
        autoSending = false;
        if (autoSendThread != null && autoSendThread.IsAlive)
        {
            autoSendThread.Join();
        }
    }

    private void AutoSendValues(string topic, int maxValue, int intervalMs)
    {
        topic = topic == "All" ? "" : topic;
        Debug.Log($"Iniciando envío automático para tópico: {selectedTopic}");

        for (int i = 0; i <= maxValue && autoSending; i++)
        {
            if (!isRunning) break;

            try
            {
                pubSocket.SendMoreFrame(topic).SendFrame(i.ToString());
                Debug.Log($"Enviado -> Tópico: {selectedTopic}, Valor: {i}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error en envío automático: {e.Message}");
                Reconnect();
            }

            Thread.Sleep(intervalMs);
        }

        autoSending = false;
        Debug.Log("Envío automático finalizado");
    }

    private void Reconnect()
    {
        try
        {
            if (pubSocket != null)
            {
                pubSocket.Dispose();
            }

            pubSocket = new PublisherSocket();
            pubSocket.Options.SendHighWatermark = 1000;
            pubSocket.Bind(connectionString);

            Debug.Log("Reconexión exitosa del publisher");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error en reconexión: {e.Message}");
        }
    }

    void OnDestroy()
    {
        isRunning = false;
        autoSending = false;

        if (autoSendThread != null && autoSendThread.IsAlive)
        {
            autoSendThread.Join();
        }

        if (pubSocket != null)
        {
            pubSocket.Dispose();
        }

        NetMQConfig.Cleanup();
    }

    void Update()
    {
        // Actualizar endpoints cada 5 segundos (opcional)
        if (Time.time % 5 < Time.deltaTime && discoveryService != null)
        {
            UpdateEndpoints();
        }
    }
    
    private void UpdateEndpoints()
    {
        var newIPs = discoveryService.GetDiscoveredIPs();
        bool needsReconnect = !newIPs.SequenceEqual(currentEndpoints);

        if (needsReconnect && newIPs.Count > 0)
        {
            currentEndpoints = newIPs;
            ReinitializePublisher();
        }
    }

    private void ReinitializePublisher()
    {
        if (pubSocket != null)
        {
            pubSocket.Dispose();
        }

        pubSocket = new PublisherSocket();
        pubSocket.Options.SendHighWatermark = 1000;

        // Bind a todas las IPs detectadas
        foreach (var ip in currentEndpoints)
        {
            string fullAddress = $"{addressPrefix}{ip}:{port}";
            try
            {
                pubSocket.Bind(fullAddress);
                Debug.Log($"Publisher vinculado a: {fullAddress}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error al vincular a {fullAddress}: {e.Message}");
            }
        }
    }
}