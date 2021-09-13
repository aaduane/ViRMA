using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_GlobalsAndActions : MonoBehaviour
{
    // global scripts
    public ViRMA_VizController vizController;
    public ViRMA_QueryController queryController;
    public ViRMA_DimExplorer dimExplorer;
    public ViRMA_MainMenu mainMenu;

    // colours
    public Color32 axisRed = new Color32(192, 57, 43, 255);
    public Color32 axisGreen = new Color32(39, 174, 96, 255);
    public Color32 axisBlue = new Color32(35, 99, 142, 255); 
    public Color32 lightBlack = new Color32(52, 73, 94, 255);
    public Color32 lightBlue = new Color32(52, 152, 219, 255);
    public Color32 BrightenColor(Color32 colorToBrighten)
    {
        float H, S, V;
        Color.RGBToHSV(colorToBrighten, out H, out S, out V);
        Color32 brighterColor = Color.HSVToRGB(H, S * 0.70f, V / 0.70f);
        return brighterColor;
    }
    public Color32 DarkenColor(Color32 colorToDarken)
    {
        float H, S, V;
        Color.RGBToHSV(colorToDarken, out H, out S, out V);
        Color32 darkerColor = Color.HSVToRGB(H, S / 0.70f, V * 0.70f);
        return darkerColor;
    }

    // --- SteamVR action sets --- \\

    // Player hand/controller appearance

    public bool rightControllerLoaded = false;
    public bool leftControllerLoaded = false;
    public bool disableAllButtonHints = false;
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
        mainMenu = GameObject.Find("MainMenu").GetComponent<ViRMA_MainMenu>();

        // assign all action sets
        AssignAllActionSets();

        // assign specific actions to functionality in ViRMA scripts
        AssignAllCustomActions();

        // this is only used during testing
        ActiveDevelopmentTesting();
    }
    private void Update()
    {
        /*
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
        */

        // SteamVR controller models take some frames to load so this waits for them to set some globals
        InitialiseSteamVRControllers();

        // control activation of SteamVR actions
        ActionActivityController();
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
        // --- SteamVR custom action assignments --- \\

        // dimension explorer 
        dimExplorer_Scroll[SteamVR_Input_Sources.Any].onStateDown += dimExplorer.SubmitTagForTraversal;
        dimExplorer_Select[SteamVR_Input_Sources.Any].onStateDown += dimExplorer.SubmitTagForContextMenu;
        dimExplorer_Select[SteamVR_Input_Sources.Any].onStateDown += dimExplorer.SubmitContextBtnForQuery;

        //menuInteraction_Scroll[SteamVR_Input_Sources.Any].onAxis += mainMenu.TestScroll;
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
            menuInteractionActions.Activate();
            dimExplorerActions.Activate();
            vizNavActions.Deactivate();
        }

        if (dimExplorer.dimExKeyboard.keyboardMoving)
        {
            menuInteractionActions.Activate();
            dimExplorerActions.Deactivate();
            vizNavActions.Deactivate();
        }

        // debugging
        if (false)
        {
            string activeSetDebug = "Active Sets:";
            if (defaultActions.IsActive())
            {
                activeSetDebug += " | default";
            }
            if (menuInteractionActions.IsActive())
            {
                activeSetDebug += " | menu interaction";
            }
            if (vizNavActions.IsActive())
            {
                activeSetDebug += " | viz nav";
            }
            if (dimExplorerActions.IsActive())
            {
                activeSetDebug += " | dimension explorer nav";
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
        //vizController.gameObject.SetActive(true);
        //queryController.gameObject.SetActive(true);
    }
    private void TestAction(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Debug.Log(action.GetShortName() + " | " + source);
    }

}
