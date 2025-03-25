using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SnapAndDetach : MonoBehaviour
{
    public Transform snapPoint; // Punto inicial opcional para el attach.
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            // Inicialmente, asigna el attachTransform al snapPoint.
            grabInteractable.attachTransform = snapPoint;

            // Suscribirse a los eventos de agarre y soltar.
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("Pos attach" + snapPoint.transform.position);
        //Debug.Log("Righ hand" + snapPoint.transform.position);
        // Cuando se agarra el objeto, se establece el attachTransform a null para liberarlo.
        grabInteractable.attachTransform = null;
        // Usar un retardo para cambiar el farAttachMode y evitar conflictos inmediatos.
       // StartCoroutine(SetFarAttachModeDelayed(UnityEngine.XR.Interaction.Toolkit.Attachment.InteractableFarAttachMode.DeferToInteractor, 0.1f));
    }

    private IEnumerator SetFarAttachModeDelayed(UnityEngine.XR.Interaction.Toolkit.Attachment.InteractableFarAttachMode mode, float delay)
    {
        yield return new WaitForSeconds(delay);
        grabInteractable.farAttachMode = mode;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // Al soltar, se restablece el attachTransform al snapPoint.
        grabInteractable.attachTransform = snapPoint;
        // Opcional: puedes restablecer el farAttachMode inmediatamente o con retardo si lo prefieres.
       // grabInteractable.farAttachMode = UnityEngine.XR.Interaction.Toolkit.Attachment.InteractableFarAttachMode.Near;
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}
