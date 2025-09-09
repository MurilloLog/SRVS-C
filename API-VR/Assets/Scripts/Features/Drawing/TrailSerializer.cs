using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class TrailSerializer : MonoBehaviour
{
    public static TrailSerializer Instance { get; private set; }
    
    public GameObject trailPrefab;
    
    // Cola para operaciones que deben ejecutarse en el hilo principal
    private readonly Queue<System.Action> _mainThreadActions = new Queue<System.Action>();
    private readonly object _queueLock = new object();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Opcional si necesitas persistencia
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Procesar todas las acciones en cola
        lock (_queueLock)
        {
            while (_mainThreadActions.Count > 0)
            {
                _mainThreadActions.Dequeue()?.Invoke();
            }
        }
    }

    public void GenerateTrailFromJSON(string jsonData)
    {
        try
        {
            // La deserialización puede hacerse en cualquier hilo
            var drawing = SerializationManager.Instance.Deserialize<DrawingModel>(jsonData);
            
            // Encolar la creación del trail para el hilo principal
            EnqueueForMainThread(() => {
                CreateTrail(drawing);
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al generar trail: {e.Message}");
        }
    }

    private void EnqueueForMainThread(System.Action action)
    {
        lock (_queueLock)
        {
            _mainThreadActions.Enqueue(action);
        }
    }

    private void CreateTrail(DrawingModel drawing)
    {
        // 1. Crear el Trail
        GameObject newTrail = Instantiate(trailPrefab, drawing.anchorPosition, Quaternion.identity);
        
        // 2. Configurar el TrailRenderer
        TrailRenderer trailRenderer = newTrail.GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            Debug.LogError("El prefab no contiene TrailRenderer");
            Destroy(newTrail);
            return;
        }

        // 3. Aplicar configuración
        trailRenderer.widthMultiplier = drawing.size / 100f;
        trailRenderer.startColor = IntToColor(drawing.lineColor);
        trailRenderer.endColor = IntToColor(drawing.lineColor);
        trailRenderer.time = Mathf.Infinity;
        trailRenderer.minVertexDistance = 0.01f;

        // 4. Generar puntos (versión optimizada)
        GenerateTrailPoints(newTrail, drawing.linePoints);
    }

    private void GenerateTrailPoints(GameObject trailObj, List<Vector3> points)
    {
        if (points == null || points.Count == 0) return;

        TrailRenderer trailRenderer = trailObj.GetComponent<TrailRenderer>();
        trailRenderer.Clear();
        
        // Método rápido para generar el trail
        StartCoroutine(GenerateTrailCoroutine(trailObj, points));
    }

    private System.Collections.IEnumerator GenerateTrailCoroutine(GameObject trailObj, List<Vector3> points)
    {
        TrailRenderer trailRenderer = trailObj.GetComponent<TrailRenderer>();

        // Limpiar y preparar el trail
        trailRenderer.Clear();
        // Posicionamiento inicial
        trailObj.transform.position = points[0]; 
        yield return null; // Esperar un frame para que Unity procese el clear

        // Mover a través de los puntos
        for (int i = 1; i < points.Count; i++)
        {
            trailObj.transform.position = points[i];
            trailRenderer.AddPosition(points[i]); // Asegurar que el punto se añade al trail
            
            yield return null;
        }
    }

    private Color IntToColor(int colorValue)
    {
        // Asegurarnos que el valor esté en el rango válido (0-5)
        int clampedValue = Mathf.Clamp(colorValue, 0, 5);
        
        switch (clampedValue)
        {
            case 0: // Rojo
                return new Color(1.0f, 0.1f, 0.1f);
            case 1: // Amarillo
                return new Color(1.0f, 1.0f, 0.1f);
            case 2: // Azul
                return new Color(0.1f, 0.1f, 1.0f);
            case 3: // Verde
                return new Color(0.1f, 1.0f, 0.1f);
            case 4: // Negro
                return new Color(0.0f, 0.0f, 0.0f);
            case 5: // Blanco
                return new Color(1.0f, 1.0f, 1.0f);
            default:
                return Color.white; // Valor por defecto si algo falla
        }
    }
}