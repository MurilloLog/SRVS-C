using UnityEngine;

[System.Serializable]
public class PlayerPoseModel : ISerializationModel
{
    public string command;
    public string playerID;
    public Vector3 position;
    public Quaternion rotation;
    
    public string ToJson() => JsonUtility.ToJson(this);
    public void FromJson(string json) => JsonUtility.FromJsonOverwrite(json, this);
}