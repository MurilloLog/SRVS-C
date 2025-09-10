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

    void Awake()
    {
        eventsManager = FindObjectOfType<Events>();
        if (eventsManager == null)
        {
            Debug.LogError("Events manager no encontrado");
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

    public void EndTrail()
    {
        if (currentTrail && eventsManager != null && currentTrailRenderer != null)
        {
            // Obtener puntos directamente del TrailRenderer
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
