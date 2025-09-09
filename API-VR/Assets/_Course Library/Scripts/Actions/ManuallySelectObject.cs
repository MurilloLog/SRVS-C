using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This script either forces the selection or deselection of an interactable objects by the interactor this script is on.
/// </summary>

public class ManuallySelectObject : MonoBehaviour
{
    [Tooltip("What object are we selecting?")]
    public XRBaseInteractable interactable = null;

    private XRBaseControllerInteractor interactor = null;
    private XRInteractionManager interactionManager = null;

    private XRBaseControllerInteractor.InputTriggerType originalTriggerType;

    private void Awake()
    {
        interactor = GetComponent<XRBaseControllerInteractor>();
        interactionManager = interactor.interactionManager;
        originalTriggerType = interactor.selectActionTrigger;
    }

    public void ManuallySelect()
    {
        interactable.gameObject.SetActive(true);
        interactor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.StateChange;
        interactionManager.SelectEnter(interactor as IXRSelectInteractor, interactable);
    }

    public void ManuallyDeselect()
    {
        /* Funcion original
        interactionManager.SelectExit(interactor as IXRSelectInteractor, interactable);
        interactor.selectActionTrigger = originalTriggerType;
        interactable.gameObject.SetActive(false);
        */

        // Verificar que los componentes existen
        if (interactor == null || interactable == null || interactionManager == null)
        {
            //Debug.LogWarning("Componentes no asignados en ManuallyDeselect");
            return;
        }

        // Convertir a las interfaces necesarias
        IXRSelectInteractor selectInteractor = interactor as IXRSelectInteractor;
        IXRSelectInteractable selectInteractable = interactable as IXRSelectInteractable;

        // Verificar si el interactor esta seleccionando este interactable
        if (selectInteractor != null && selectInteractable != null && 
            selectInteractor.interactablesSelected.Contains(selectInteractable))
        {
            // Realizar la deseleccion
            interactionManager.SelectExit(selectInteractor, selectInteractable);
            
            // Restaurar el trigger original
            interactor.selectActionTrigger = originalTriggerType;
            
            // Desactivar el objeto
            interactable.gameObject.SetActive(false);
        }
        else
        {
            //Debug.LogWarning("Intento de deseleccionar un objeto no seleccionado");
        }
    }
}
