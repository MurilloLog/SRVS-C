using UnityEngine;

/* El script no puede ser un ScriptableObject al heredar MonoBehaviour, herencia indispensable ya que 
// forma parte de un GameObject. 
*/
// [CreateAssetMenu(fileName = "LineSettings", menuName = "Create Line Settings", order = 0)]
public class LineSettings : MonoBehaviour
{
    public string lineTagName = "Line";

    public Color startColor = Color.white;
    public Color endColor = Color.white;
    public float distanceFromCamera = 0.3f;

    public Material defaultColorMaterial;
    public Material[] materialOptions;
    public int currentColor;
    public int cornerVertices = 5;

    public int endCapVertices = 5;

    public float startWidth = 0.01f;
    public float endWidth = 0.01f;
    public int size = 1;

    [Range(0, 1.0f)]
    public float minDistanceBeforeNewPoint = 0.01f;

    [Header("Tolerance Options")]
    public bool allowSimplification = false;

    public float tolerance = 0.001f;

    public float applySimplifyAfterPoints = 20.0f;

    public void SelectColor(int c)
    {
        currentColor = c;
        defaultColorMaterial = materialOptions[c];
    }

    public int GetLineColor() { return currentColor; }
    public int GetLineSize() { return size; }
    public void SetLineRef(int _size)
    {
        switch (_size)
        {
            case 1:
                SetLineWidth(0.01f);
                break;
            case 2:
                SetLineWidth(0.025f);
                break;
            case 3:
                SetLineWidth(0.05f);
                break;
            default:
                SetLineWidth(0.01f);
                break;
        }
        size = _size;
    }
    public void SetLineWidth(float width)
    {
        startWidth = width;
        endWidth = width;
    }
}