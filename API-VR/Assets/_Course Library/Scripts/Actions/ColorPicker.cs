using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    [Range(0, 1)] public float red = 0.0f;
    [Range(0, 1)] public float green = 0.0f;
    [Range(0, 1)] public float blue = 0.0f;

    [System.Serializable]
    public class ColorChangeEvent : UnityEvent<Color> { }
    public ColorChangeEvent OnColorChange;// = new ColorChangeEvent();

    // Métodos para cambiar cada canal
    public void SetRed(float value)
    {
        red = Mathf.Clamp01(value);
        green = 0.1f;
        blue = 0.1f;
        UpdateColor();
    }

    public void SetYellow()
    {
        red = 1.0f;
        green = 1.0f;
        blue = 0.1f;
        UpdateColor();
    }

    public void SetGreen(float value)
    {
        red = 0.1f;
        green = Mathf.Clamp01(value);
        blue = 0.1f;
        UpdateColor();
    }

    public void SetBlue(float value)
    {
        red = 0.1f;
        green = 0.1f;
        blue = Mathf.Clamp01(value);
        UpdateColor();
    }

    public void SetBlack()
    {
        blue = 0.0f;
        green = 0.0f;
        red = 0.0f;
        UpdateColor();
    }

    public void SetWhite()
    {
        blue = 1.0f;
        green = 1.0f;
        red = 1.0f;
        UpdateColor();
    }

    // Actualiza el color combinando los canales
    private void UpdateColor()
    {
        Color newColor = new Color(red, green, blue);
        OnColorChange.Invoke(newColor);
    }
}