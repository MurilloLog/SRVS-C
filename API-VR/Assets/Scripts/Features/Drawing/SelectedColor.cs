using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SelectedColor : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float pulseSpeed = 1f;
    public float minScale = 0.9f;
    public float maxScale = 1.1f;

    private Vector3 _originalScale;
    private Coroutine _animationCoroutine;

    private void Start()
    {
        _originalScale = transform.localScale;
        ImageSelectionManager.Instance.RegisterImage(this); // Registra esta imagen en el Manager
    }

    public void OnClick() // Llamar este método desde el evento OnClick del botón
    {
        ImageSelectionManager.Instance.SelectImage(this); // Notifica al Manager
    }

    public void StartAnimation()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        _animationCoroutine = StartCoroutine(Animate());
    }

    public void StopAnimation()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        transform.localScale = _originalScale;
        //transform.rotation = Quaternion.identity; // Opcional: Resetea la rotación
    }

    private IEnumerator Animate()
    {
        float timer = 0f;
        while (true)
        {
            // Rotación
            transform.Rotate(Vector3.back, rotationSpeed * Time.deltaTime);

            // Escalado pulsante
            timer += Time.deltaTime * pulseSpeed;
            float scale = Mathf.Lerp(minScale, maxScale, Mathf.PingPong(timer, 1f));
            transform.localScale = _originalScale * scale;

            yield return null;
        }
    }
}