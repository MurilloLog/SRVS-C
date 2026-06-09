using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This script creates a trail at the location of a gameobject with a particular width and color.
/// </summary>

public class CreateTrail : MonoBehaviour
{
    public GameObject trailPrefab = null;
    public Events eventsManager;

    private float width = 0.05f;
    private int sizeLine = 1;
    private int lineColor = 5;
    private Color color = Color.white;

    private GameObject currentTrail = null;
    private TrailRenderer currentTrailRenderer = null;
    private string anchorID;
    private LatencyMeasurementManager latencyManager; // Reference to the latency manager
     private DrawingDataLogger dataLogger;

    void Awake()
    {
        eventsManager = FindObjectOfType<Events>();
        latencyManager = FindObjectOfType<LatencyMeasurementManager>();
        dataLogger = FindObjectOfType<DrawingDataLogger>();
        if (eventsManager == null)
        {
            Debug.LogError("Events manager no encontrado");
        }
        if (latencyManager == null)
        {
            Debug.LogError("LatencyMeasurementManager no encontrado");
        }
    }

    public void StartTrail()
    {
        if (!currentTrail && eventsManager != null)
        {
            anchorID = System.Guid.NewGuid().ToString();
            currentTrail = Instantiate(trailPrefab, transform.position, transform.rotation, transform);
            currentTrailRenderer = currentTrail.GetComponent<TrailRenderer>();
            ApplySettings(currentTrail);
            Debug.Log("Trail started with ID: " + anchorID);
        }
    }

    // Make sure to call this method to end the trail and send data from Unity inspector
    public void EndTrail()
    {
        if (currentTrail && eventsManager != null && currentTrailRenderer != null)
        {
            // Get all positions from the TrailRenderer
            Vector3[] trailPositions = new Vector3[currentTrailRenderer.positionCount];
            currentTrailRenderer.GetPositions(trailPositions);
            eventsManager.drawing = false;

            Drawings drawingData = new Drawings
            {
                command = "DRAWING",
                _id = eventsManager.id,
                roomId = eventsManager.roomId,
                anchorID = anchorID,
                anchorPosition = trailPositions.Length > 0 ? trailPositions[0] : Vector3.zero,
                linePoints = new List<Vector3>(trailPositions),
                lineColor = lineColor,
                size = sizeLine
            };

            string jsonData = JsonUtility.ToJson(drawingData, false) + "|";
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError("JSON data is empty.");
            }
            else
            {
                // Send the drawing data to the server and other clients via the events manager
                eventsManager.sendRoomAction(jsonData);
                Debug.Log("Trail ended and data sent: " + jsonData);
            }

            currentTrail.transform.parent = null;
            currentTrail = null;
            currentTrailRenderer = null;
        }
    }

    public void EndTrailForLatencyTesting()
    {
        if (currentTrail && eventsManager != null && currentTrailRenderer != null)
        {
            // Get all positions from the TrailRenderer
            Vector3[] trailPositions = new Vector3[currentTrailRenderer.positionCount];
            currentTrailRenderer.GetPositions(trailPositions);
            eventsManager.drawing = false;

            List<Vector3> linePointsList = new List<Vector3>(trailPositions);
            int pointCount = linePointsList.Count;

            Drawings drawingData = new Drawings
            {
                command = "DRAWING",
                _id = eventsManager.id,
                roomId = eventsManager.roomId,
                anchorID = anchorID,
                anchorPosition = trailPositions.Length > 0 ? trailPositions[0] : Vector3.zero,
                linePoints = linePointsList,
                lineColor = lineColor,
                size = sizeLine
            };

            string jsonData = JsonUtility.ToJson(drawingData, false) + "|";
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError("JSON data is empty.");
            }
            else
            {
                // Measure latency and data size when ending the trail
                long T1 = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
                int messageSize = System.Text.Encoding.UTF8.GetByteCount(jsonData);
                latencyManager.RecordT1(anchorID, T1, messageSize, pointCount);
                dataLogger.SaveDrawingData(drawingData.anchorID, drawingData.lineColor, drawingData.size, linePointsList);

                // Send the drawing data to the server and other clients via the events manager
                eventsManager.sendRoomAction(jsonData);
                Debug.Log("Trail ended and data sent: " + jsonData);
            }

            currentTrail.transform.parent = null;
            currentTrail = null;
            currentTrailRenderer = null;
        }
    }

    private void ApplySettings(GameObject trailObject)
    {
        TrailRenderer trailRenderer = trailObject.GetComponent<TrailRenderer>();
        trailRenderer.widthMultiplier = width;
        trailRenderer.startColor = color;
        trailRenderer.endColor = color;
    }

    public void SetWidth(float value)
    {
        width = value;
    }

    public void SetSize(int value)
    {
        sizeLine = value;
    }

    public void SetColor(Color value)
    {
        color = value;
    }
    public void SetLineColor(int value)
    {
        lineColor = value;
    }
}
