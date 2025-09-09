using UnityEngine;
using NetMQ;

public class ForceNetMQCleanup : MonoBehaviour
{
    void OnApplicationQuit()
    {
        Debug.Log("Forzando limpieza NetMQ");
        NetMQConfig.Cleanup(block: true);
        System.Threading.Thread.Sleep(100); // Pequeña pausa
    }
}