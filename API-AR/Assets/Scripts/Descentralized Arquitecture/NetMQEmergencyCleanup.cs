using UnityEngine;
using NetMQ;

public class NetMQEmergencyCleanup : MonoBehaviour
{
    void OnApplicationQuit()
    {
        Debug.Log("EJECUTANDO LIMPIEZA DE EMERGENCIA");
        NetMQConfig.Cleanup(block: true);
        
        // Matar hilos persistentes
        System.Threading.Thread.Sleep(200);
        NetMQConfig.Cleanup(block: true);
    }
}