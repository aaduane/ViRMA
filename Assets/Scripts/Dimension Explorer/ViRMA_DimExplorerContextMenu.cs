using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorerContextMenu : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.dimExplorer.ToggleDimExFade(false);

            transform.parent.GetComponent<ViRMA_DimExplorerBtn>().contextMenuActiveOnBtn = false;

            Destroy(gameObject);
        }
    }

}
