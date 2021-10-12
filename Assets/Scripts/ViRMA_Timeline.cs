using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_Timeline : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Cell timelineCellData;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    public void LoadTimeline(GameObject submittedCell)
    {
        Debug.Log("Loading timeline... ");

        // api/cell/?filters=[{"type":"tag","ids":["147","132"]},{"type":"node","ids":["699"]},{"type":"tag","ids":["17"]}]&all=[]

        if (submittedCell.GetComponent<ViRMA_Cell>())
        {
            timelineCellData = submittedCell.GetComponent<ViRMA_Cell>().thisCellData;

            // globals.vizController.activeAxesLabels 
        }

    }

}
