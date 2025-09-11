using ARDrawing.Core.Singletons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using System.Linq;
using System;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

[RequireComponent(typeof(ARAnchorManager))]
public class ARDrawManager : Singleton<ARDrawManager>
{
    // Configuración del área de dibujo
    [SerializeField] private float areaWidthPercentage = 1.0f;
    [SerializeField] private float areaHeightPercentage =0.75f;
    [SerializeField] private float areaXPositionPercentage = 0f;
    [SerializeField] private float areaYPositionPercentage = 0f;
    [SerializeField] private Rect drawingArea;
    [SerializeField] private bool showDrawingAreaIndicator = false;
    [SerializeField] private Color areaIndicatorColor = new Color(0f, 1.0f, 0f, 0.2f);

    // Configuración de la línea
    [SerializeField] private LineSettings lineSettings;
    [SerializeField] private int currentSelectedColor = 5; // Default color
    [SerializeField] private UnityEvent OnDraw;
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private Camera arCamera;

    [Header("Drawing Animation Settings")]
    [SerializeField] private float drawSpeed = 0.02f; // Tiempo entre puntos (ajustable)
    [SerializeField] private float interpolationSteps = 1f; // Pasos de interpolación entre puntos
    [SerializeField] private float maxPointsPerFrame = 5f; // Máximo de puntos a procesar por frame para optimización

    private Queue<DrawingTask> drawingQueue = new Queue<DrawingTask>();
    private bool isDrawing = false;

    // Clase para manejar tareas de dibujo
    private class DrawingTask
    {
        public Drawings drawingData;
        public ARLine line;
        public ARAnchor anchor;
    }

    private List<ARAnchor> arAnchors = new List<ARAnchor>();
    private Dictionary<int, ARLine> Lines = new Dictionary<int, ARLine>();
    private bool CanDraw { get; set; }
    public Events backendEvents;

    // Variables para el indicador visual
    private GameObject areaIndicator;
    private Image indicatorImage;

    void Awake()
    {
        backendEvents = FindObjectOfType<Events>();
    }

    void Start()
    {
        UpdateDrawingArea();
        //CreateAreaIndicator();
    }
    
    private void HandleDrawingMessage(string message)
    {
        
        try
        {
            Drawings drawingData = JsonUtility.FromJson<Drawings>(message);
            if (drawingData != null && drawingData.roomId == backendEvents.roomId)
            {
                CreateAnchorFromData(drawingData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing drawing message: {e.Message}");
        }
    }

    private void CreateAreaIndicator()
    {
        // Crear Canvas si no existe
        if (GameObject.Find("DrawingAreaCanvas") == null)
        {
            GameObject canvasGO = new GameObject("DrawingAreaCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
        }

        // Crear el objeto indicador
        areaIndicator = new GameObject("DrawingAreaIndicator");
        areaIndicator.transform.SetParent(GameObject.Find("DrawingAreaCanvas").transform);

        // Añadir componente Image
        indicatorImage = areaIndicator.AddComponent<Image>();
        indicatorImage.color = new Color(1f, 0f, 0f, 0.2f);

        // Configurar RectTransform correctamente
        RectTransform rt = areaIndicator.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = Vector2.zero;

        UpdateIndicatorPosition();
    }

    private void UpdateDrawingArea()
    {
        float areaWidth = Screen.width * areaWidthPercentage;
        float areaHeight = Screen.height * areaHeightPercentage;
        float areaX = Screen.width * areaXPositionPercentage;
        float areaY = Screen.height * (1 - areaYPositionPercentage - areaHeightPercentage);

        drawingArea = new Rect(areaX, areaY, areaWidth, areaHeight);
        
        Debug.Log($"Área de dibujo: X={areaX}, Y={areaY}, Width={areaWidth}, Height={areaHeight}");
        
        if (areaIndicator != null)
        {
            UpdateIndicatorPosition();
        }
    }

    private void UpdateIndicatorPosition()
    {
        if (!showDrawingAreaIndicator || areaIndicator == null) return;

        RectTransform rt = areaIndicator.GetComponent<RectTransform>();
        
        // Configurar posición y tamaño
        rt.anchoredPosition = new Vector2(drawingArea.x, drawingArea.y);
        rt.sizeDelta = new Vector2(drawingArea.width, drawingArea.height);
        
        // Debug visual
        Debug.Log($"Indicador posicionado en: X={rt.anchoredPosition.x}, Y={rt.anchoredPosition.y}, " +
                $"Ancho={rt.sizeDelta.x}, Alto={rt.sizeDelta.y}");
    }

    public void DeserializeAndAddAnchor(string json)
    {
        // Deserializar el JSON recibido
        //Drawing drawingData = JsonUtility.FromJson<Drawing>(json);
        Drawings drawingData = JsonUtility.FromJson<Drawings>(json);
        //drawingData.SetTimestampT4(timestampT4);

        if (drawingData != null)
        {
            CreateAnchorFromData(drawingData);
            //Debug.Log("El anchor del otro jugador se creo correctamente.");
        }
        else
        {
            //Debug.LogError("No se pudo deserializar el JSON.");
        }
    }

    private void CreateAnchorFromData(Drawings drawingData)
    {
        // Ajustar las coordenadas del anchor (restar 1.15 m en Y)
        Vector3 adjustedAnchorPosition = drawingData.anchorPosition;
        adjustedAnchorPosition.y -= backendEvents.VROffset; // Ajuste para RV

        // Ajustar los puntos de la línea (restar 1.15 m en Y)
        List<Vector3> adjustedLinePoints = drawingData.linePoints.Select(point => {
            Vector3 adjustedPoint = point;
            adjustedPoint.y -= backendEvents.VROffset; // Ajuste para RV
            return adjustedPoint;
        }).ToList();
        
        // Crear un GameObject para el Anchor
        GameObject anchorObject = new GameObject($"Anchor_{drawingData.anchorID}");
        anchorObject.transform.position = adjustedAnchorPosition;//drawingData.anchorPosition;
        anchorObject.transform.rotation = Quaternion.identity;

        // Agregar un ARAnchor al GameObject
        ARAnchor anchor = anchorObject.AddComponent<ARAnchor>();

        if (anchor == null)
        {
            //Debug.LogError("Error al crear el anclaje desde los datos recibidos.");
            return;
        }

        // Agregar el Anchor a la lista local
        arAnchors.Add(anchor);

        // Crear una nueva linea y aniadir los puntos recibidos
        ARLine line = new ARLine(lineSettings, this);
        currentSelectedColor = line.GetCurrentColor();
        lineSettings.SelectColor(drawingData.lineColor);
        lineSettings.SetLineRef(drawingData.size);
        line.AddNewLineRenderer(transform, anchor, adjustedAnchorPosition); //drawingData.anchorPosition);
        
        // Encolar la tarea de dibujo
        drawingQueue.Enqueue(new DrawingTask {
            drawingData = drawingData,
            line = line,
            anchor = anchor
        });

        // Iniciar el proceso si no estaba activo
        if (!isDrawing)
        {
            StartCoroutine(ProcessDrawingQueue());
        }

        // Aniadir la linea al diccionario local
        //Lines.Add(Lines.Count, line);
        lineSettings.SelectColor(currentSelectedColor);

        // Timestamp desde donde se considera que ha finalizado la replica de una instruccion
        //long T5 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //drawingData.SetTimestampT5(T5);
        //drawingData.SaveLatencyData();
        //Debug.LogError("Mensaje replicado exitosamente a las: " + T5.ToString());
        //Debug.LogError("La latencia del mensaje fue de: " + (drawingData.T1 - T5).ToString());


        //Debug.Log($"Anclaje y linea creados con exito desde los datos: {drawingData.anchorID}");
        backendEvents.drawing = false;
    }

    private IEnumerator ProcessDrawingQueue()
    {
        isDrawing = true;
        
        while (drawingQueue.Count > 0)
        {
            DrawingTask task = drawingQueue.Dequeue();
            yield return StartCoroutine(AnimateLineDrawing(task.line, task.drawingData.linePoints));
        }

        isDrawing = false;
    }

    private IEnumerator AnimateLineDrawing(ARLine line, List<Vector3> points)
    {
        if (points == null || points.Count == 0) yield break;

        // Aplicar offset a todos los puntos recibidos
        List<Vector3> adjustedPoints = points.Select(point => {
            Vector3 adjustedPoint = point;
            adjustedPoint.y -= backendEvents.VROffset; // Ajuste para RV
            return adjustedPoint;
        }).ToList();

        // Dibujar el primer punto inmediatamente
        line.AddPoint(adjustedPoints[0]);

        for (int i = 1; i < adjustedPoints.Count; i++)
        {
            Vector3 startPoint = adjustedPoints[i - 1];
            Vector3 endPoint = adjustedPoints[i];
            float distance = Vector3.Distance(startPoint, endPoint);
            int steps = Mathf.CeilToInt(distance * interpolationSteps);

            // Procesar varios puntos por frame para optimización
            int pointsThisFrame = 0;
            
            for (int j = 1; j <= steps; j++)
            {
                float t = j / (float)steps;
                Vector3 interpolatedPoint = Vector3.Lerp(startPoint, endPoint, t);
                line.AddPoint(interpolatedPoint);

                pointsThisFrame++;
                if (pointsThisFrame >= maxPointsPerFrame)
                {
                    pointsThisFrame = 0;
                    yield return null; // Esperar al siguiente frame
                }
            }

            yield return new WaitForSeconds(drawSpeed);
        }

        // Restaurar configuración al finalizar
        lineSettings.SelectColor(currentSelectedColor);
        backendEvents.drawing = false;
    }

    private void OnEnable()
    {
        // Habilitar el Enhanced Touch support
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        // Deshabilitar el Enhanced Touch support
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
#if !UNITY_EDITOR
        var activeTouches = Touch.activeTouches;
        if (activeTouches.Count > 0)
        {
            var touch = activeTouches[0];
            Vector3 touchPosition = touch.screenPosition;

            // Verificar si el toque está dentro del área de dibujo
            if (drawingArea.Contains(touchPosition))
            {
                DrawOnTouch();
            }
        }
#else
        // Código para editor permanece igual
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            if (drawingArea.Contains(mousePos))
            {
                DrawOnMouse();
            }
        }

#endif
    }

    public void AllowDraw(bool isAllow)
    {
        CanDraw = isAllow;
    }

    private void DrawOnTouch()
    {
        var activeTouches = Touch.activeTouches;
        if (activeTouches.Count > 0)
        {
            DrawOnTouch(activeTouches[0], anchorManager);
        }
    }

    void DrawOnTouch(Touch touch, ARAnchorManager anchorManager)
    {
        if (!CanDraw) return;

        Vector3 touchPosition = arCamera.ScreenToWorldPoint(
            new Vector3(touch.screenPosition.x, touch.screenPosition.y, lineSettings.distanceFromCamera));

        if (touch.phase == TouchPhase.Began)
        {
            OnDraw?.Invoke();

            // Crear un GameObject en la posición del toque
            GameObject anchorObject = new GameObject("ARAnchor");
            anchorObject.transform.position = touchPosition;
            anchorObject.transform.rotation = Quaternion.identity;

            // Agregar un componente ARAnchor al GameObject creado
            ARAnchor anchor = anchorObject.AddComponent<ARAnchor>();

            // Validar la creación del anclaje
            if (anchor == null)
                Debug.LogError("Error creating reference point");
            else
            {
                arAnchors.Add(anchor);
            }

            ARLine line = new ARLine(lineSettings);
            Lines.Add(touch.touchId, line);  // Cambiado de fingerId a touchId
            line.AddNewLineRenderer(transform, anchor, touchPosition);
        }
        else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        {
            Lines[touch.touchId].AddPoint(touchPosition);
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            // Get the line and anchor associated with this touch
            ARLine line = Lines[touch.touchId];
            ARAnchor anchor = arAnchors[arAnchors.Count - 1];

            // Round positions and prepare data for serialization
            Vector3 roundedAnchorPosition = anchor.transform.position;
            roundedAnchorPosition.y += backendEvents.VROffset;
            
            List<Vector3> roundedLinePoints = line.GetPoints().Select(point => {
                Vector3 adjustedPoint = RoundVector3(point);
                adjustedPoint.y += backendEvents.VROffset;
                return adjustedPoint;
            }).ToList();

            // Create the Drawings object
            Drawings serializedData = new Drawings(
                backendEvents.id,
                backendEvents.roomId,
                anchor.trackableId.ToString(),
                roundedAnchorPosition,
                roundedLinePoints,
                line.GetCurrentColor(),
                line.GetSize()
            );

            // Convert to JSON
            string json = JsonUtility.ToJson(serializedData);
            // Send to other peers
            backendEvents.sendRoomAction(json + "|");
            Lines.Remove(touch.touchId);
        }
    }

    private void HandleNetworkDrawing(string message)
    {
        try
        {
            Drawings drawingData = JsonUtility.FromJson<Drawings>(message);
            if (drawingData != null)
            {
                CreateAnchorFromData(drawingData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing network drawing: {e.Message}");
        }
    }
    private Vector3 RoundVector3(Vector3 vector)
    {
        return new Vector3(
            Mathf.Round(vector.x * 1000f) / 1000f,
            Mathf.Round(vector.y * 1000f) / 1000f,
            Mathf.Round(vector.z * 1000f) / 1000f
        );
    }

    private Quaternion RoundQuaternion(Quaternion quaternion)
    {
        return new Quaternion(
            Mathf.Round(quaternion.x * 1000f) / 1000f,
            Mathf.Round(quaternion.y * 1000f) / 1000f,
            Mathf.Round(quaternion.z * 1000f) / 1000f,
            Mathf.Round(quaternion.w * 1000f) / 1000f
        );
    }

    public void ClearAnchors()
    {
        StopAllCoroutines();
        isDrawing = false;
        drawingQueue.Clear();

        foreach (var line in Lines.Values)
        {
            if (line != null)
            {
                line.DestroyLine(); 
            }
        }
        Lines.Clear();

        // Borrar los anchors almacenados en la lista cuando se selecciona el borrador
        foreach (ARAnchor anchor in arAnchors)
        {
            if (anchor != null  && anchor.gameObject != null)
            {
                // Eliminar el componente ARAnchor
                Destroy(anchor.gameObject);
            }
        }
        // Limpiar la lista despues de borrar los anchors
        arAnchors.Clear();

        //Debug.Log("All lines and their anchors were deleted!");
        //Debug.Log($"The number of ARAnchors is: {anchorCount}");
        //Debug.Log($"The number of ARLines is: {lineCount}");
    }

    private void ShowAnchorInfo(ARAnchor anchor)
    {
        /*  
         *  Proporcionar informacion sobre el anchor creado
         *  Este metodo es exclusivo para observar y comprender el comportamiento 
         *  de los anchors dentro de la escena RA. Puede comentarse cuando ya no 
         *  sea necesario.
        */

        int anchorCount = arAnchors.Count;
        int lineCount = Lines.Count;

        // Posicion del anchor
        Vector3 anchorPosition = anchor.transform.position;
        // Rotacion del anchor
        Quaternion anchorRotation = anchor.transform.rotation;
        // ID del anchor
        string anchorID = anchor.trackableId.ToString();

        Debug.Log("A new anchor was created!");
        Debug.Log($"Anchor position: {anchorPosition}");
        Debug.Log($"Anchor rotation: {anchorRotation}");
        Debug.Log($"Anchor trackableId: {anchorID}");

        Debug.Log($"Now there are {arAnchors.Count} anchors in the AR world");
        Debug.Log($"The number of ARAnchors is: {anchorCount}");
        Debug.Log($"The number of ARLines is: {lineCount}");
    }

    private void DrawOnMouse()
    {
        if (!CanDraw) return;

        Vector3 mousePos = arCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, lineSettings.distanceFromCamera));
        if (Input.GetMouseButtonDown(0))
        {
            OnDraw?.Invoke();
            if (Lines.Keys.Count == 0)
            {
                ARLine line = new ARLine(lineSettings);
                Lines.Add(0, line);
                line.AddNewLineRenderer(transform, null, mousePos);
            }
            else
            {
                Lines[0].AddPoint(mousePos);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Lines.Remove(0);
        }
    }
    
    public void ToggleAreaIndicator(bool visible)
    {
        showDrawingAreaIndicator = visible;
        if (areaIndicator != null)
        {
            areaIndicator.SetActive(visible);
        }
    }

    void OnDestroy()
    {
        if (areaIndicator != null)
        {
            Destroy(areaIndicator);
        }
    }
}