using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViRMA_DimExplorerContextMenu : MonoBehaviour
{
    GameObject targetDimExBtn;

    private void LoadContextMenu(GameObject submittedDimExBtn)
    {
        targetDimExBtn = submittedDimExBtn;


    }

    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {

        }
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {

        }
    }

}
