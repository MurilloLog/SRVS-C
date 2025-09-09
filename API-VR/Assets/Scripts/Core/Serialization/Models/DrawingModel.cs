using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DrawingModel : ISerializationModel
{
    public string command = "DRAWING";
    public string _id;
    public string roomId;
    public string anchorID;
    public Vector3 anchorPosition;
    public List<Vector3> linePoints;
    public int lineColor;
    public int size;

    public string ToJson() => JsonUtility.ToJson(this);
    public void FromJson(string json) => JsonUtility.FromJsonOverwrite(json, this);

    public void setCommand(string cmd) => command = cmd;
    public void setPlayerID(string id) => _id = id;
    public void setRoomID(string id) => roomId = id;
    public void setAnchorID(string id) => anchorID = id;
    public void setAnchorPosition(Vector3 position) => anchorPosition = position;
    public void setLinePoints(List<Vector3> points) => linePoints = points;
    public void setLineColor(int color) => lineColor = color;
    public void setSize(int newSize) => size = newSize;
}