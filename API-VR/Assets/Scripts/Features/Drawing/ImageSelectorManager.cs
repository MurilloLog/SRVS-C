using UnityEngine;
using System.Collections.Generic;

public class ImageSelectionManager : MonoBehaviour
{
    public static ImageSelectionManager Instance { get; private set; }

    private List<SelectedColor> _selectableImages = new List<SelectedColor>();
    private SelectedColor _currentSelected;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterImage(SelectedColor image)
    {
        if (!_selectableImages.Contains(image))
        {
            _selectableImages.Add(image);
        }
    }

    public void SelectImage(SelectedColor selectedImage)
    {
        // Desactivar todas las imágenes primero
        foreach (var image in _selectableImages)
        {
            if (image != selectedImage)
            {
                image.StopAnimation();
            }
        }

        // Activar la nueva selección
        _currentSelected = selectedImage;
        selectedImage.StartAnimation();
    }
}