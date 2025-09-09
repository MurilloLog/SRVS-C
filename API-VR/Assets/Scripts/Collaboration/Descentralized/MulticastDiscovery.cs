using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

using NetMQ;
using NetMQ.Sockets;

public class MulticastDiscovery : MonoBehaviour
{
    [Header("Multicast Settings")]
    [SerializeField] private string multicastAddress = "224.0.0.1"; // IP de multidifusión
    [SerializeField] private int port = 2718; // Puerto de comunicación
    [SerializeField] private float nodeTimeout = 10f; // Tiempo para considerar un nodo inactivo
    [SerializeField] private float discoveryInterval = 1f;
    [SerializeField] private float cleanupInterval = 2f;

    [Header("Debug Info")]
    [SerializeField] private string localIP;
    public string _localIP => localIP; // Propiedad pública para acceder a la IP local
    [SerializeField] private List<string> discoveredNodes = new List<string>();


    private UdpClient udpClient;
    private bool isRunning = false;
    private Dictionary<string, DateTime> knownNodes = new Dictionary<string, DateTime>();
    private readonly Queue<Action> mainThreadActions = new Queue<Action>();
    
    private Thread receiveThread;
    private float lastDiscoveryTime;
    private float lastCleanupTime;

    void Start()
    {
        isRunning = true;
        localIP = GetLocalIPAddress();
        
        if (string.IsNullOrEmpty(localIP))
        {
            Debug.LogError("[Multicast] No se pudo obtener la IP local.");
            return;
        }

        try
        {
            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            udpClient.JoinMulticastGroup(IPAddress.Parse(multicastAddress));

            Debug.Log($"[Multicast] Se ha unido al Multicast con mi direccion IP: {localIP}");

            // Iniciar hilos
            receiveThread = new Thread(ReceiveThreadLoop);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Multicast] Error al iniciar: {ex.Message}");
        }
    }

    void Update()
    {
        // Ejecutar acciones en el hilo principal
        while (mainThreadActions.Count > 0)
        {
            Action action = mainThreadActions.Dequeue();
            action?.Invoke();
        }

        // Enviar mensajes de descubrimiento en intervalos
        if (Time.time - lastDiscoveryTime >= discoveryInterval)
        {
            SendDiscoveryMessage();
            lastDiscoveryTime = Time.time;
        }

        // Limpiar nodos inactivos en intervalos
        if (Time.time - lastCleanupTime >= cleanupInterval)
        {
            CleanInactiveNodes();
            lastCleanupTime = Time.time;
        }
    }

    void OnDestroy()
    {
        isRunning = false;
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join(1000); // Esperar máximo 1 segundo
        }

        if (udpClient != null)
        {
            try
            {
                udpClient.DropMulticastGroup(IPAddress.Parse(multicastAddress));
                udpClient.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Multicast] Error al cerrar: {ex.Message}");
            }
        }
    }

    private void SendDiscoveryMessage()
    {
        try
        {
            string message = $"DISCOVERY|IP:{localIP}|TYPE:ARDrawing|TS:{DateTime.Now.Ticks}";
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, multicastAddress, port);
            //Debug.Log($"[Multicast] Mensaje de descubrimiento enviado: {message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Multicast] Error al enviar: {ex.Message}");
        }
    }

    private void ReceiveThreadLoop()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);

        while (isRunning)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEP);
                string message = Encoding.UTF8.GetString(data);

                // Solo procesamiento mínimo en el hilo secundario
                string nodeIP = ExtractIPFromMessage(message);

                if (!string.IsNullOrEmpty(nodeIP) && nodeIP != localIP)
                {
                    lock (knownNodes)
                    {
                        if (!knownNodes.ContainsKey(nodeIP))
                        {
                            knownNodes[nodeIP] = DateTime.Now;
                            mainThreadActions.Enqueue(() => OnNewNodeDiscovered(nodeIP));
                        }
                        else
                        {
                            knownNodes[nodeIP] = DateTime.Now; // Actualizar timestamp
                        }
                    }
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                break; // Salir cuando se cierre el socket
            }
            catch (Exception ex)
            {
                if (isRunning)
                    Debug.LogWarning($"[Multicast] Error de red: {ex.Message}");
            }
        }
    }

     private void CleanInactiveNodes()
    {
        lock (knownNodes)
        {
            var inactiveNodes = knownNodes
                .Where(node => (DateTime.Now - node.Value).TotalSeconds > nodeTimeout)
                .Select(node => node.Key)
                .ToList();

            foreach (var node in inactiveNodes)
            {
                knownNodes.Remove(node);
                mainThreadActions.Enqueue(() => OnNodeLost(node));
            }
        }
    }

    private void OnNewNodeDiscovered(string ip)
    {
        Debug.Log($"[Network] Nuevo nodo detectado: {ip}");
        UpdateDiscoveredNodesList();
        
        // Notificar al CommunicationManager
        var commManager = FindObjectOfType<CommunicationManager>();
        if (commManager != null)
        {
            commManager.UpdateKnownIPs(GetDiscoveredIPs());
        }
    }

    private void OnNodeLost(string ip)
    {
        Debug.Log($"[Network] Nodo perdido: {ip}");
        UpdateDiscoveredNodesList();
        
        // Notificar al CommunicationManager
        var commManager = FindObjectOfType<CommunicationManager>();
        if (commManager != null)
        {
            commManager.UpdateKnownIPs(GetDiscoveredIPs());
        }
    }

    private void UpdateDiscoveredNodesList()
    {
        lock (knownNodes)
        {
            discoveredNodes = knownNodes.Keys.ToList();
        }
    }

    private string ExtractIPFromMessage(string message)
    {
        var parts = message.Split('|');
        foreach (var part in parts)
        {
            if (part.StartsWith("IP:"))
            {
                return part.Substring(3);
            }
        }
        return null;
    }

    private string GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                {
                    Debug.Log($"[Multicast] Mi IP identificada en la red es: {ip.ToString()}");
                    return ip.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Multicast] Error al obtener mi IP: {ex.Message}");
        }

        return null;
    }

    // Método público para obtener las IPs activas
    public List<string> GetDiscoveredIPs()
    {
        lock (knownNodes)
        {
            return knownNodes.Keys.Where(ip => ip != localIP).ToList();
        }
    }
}