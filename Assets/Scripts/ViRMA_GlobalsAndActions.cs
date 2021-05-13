using System.Collections;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_GlobalsAndActions : MonoBehaviour
{
    // global scripts
    public ViRMA_VizController vizController;
    public ViRMA_QueryController queryController;
    public ViRMA_DimExplorer dimExplorer;

    // --- SteamVR action sets --- \\

    // default actions
    public SteamVR_ActionSet defaultAction;

    // test actions
    public SteamVR_ActionSet menuInteractionActions;
    public SteamVR_Action_Boolean menuInteraction_Select;
    public SteamVR_Action_Boolean menuInteraction_MenuControl;

    // viz actions
    public SteamVR_ActionSet vizNavActions;
    public SteamVR_Action_Boolean vizNav_Position;
    public SteamVR_Action_Boolean vizNav_Rotation;   

    private void Awake()
    {
        // assign all global scripts
        vizController = GameObject.Find("VisualisationController").GetComponent<ViRMA_VizController>();
        queryController = GameObject.Find("QueryController").GetComponent<ViRMA_QueryController>();
        dimExplorer = GameObject.Find("DimensionExplorer").GetComponent<ViRMA_DimExplorer>();

        // assign all action sets
        AssignAllActionSets();

        // assign specific actions to functionality in ViRMA scripts
        AssignAllActions();

        // this is only used during testing
        ActiveDevelopmentTesting();
    }

    private void Update()
    {
        ToggleControllerAppearance();
    }

    private void AssignAllActionSets()
    {
        // default action set 
        defaultAction = SteamVR_Input.GetActionSet("default");

        // ui interaction action set
        menuInteractionActions = SteamVR_Input.GetActionSet("MenuInteraction");
        menuInteraction_Select = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Select");
        menuInteraction_MenuControl = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("MenuControl");

        // viz navigation action set
        vizNavActions = SteamVR_Input.GetActionSet("VizNavigation");
        vizNav_Position = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Position");
        vizNav_Rotation = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Rotation");


        /*
        Debug.Log(testActionSet.allActions);
        foreach (var testAction in testActionSet.allActions)
        {
            Debug.Log(testAction.GetShortName());
        }     
        */
    }
    private void AssignAllActions()
    {
        // ui interaction actions
        menuInteraction_MenuControl[SteamVR_Input_Sources.Any].onStateDown += dimExplorer.positionDimExplorer;
    }
    public void ToggleActionSet(SteamVR_ActionSet targetActionSet, bool onOff)
    {
        SteamVR_ActionSet_Manager.DisableAllActionSets();
        defaultAction.Activate();
        if (onOff)
        {
            targetActionSet.Activate();
        }
        else
        {
            targetActionSet.Deactivate();
        }

        /*
        SteamVR_ActionSet[] actionSets = SteamVR_Input.GetActionSets();
        string activeActionSets = "";
        foreach (var actionSet in actionSets)
        {
            if (actionSet.IsActive())
            {
                activeActionSets += actionSet.GetShortName() + " | ";
            }
        }
        Debug.Log(activeActionSets);
        */
    }
    private void ToggleControllerAppearance()
    {
        foreach (Hand hand in Player.instance.hands)
        {
            hand.HideSkeleton();
            hand.ShowController();
            //ControllerButtonHints.HideAllButtonHints(hand);
            //ControllerButtonHints.HideAllTextHints(hand);
        }
    }

    private void ActiveDevelopmentTesting()
    {
        queryController.gameObject.SetActive(false);
        //menuInteractionActions.Activate();



    }

}
