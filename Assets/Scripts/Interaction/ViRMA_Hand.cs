using System.Collections;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_Hand : Hand
{
    public GameObject handDrumstick;

    //public SphereCollider drumstickCollider;

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

            // substituted drumstick functionality
            if (handDrumstick != null)
            {
                
                Vector3 drumstickPosition = handDrumstick.transform.position;
                float drumstickSize = handDrumstick.transform.lossyScale.x;
                CheckHoveringForTransform(drumstickPosition, drumstickSize / 2f, ref closestDistance, ref closestInteractable, Color.blue);
                

                //CheckHoveringForTransform(drumstickCollider.transform.position, drumstickCollider.radius, ref closestDistance, ref closestInteractable, Color.blue);

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
            // substituted drumstick functionality
            if (handDrumstick != null)
            {
                Gizmos.color = Color.blue;
                Vector3 drumstickPosition = handDrumstick.transform.position;
                float drumstickSize = handDrumstick.transform.lossyScale.x;
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
