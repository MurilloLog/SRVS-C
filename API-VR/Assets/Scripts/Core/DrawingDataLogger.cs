using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class DrawingDataLogger : MonoBehaviour
{
    public static DrawingDataLogger Instance { get; private set; }
    
    private string filePath;
    private const string fileName = "drawing_data.csv";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Se cambia la ruta para que apunte al directorio de la aplicación.
            filePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), fileName);

            // Crear el archivo con encabezado si no existe
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "AnchorID,LineColor,LineSize,LinePoints\n");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveDrawingData(string anchorID, int lineColor, float lineSize, List<Vector3> points)
    {
        // Serializar la lista de puntos con formato claro
        StringBuilder pointsString = new StringBuilder();
        for (int i = 0; i < points.Count; i++)
        {
            // Usar formato invariante para asegurar punto decimal
            pointsString.Append($"{points[i].x.ToString(System.Globalization.CultureInfo.InvariantCulture)},");
            pointsString.Append($"{points[i].y.ToString(System.Globalization.CultureInfo.InvariantCulture)},");
            pointsString.Append($"{points[i].z.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

            if (i < points.Count - 1)
            {
                pointsString.Append(";"); // Separador entre puntos
            }
        }

        // Crear la línea del CSV
        string lineSizeStr = lineSize.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string line = $"{anchorID},{lineColor},{lineSizeStr},{pointsString.ToString()}\n";

        // Escribir la línea en el archivo
        File.AppendAllText(filePath, line);
        Debug.Log($"Drawing data saved: {anchorID} with {points.Count} points");
    }


}