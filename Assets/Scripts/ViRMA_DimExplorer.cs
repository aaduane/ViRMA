using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorer : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    public void positionDimExplorer(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
        flattenedVector.y = 0;
        flattenedVector.Normalize();
        Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.5f;
        transform.position = spawnPos;
        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);

        globals.vizNavActions.Deactivate();

        foreach (Hand hand in Player.instance.hands)
        {
            //ControllerButtonHints.ShowButtonHint(hand, globals.actionClicked);
        }
        
    }


}
