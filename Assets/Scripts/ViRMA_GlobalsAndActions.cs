using UnityEngine;
using Valve.VR;

public class ViRMA_GlobalsAndActions : MonoBehaviour
{
    // global scripts
    public ViRMA_VizController vizController;
    public ViRMA_QueryController queryController;

    // test actions
    public SteamVR_ActionSet testActions;
    public SteamVR_Action_Boolean actionClicked;

    private void Awake()
    {
        vizController = GameObject.Find("VisualisationController").GetComponent<ViRMA_VizController>();
        queryController = GameObject.Find("QueryController").GetComponent<ViRMA_QueryController>();

        testActions = SteamVR_Input.GetActionSet("TestActions");
        actionClicked = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("actionClicked");     
    }

    private void Start()
    {
        actionClicked[SteamVR_Input_Sources.Any].onStateDown += queryController.doTestActionNow;
        //testActions.Activate(SteamVR_Input_Sources.Any, 0, true);

        /*
        Debug.Log(testActionSet.allActions);
        foreach (var testAction in testActionSet.allActions)
        {
            Debug.Log(testAction.GetShortName());
        }     
        */
    }

}
