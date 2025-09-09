using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Drawings
{
    public string command;
    public string _id;
    public string roomId;
    public string anchorID;
    public Vector3 anchorPosition;
    //[SerializeField] private Quaternion anchorRotation;
    public List<Vector3> linePoints;
    public int lineColor; // Color in RGB int format
    public int size;

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

    public Drawings(string anchorId, Vector3 position, List<Vector3> points, int color, int _size)
    {
        anchorID = anchorId;
        anchorPosition = position;
        linePoints = points;
        lineColor = color;
        size = _size;
    }

    public void SetCommand(string _command) { command = _command; }
    public void SetPlayerID(string playerId) { _id = playerId; }
    public void SetAnchorId(string _anchorId) { anchorID = _anchorId; }
    public void SetAnchorPosition(Vector3 position) { anchorPosition = position; }
    public void SetLinePoints(List<Vector3> points) { linePoints = points; }
    public void SetColor(int lineColor)
    {
        this.lineColor = lineColor;
        // Optionally, you can convert the RGB int to Color if needed
        // Color color = new Color((lineColor >> 16 & 0xFF) / 255f, (lineColor >> 8 & 0xFF) / 255f, (lineColor & 0xFF) / 255f);
    }
    public void SetSize(int _size) { size = _size; }

    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }
    public void FromJson(string json)
    {
        Drawings data = JsonUtility.FromJson<Drawings>(json);
        command = data.command;
        _id = data._id;
        anchorID = data.anchorID;
        anchorPosition = data.anchorPosition;
        linePoints = data.linePoints;
        lineColor = data.lineColor;
        size = data.size;
    }
    public Vector3 GetAnchorPosition()
    {
        return anchorPosition;
    }
    public string GetCommand()
    {
        return command;
    }
    public string GetPlayerID()
    {
        return _id;
    }
    public string GetAnchorId()
    {
        return anchorID;
    }
    public List<Vector3> GetLinePoints()
    {
        return linePoints;
    }
    public int GetSize()
    {
        return size;
    }
    public void Clear()
    {
        command = "";
        _id = "";
        anchorID = "";
        anchorPosition = Vector3.zero;
        linePoints.Clear();
        lineColor = 5; // Reset color to white
        size = 1; // Reset size to default
    }

}
