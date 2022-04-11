using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using System;

public class ViRMA_GlobalsAndActions : MonoBehaviour
{
    // global scripts
    public ViRMA_VizController vizController;
    public ViRMA_QueryController queryController;
    public ViRMA_DimExplorer dimExplorer;
    public ViRMA_MainMenu mainMenu;
    public ViRMA_Timeline timeline;

    // --- SteamVR action sets --- \\

    // Player hand/controller appearance
    public bool disableAllButtonHints = false;
    public bool rightControllerLoaded = false;
    public bool leftControllerLoaded = false;
    public bool rightControllerFaded = false;
    public bool leftControllerFaded = false;
    public Material controllerFadedMaterial;
    public Material leftControllerNormalMaterial;
    public Material rightControllerNormalMaterial;

    // default actions
    public SteamVR_ActionSet defaultActions;

    // test actions
    public SteamVR_ActionSet menuInteractionActions;
    public SteamVR_Action_Boolean menuInteraction_Select;
    public SteamVR_Action_Boolean menuInteraction_MenuControl;
    public SteamVR_Action_Vector2 menuInteraction_Scroll;

    // viz actions
    public SteamVR_ActionSet vizNavActions;
    public SteamVR_Action_Boolean vizNav_Position;
    public SteamVR_Action_Boolean vizNav_Rotation;
    public SteamVR_Action_Boolean vizNav_Select;
    //public SteamVR_Action_Single vizNav_HardGrip;

    // dimExplorer actions
    public SteamVR_ActionSet dimExplorerActions;
    public SteamVR_Action_Boolean dimExplorer_Select;
    public SteamVR_Action_Boolean dimExplorer_Scroll;

    // timeline actions
    public SteamVR_ActionSet timelineActions;
    public SteamVR_Action_Boolean timeline_Select;
    public SteamVR_Action_Boolean timeline_Scroll;
    public SteamVR_Action_Boolean timeline_Back;

    private void Awake()
    {
        // assign all global scripts
        vizController = GameObject.Find("VisualisationController").GetComponent<ViRMA_VizController>();
        queryController = GameObject.Find("QueryController").GetComponent<ViRMA_QueryController>();
        dimExplorer = GameObject.Find("DimensionExplorer").GetComponent<ViRMA_DimExplorer>();
        mainMenu = GameObject.Find("MainMenu").GetComponent<ViRMA_MainMenu>();
        timeline = GameObject.Find("Timeline").GetComponent<ViRMA_Timeline>();

        // assign all action sets
        AssignAllActionSets();

        // assign specific actions to functionality in ViRMA scripts
        AssignAllCustomActions();
    }
    private void Update()
    {
        // SteamVR controller models take some frames to load so this waits for them to set some globals
        InitialiseSteamVRControllers();

        // control activation of SteamVR actions
        ActionActivityController();

        // keyboard functions
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
        if (Input.GetKey("p"))
        {
            Debug.Log("Screenshot taken: " + "C:/Users/Aaron Duane/Downloads/" + DateTime.Now.ToString("HH_mm_ss") + ".png");
            ScreenCapture.CaptureScreenshot("C:/Users/Aaron Duane/Downloads/" + DateTime.Now.ToString("HH_mm_ss") + ".png", 2);
            //ScreenCapture.CaptureScreenshot(System.IO.Directory.GetCurrentDirectory().ToString() + "/" + DateTime.Now.ToString("HH_mm_ss") + ".png", 2);
        }
    }

    // actions
    private void AssignAllActionSets()
    {
        // default action set 
        defaultActions = SteamVR_Input.GetActionSet("default");

        // ui interaction action set
        menuInteractionActions = SteamVR_Input.GetActionSet("MenuInteraction");
        menuInteraction_Select = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/MenuInteraction/in/Select");
        menuInteraction_MenuControl = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/MenuInteraction/in/MenuControl");
        menuInteraction_Scroll = SteamVR_Input.GetActionFromPath<SteamVR_Action_Vector2>("/actions/MenuInteraction/in/Scroll");

        // viz navigation action set
        vizNavActions = SteamVR_Input.GetActionSet("VizNavigation");
        vizNav_Position = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/VizNavigation/in/Position");
        vizNav_Rotation = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/VizNavigation/in/Rotation");
        vizNav_Select = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/VizNavigation/in/Select");
        //vizNav_HardGrip = SteamVR_Input.GetActionFromPath<SteamVR_Action_Single>("/actions/VizNavigation/in/HardGrip");

        // dimension explorer action set
        dimExplorerActions = SteamVR_Input.GetActionSet("DimExplorer");
        dimExplorer_Select = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/DimExplorer/in/Select");
        dimExplorer_Scroll = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/DimExplorer/in/Scroll");

        // timeline action set
        timelineActions = SteamVR_Input.GetActionSet("Timeline");
        timeline_Select = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/Timeline/in/Select");
        timeline_Scroll = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/Timeline/in/Scroll");
        timeline_Back = SteamVR_Input.GetActionFromPath<SteamVR_Action_Boolean>("/actions/Timeline/in/Back");
    }
    private void AssignAllCustomActions()
    {
        // --- SteamVR custom action assignments --- \\

        // menu interaction
        menuInteraction_MenuControl[SteamVR_Input_Sources.Any].onStateDown += mainMenu.ToggleMainMenuAlias;

        // viz controller
        vizNav_Select[SteamVR_Input_Sources.Any].onStateDown += vizController.SubmitCellForTimeline;
        vizNav_Select[SteamVR_Input_Sources.Any].onStateDown += vizController.DrillDownRollUp;
        
        // dimension explorer 
        dimExplorer_Scroll[SteamVR_Input_Sources.Any].onStateDown += dimExplorer.SubmitTagForTraversal;
        dimExplorer_Select[SteamVR_Input_Sources.Any].onStateDown += dimExplorer.SubmitTagForContextMenu;
        dimExplorer_Select[SteamVR_Input_Sources.Any].onStateDown += dimExplorer.SubmitContextBtnForQuery;

        //timeline explorer 
        timeline_Select[SteamVR_Input_Sources.Any].onStateDown += timeline.SubmitChildForContextMenu;
        timeline_Select[SteamVR_Input_Sources.Any].onStateDown += timeline.SubmitContextMenuBtn;
        timeline_Back[SteamVR_Input_Sources.Any].onStateDown += timeline.BackButton;

        // testing
        //menuInteraction_Scroll[SteamVR_Input_Sources.Any].onAxis += mainMenu.TestScroll;
        //vizNav_HardGrip[SteamVR_Input_Sources.Any].onAxis += TestGripAction;
    }
    public void ToggleOnlyThisActionSet(SteamVR_ActionSet targetActionSet)
    {
        SteamVR_ActionSet_Manager.DisableAllActionSets();
        defaultActions.Activate();
        targetActionSet.Activate();
    }
    private void ActionActivityController()
    {
        
        if (dimExplorer.dimensionExpLorerLoaded)
        {
            dimExplorerActions.Activate();
            vizNavActions.Deactivate();
        }
        else if (vizController.vizFullyLoaded)
        {
            vizNavActions.Activate();
            dimExplorerActions.Deactivate();
        }


        if (dimExplorer.dimExKeyboard.keyboardLoaded)
        {
            dimExplorerActions.Activate();
            vizNavActions.Deactivate();
        }


        if (dimExplorer.dimExKeyboard.keyboardMoving)
        {   
            dimExplorerActions.Deactivate();
            vizNavActions.Deactivate();
        }


        if (timeline.timelineLoaded)
        {
            timelineActions.Activate();    
        }
        else
        {
            timelineActions.Deactivate();
        }

        menuInteractionActions.Activate();


        // debugging
        if (false)
        {
            //Debug.Log("dimensionExpLorerLoaded: " + dimExplorer.dimensionExpLorerLoaded + " | vizFullyLoaded: " + vizController.vizFullyLoaded + " | keyboardLoaded: " + dimExplorer.dimExKeyboard.keyboardLoaded + " | keyboardMoving: " + dimExplorer.dimExKeyboard.keyboardMoving);

            string activeSetDebug = "Active Sets:";
            if (defaultActions.IsActive())
            {
                activeSetDebug += " | default";
            }
            if (vizNavActions.IsActive())
            {
                activeSetDebug += " | viz";
            }
            if (dimExplorerActions.IsActive())
            {
                activeSetDebug += " | dimEx";
            }
            if (menuInteractionActions.IsActive())
            {
                activeSetDebug += " | menu";
            }
            if (timelineActions.IsActive())
            {
                activeSetDebug += " | timeline";
            }
            Debug.Log(activeSetDebug);
        }    
    }

    // controller appearance
    public void InitialiseSteamVRControllers()
    {
        GameObject drumstickPrefab = Resources.Load("Prefabs/Drumstick") as GameObject;

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
                        GameObject drumstick = Instantiate(drumstickPrefab, controller.transform);                  
                        drumstick.transform.localPosition = new Vector3(0, 0, 0.04f);
                        drumstick.GetComponent<ViRMA_Drumstick>().hand = Player.instance.rightHand;
                        drumstick.name = "RightHandDrumstick";
                        Player.instance.rightHand.gameObject.GetComponent<ViRMA_Hand>().drumstick = drumstick;
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
                        GameObject drumstick = Instantiate(drumstickPrefab, controller.transform);
                        drumstick.transform.localPosition = new Vector3(0, 0, 0.04f);
                        drumstick.GetComponent<ViRMA_Drumstick>().hand = Player.instance.leftHand;
                        drumstick.name = "LeftHandDrumstick";
                        Player.instance.leftHand.gameObject.GetComponent<ViRMA_Hand>().drumstick = drumstick;
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
                if (rend.transform.parent.name == "controller(Clone)" && !rend.gameObject.GetComponent<ViRMA_Drumstick>())
                {
                    if (toFade)
                    {
                        if (hand.handType.ToString() == "RightHand")
                        {
                            rend.material = controllerFadedMaterial;
                            rightControllerFaded = true;
                        }
                        if (hand.handType.ToString() == "LeftHand")
                        {
                            rend.material = controllerFadedMaterial;
                            leftControllerFaded = true;
                        }
                    }
                    else
                    {
                        if (hand.handType.ToString() == "RightHand")
                        {
                            rend.material = rightControllerNormalMaterial;
                            rightControllerFaded = false;
                        }
                        if (hand.handType.ToString() == "LeftHand")
                        {
                            rend.material = leftControllerNormalMaterial;
                            leftControllerFaded = false;
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
    private void TestAction(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Debug.Log(action.GetShortName() + " | " + source);
    }
    public void TestGripAction(SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta)
    {
        if (newAxis > 0.9f)
        {
            Debug.Log(newAxis);
        }
    }

}
