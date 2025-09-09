using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Drawings
{
    public string command = "DRAWING";
    public string _id; // Unique identifier for the user provided by the server
    public string roomId; // Unique identifier for the room provided by the server
    public string anchorID; // Unique identifier for the anchor created randomly
    public Vector3 anchorPosition;
    public List<Vector3> linePoints; // List of points in the line
    public int lineColor; // Could store as hex or reference to color palette
    public int size; // Line width or size

    public Drawings(string playerId, string _roomId, string anchorId, Vector3 position, List<Vector3> points, int color, int _size)
    {
        command = "DRAWING";
        _id = playerId;
        roomId = _roomId;
        anchorID = anchorId;
        anchorPosition = position;
        linePoints = points;
        lineColor = color;
        size = _size;
    }
}