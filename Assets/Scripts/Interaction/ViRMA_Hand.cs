using System.Collections;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_Hand : Hand
{
    public GameObject drumstick;
    public bool isFaded;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void UpdateHovering()
    {
        if ((noSteamVRFallbackCamera == null) && (isActive == false))
        {
            return;
        }

        if (hoverLocked)
            return;

        if (applicationLostFocusObject.activeSelf)
            return;

        float closestDistance = float.MaxValue;
        Interactable closestInteractable = null;

        if (useHoverSphere)
        {
            float scaledHoverRadius = hoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(hoverSphereTransform));
            CheckHoveringForTransform(hoverSphereTransform.position, scaledHoverRadius, ref closestDistance, ref closestInteractable, Color.green);
        }

        if (useControllerHoverComponent && mainRenderModel != null && mainRenderModel.IsControllerVisibile())
        {
            float scaledHoverRadius = controllerHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));

            // substitute drumstick object in place of SteamVR's HoverPoint
            if (drumstick != null)
            {       
                // get position and radius of drumstick
                Vector3 drumstickPosition = drumstick.transform.position;
                float drumstickRadius = drumstick.transform.lossyScale.x / 2f;

                // use SteamVR's method to detect if interactable component overlaps with drumstick size and position
                CheckHoveringForTransform(drumstickPosition, drumstickRadius, ref closestDistance, ref closestInteractable, Color.blue);
            }
            else
            {
                Debug.LogError("Drumsticks not detected. Defaulting to SteamVR hover component.");
                CheckHoveringForTransform(mainRenderModel.GetControllerPosition(controllerHoverComponent), scaledHoverRadius / 2f, ref closestDistance, ref closestInteractable, Color.blue);
            }            
        }

        if (useFingerJointHover && mainRenderModel != null && mainRenderModel.IsHandVisibile())
        {
            float scaledHoverRadius = fingerJointHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));
            CheckHoveringForTransform(mainRenderModel.GetBonePosition((int)fingerJointHover), scaledHoverRadius / 2f, ref closestDistance, ref closestInteractable, Color.yellow);
        }

        // Hover on this one
        hoveringInteractable = closestInteractable;
    }
    protected override void OnDrawGizmos()
    {
        if (useHoverSphere && hoverSphereTransform != null)
        {
            Gizmos.color = Color.green;
            float scaledHoverRadius = hoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(hoverSphereTransform));
            Gizmos.DrawWireSphere(hoverSphereTransform.position, scaledHoverRadius / 2);
        }

        if (useControllerHoverComponent && mainRenderModel != null && mainRenderModel.IsControllerVisibile())
        {
            // draw gizmo to verify that it matches the size and shape of the drumstick accurately
            if (drumstick != null)
            {
                Gizmos.color = Color.blue;
                Vector3 drumstickPosition = drumstick.transform.position;
                float drumstickSize = drumstick.transform.lossyScale.x;
                Gizmos.DrawWireSphere(drumstickPosition, drumstickSize / 2);
            }
            else
            {
                Gizmos.color = Color.blue;
                float scaledHoverRadius = controllerHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));
                Gizmos.DrawWireSphere(mainRenderModel.GetControllerPosition(controllerHoverComponent), scaledHoverRadius / 2);
            }            
        }

        if (useFingerJointHover && mainRenderModel != null && mainRenderModel.IsHandVisibile())
        {
            Gizmos.color = Color.yellow;
            float scaledHoverRadius = fingerJointHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));
            Gizmos.DrawWireSphere(mainRenderModel.GetBonePosition((int)fingerJointHover), scaledHoverRadius / 2);
        }
    }

}
