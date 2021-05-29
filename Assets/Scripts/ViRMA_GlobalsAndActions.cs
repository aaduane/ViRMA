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
        //menuInteraction_MenuControl[SteamVR_Input_Sources.Any].onStateDown += dimExplorer.positionDimExplorer; 
        menuInteraction_MenuControl[SteamVR_Input_Sources.Any].onStateDown += Foo;
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
        if (!rightControllerLoaded)
        {
            if (Player.instance.rightHand.mainRenderModel)
            {
                if (Player.instance.rightHand.mainRenderModel.transform.Find("controller(Clone)"))
                {
                    GameObject controller = Player.instance.rightHand.mainRenderModel.transform.Find("controller(Clone)").gameObject;
                    if (controller.transform.Find("body"))
                    {
                        rightControllerLoaded = true;
                        Player.instance.rightHand.HideSkeleton();
                        Player.instance.rightHand.ShowController();

                        GameObject steamVRControllerBody = Player.instance.rightHand.mainRenderModel.transform.Find("controller(Clone)").Find("body").gameObject;
                        Renderer controllerRend = steamVRControllerBody.GetComponent<Renderer>();
                        rightControllerNormalMaterial = new Material(controllerRend.material);

                        // add drumstick appendage to controller
                        GameObject drumstick = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        drumstick.name = "RightHandDrumstick";
                        drumstick.transform.SetParent(controller.transform);
                        drumstick.transform.localScale = Vector3.one * 0.05f;
                        drumstick.transform.localPosition = new Vector3(0, 0, 0.05f);

                        drumstick.GetComponent<Renderer>().material.color = Color.red;

                        drumstick.AddComponent<ViRMA_Drumstick>().hand = Player.instance.rightHand;
                        Player.instance.rightHand.gameObject.GetComponent<ViRMA_Hand>().handDrumstick = drumstick;
                    }
                }
            }
        }

        if (!leftControllerLoaded)
        {
            if (Player.instance.leftHand.mainRenderModel)
            {
                if (Player.instance.leftHand.mainRenderModel.transform.Find("controller(Clone)"))
                {
                    GameObject controller = Player.instance.leftHand.mainRenderModel.transform.Find("controller(Clone)").gameObject;
                    if (controller.transform.Find("body"))
                    {
                        leftControllerLoaded = true;
                        Player.instance.leftHand.HideSkeleton();
                        Player.instance.leftHand.ShowController();

                        GameObject steamVRControllerBody = Player.instance.leftHand.mainRenderModel.transform.Find("controller(Clone)").Find("body").gameObject;
                        Renderer controllerRend = steamVRControllerBody.GetComponent<Renderer>();
                        leftControllerNormalMaterial = new Material(controllerRend.material);

                        // add drumstick appendage to controller
                        GameObject drumstick = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        drumstick.name = "LeftHandDrumstick";
                        drumstick.transform.SetParent(controller.transform);
                        drumstick.transform.localScale = Vector3.one * 0.05f;
                        drumstick.transform.localPosition = new Vector3(0, 0, 0.05f);

                        drumstick.GetComponent<Renderer>().material.color = Color.red;

                        drumstick.AddComponent<ViRMA_Drumstick>().hand = Player.instance.leftHand;
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
        vizController.gameObject.SetActive(false);
        queryController.gameObject.SetActive(true);

        menuInteractionActions.Activate();
    }
    private void Foo(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Debug.Log(action.GetShortName() + " | " + source);
    }

}
