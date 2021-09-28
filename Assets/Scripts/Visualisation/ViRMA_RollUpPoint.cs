using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_RollUpPoint : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Rigidbody axisRollUpRigidbody;
    public int axisId;

    [HideInInspector] public bool x;
    [HideInInspector] public bool y;
    [HideInInspector] public bool z;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        axisRollUpRigidbody = gameObject.AddComponent<Rigidbody>();
        axisRollUpRigidbody.isKinematic = true;
        axisRollUpRigidbody.useGravity = false;
    }

    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.vizController.focusedAxisPoint = gameObject;

            globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, true);
        }
    }

    private void OnTriggerStay(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.vizController.focusedAxisPoint = gameObject;

            globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, true);
        }
    }

    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            if (globals.vizController.focusedAxisPoint == gameObject)
            {
                globals.vizController.focusedAxisPoint = null;

                globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, false);
            }
        }
    }
}
