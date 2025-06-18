using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ToggleGrab : XRGrabInteractable
{
    private IXRSelectInteractor currentInteractor;
    private bool isGrabbed = false;
    private bool gripPressedLastFrame = false;
    private float lastGripTime = 0f;
    private float gripCooldown = 0.3f; // ⏱ Evita doble acción rápida
    private bool forceManualRelease = false;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        currentInteractor = args.interactorObject as IXRSelectInteractor;
        isGrabbed = true;
        forceManualRelease = false;
        Debug.Log("Agarrado manualmente.");
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        if (!forceManualRelease)
        {
            Debug.Log("Evité soltado automático.");
            interactionManager.SelectEnter(args.interactorObject, this); // Reagarrar
        }
        else
        {
            Debug.Log("Soltado manualmente.");
            base.OnSelectExited(args);
        }
    }

    private void Update()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed);

        float timeNow = Time.time;

        if (gripPressed && !gripPressedLastFrame && (timeNow - lastGripTime > gripCooldown))
        {
            lastGripTime = timeNow;

            if (isGrabbed)
            {
                forceManualRelease = true;
                interactionManager.SelectExit(currentInteractor, this);
                isGrabbed = false;
                currentInteractor = null;
            }
            else if (currentInteractor != null)
            {
                interactionManager.SelectEnter(currentInteractor, this);
                isGrabbed = true;
                forceManualRelease = false;
            }
        }

        gripPressedLastFrame = gripPressed;
    }

    public bool IsGrabbed()
    {
        return isGrabbed;
    }
}
