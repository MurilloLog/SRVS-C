using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class LatencyMeasurementManager : MonoBehaviour
{
    public static LatencyMeasurementManager Instance { get; private set; }
    
    private string filePath;
    private Dictionary<string, (long T1, int size, int pointCount)> t1Data = new Dictionary<string, (long, int, int)>();
    private int sampleCount = 0;
    private const int maxSamples = 150;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            filePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "latency_data.csv");
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "AnchorID,T1,T4,T5,MessageSize,PointCount\n");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RecordT1(string anchorID, long t1, int size, int pointCount)
    {
        if (sampleCount < maxSamples)
        {
            t1Data[anchorID] = (t1, size, pointCount);
        }
    }

    public void SaveLatencyData(string anchorID, long t4, long t5)
    {
        if (sampleCount < maxSamples)
        {
            string line;

            if (t1Data.ContainsKey(anchorID))
            {
                // Caso normal: tenemos todos los datos
                (long t1, int size, int pointCount) = t1Data[anchorID];
                line = $"{anchorID},{t1},{t4},{t5},{size},{pointCount}\n";
                t1Data.Remove(anchorID);
            }
            else
            {
                // Caso donde no tenemos registro T1: usar -1 para los valores faltantes
                line = $"{anchorID},-1,{t4},{t5},-1,-1\n";
                Debug.LogWarning($"No se encontró registro T1 para anchorID: {anchorID}. Se guarda con valores -1.");
            }

            File.AppendAllText(filePath, line);
            Debug.Log("Latency data saved: " + line);

            sampleCount++;
            if (sampleCount >= maxSamples)
            {
                Debug.Log($"{maxSamples} muestras de latencia registradas. La medición ha finalizado.");
            }
        }
        else
        {
            Debug.Log("Límite de muestras alcanzado. No se guardan más datos de latencia.");
        }
    }
}