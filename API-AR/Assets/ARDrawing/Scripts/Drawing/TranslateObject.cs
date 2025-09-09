using UnityEngine;

public class TranslateObject : MonoBehaviour
{
    private Vector3 startPos;
    private Quaternion startRot;

    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    // Método principal de traslación modificado para mover hacia arriba
    public void TranslateUp(float amount)
    {
        transform.position += transform.up * amount;
    }

    // Métodos secundarios para otros ejes
    public void TranslateRight(float amount)
    {
        transform.position += transform.right * amount;
    }

    public void TranslateForward(float amount)
    {
        transform.position += transform.forward * amount;
    }

    // Métodos para establecer posición específica
    public void SetLocalHeight(float newHeight)
    {
        transform.localPosition = new Vector3(transform.localPosition.x, newHeight, transform.localPosition.z);
    }

    public void SetLocalRightPosition(float newPosition)
    {
        transform.localPosition = new Vector3(newPosition, transform.localPosition.y, transform.localPosition.z);
    }

    public void SetLocalXPosition(float newPosition)
    {
        transform.localPosition = new Vector3(newPosition, transform.localPosition.y, transform.localPosition.z);
    }

    public void SetLocalForwardPosition(float newPosition)
    {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, newPosition);
    }

    public void ResetToStartingPosition()
    {
        transform.SetPositionAndRotation(startPos, startRot);
    }
}