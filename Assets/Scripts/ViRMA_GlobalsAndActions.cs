using System;
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

    // colours
    public Color flatRed = new Color32(192, 57, 43, 255);
    public Color flatGreen = new Color32(39, 174, 96, 255);
    public Color flatLightBlue = new Color32(52, 152, 219, 255);
    public Color flatDarkBlue = new Color32(35, 99, 142, 255);

    // --- SteamVR action sets --- \\

    // Player hand/controller appearance

    public bool rightControllerLoaded = false;
    public bool leftControllerLoaded = false;
    public bool disableAllButtonHints = false;
    public Material controllerFadedMaterial;
    public Material leftControllerNormalMaterial;
    public Material rightControllerNormalMaterial;

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

    // dimExplorer actions
    public SteamVR_ActionSet dimExplorerActions;
    public SteamVR_Action_Boolean dimExplorer_Select;
    public SteamVR_Action_Boolean dimExplorer_Scroll;

    private void Awake()
    {
        // assign all global scripts
        vizController = GameObject.Find("VisualisationController").GetComponent<ViRMA_VizController>();
        queryController = GameObject.Find("QueryController").GetComponent<ViRMA_QueryController>();
        dimExplorer = GameObject.Find("DimensionExplorer").GetComponent<ViRMA_DimExplorer>();

        // assign all action sets
        AssignAllActionSets();

        // assign specific actions to functionality in ViRMA scripts
        AssignAllCustomActions();

        // this is only used during testing
        ActiveDevelopmentTesting();
    }
    private void Update()
    {
        // SteamVR controller models take some frames to load so this waits for them to set some globals
        InitialiseSteamVRControllers();
    }

    // actions
    private void AssignAllActionSets()
    {
        // default action set 
        defaultAction = SteamVR_Input.GetActionSet("default");

        // ui interaction action set
        menuInteractionActions = SteamVR_Input.GetActionSet("MenuInteraction");
        menuInteraction_Select = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/MenuInteraction/in/Select");
        menuInteraction_MenuControl = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("/actions/MenuInteraction/in/MenuControl");

        // viz navigation action set
        vizNavActions = SteamVR_Input.GetActionSet("VizNavigation");
        vizNav_Position = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("/actions/VizNavigation/in/Position");
        vizNav_Rotation = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("/actions/VizNavigation/in/Rotation");

        // dimension explorer action set
        dimExplorerActions = SteamVR_Input.GetActionSet("DimExplorer");
        dimExplorer_Select = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/DimExplorer/in/Select");
        dimExplorer_Scroll = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/DimExplorer/in/Scroll");

        /*
        Debug.Log(dimExplorerActions.allActions);
        foreach (var action in dimExplorerActions.allActions)
        {
            Debug.Log(action.GetShortName());
        }     
        */
    }
    private void AssignAllCustomActions()
    {
        // ui interaction actions
        //menuInteraction_MenuControl[SteamVR_Input_Sources.Any].onStateDown += TestAction;
        dimExplorer_Select[SteamVR_Input_Sources.Any].onStateDown += dimExplorer.SubmitTagForTraversal;
    }
    public void ToggleOnlyThisActionSet(SteamVR_ActionSet targetActionSet)
    {
        SteamVR_ActionSet_Manager.DisableAllActionSets();
        defaultAction.Activate();
        targetActionSet.Activate();
    }

    // controller appearance
    public void InitialiseSteamVRControllers()
    {
        // right controller
        if (!rightControllerLoaded)
        {
            if (Player.instance.rightHand.mainRenderModel)
            {
                if (Player.instance.rightHand.mainRenderModel.transform.Find("controller(Clone)"))
                {
                    GameObject controller = Player.instance.rightHand.mainRenderModel.transform.Find("controller(Clone)").gameObject;
                    if (controller.transform.Find("body"))
                    {
                        // set controller initial appearance
                        rightControllerLoaded = true;
                        Player.instance.rightHand.HideSkeleton();
                        Player.instance.rightHand.ShowController();

                        // save copy of material for fade in and out later
                        GameObject steamVRControllerBody = Player.instance.rightHand.mainRenderModel.transform.Find("controller(Clone)").Find("body").gameObject;
                        Renderer controllerRend = steamVRControllerBody.GetComponent<Renderer>();
                        rightControllerNormalMaterial = new Material(controllerRend.material);

                        // add 'drumstick' to controller for Ui interaction
                        GameObject drumstick = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        drumstick.name = "RightHandDrumstick";
                        drumstick.transform.SetParent(controller.transform);
                        drumstick.transform.localScale = Vector3.one * 0.05f;
                        drumstick.transform.localPosition = new Vector3(0, 0, 0.05f);
                        drumstick.AddComponent<ViRMA_Drumstick>().hand = Player.instance.rightHand;

                        // set drumstick in ViRMA Hand
                        Player.instance.rightHand.gameObject.GetComponent<ViRMA_Hand>().handDrumstick = drumstick;
                    }
                }
            }
        }

        // left controller
        if (!leftControllerLoaded)
        {
            if (Player.instance.leftHand.mainRenderModel)
            {
                if (Player.instance.leftHand.mainRenderModel.transform.Find("controller(Clone)"))
                {
                    GameObject controller = Player.instance.leftHand.mainRenderModel.transform.Find("controller(Clone)").gameObject;
                    if (controller.transform.Find("body"))
                    {
                        // set controller initial appearance
                        leftControllerLoaded = true;
                        Player.instance.leftHand.HideSkeleton();
                        Player.instance.leftHand.ShowController();

                        // save copy of material for fade in and out later
                        GameObject steamVRControllerBody = Player.instance.leftHand.mainRenderModel.transform.Find("controller(Clone)").Find("body").gameObject;
                        Renderer controllerRend = steamVRControllerBody.GetComponent<Renderer>();
                        leftControllerNormalMaterial = new Material(controllerRend.material);

                        // add 'drumstick' to controller for Ui interaction
                        GameObject drumstick = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        drumstick.name = "LeftHandDrumstick";
                        drumstick.transform.SetParent(controller.transform);
                        drumstick.transform.localScale = Vector3.one * 0.05f;
                        drumstick.transform.localPosition = new Vector3(0, 0, 0.05f);
                        drumstick.AddComponent<ViRMA_Drumstick>().hand = Player.instance.leftHand;

                        // set drumstick in ViRMA Hand
                        Player.instance.leftHand.gameObject.GetComponent<ViRMA_Hand>().handDrumstick = drumstick;
                    }
                }
            }
        }
    }
    public void ToggleControllerFade(Hand hand, bool toFade)
    {
        if (hand.mainRenderModel)
        {
            Renderer[] renderers = hand.mainRenderModel.GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                if (rend.transform.parent.name == "controller(Clone)")
                {
                    if (toFade)
                    {
                        rend.material = controllerFadedMaterial;
                    }
                    else
                    {
                        if (hand.handType.ToString() == "RightHand")
                        {
                            rend.material = rightControllerNormalMaterial;
                        }
                        if (hand.handType.ToString() == "LeftHand")
                        {
                            rend.material = leftControllerNormalMaterial;
                        }
                    }
                }
            }
        }
    }
    private void HideAllButtonHints()
    {
        foreach (Hand hand in Player.instance.hands)
        {
            if (disableAllButtonHints)
            {
                ControllerButtonHints.HideAllButtonHints(hand);
                ControllerButtonHints.HideAllTextHints(hand);
            }
        }
    }

    // testing
    private void ActiveDevelopmentTesting()
    {
        vizController.gameObject.SetActive(true);

        queryController.gameObject.SetActive(true);

        ToggleOnlyThisActionSet(dimExplorerActions);
    }
    private void TestAction(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Debug.Log(action.GetShortName() + " | " + source);
    }

}
