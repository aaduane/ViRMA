using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_InputModule : BaseInputModule
{
	public bool pointerUIEnabled;
	public bool contactUIEnabled;

	public Camera pointerCamera;
	public SteamVR_Input_Sources pointerTargetSource;
	public SteamVR_Action_Boolean pointerClickAction;
	
	private GameObject pointerTargetObject = null;
	private PointerEventData pointerData = null;

    protected override void Awake()
    {
        base.Awake();

		pointerData = new PointerEventData(eventSystem);

		pointerUIEnabled = false;
		contactUIEnabled = true;
	}

	// --- SteamVR: UI interaction with Hand script --- \\
	private GameObject submitObject;
	private static ViRMA_InputModule _instance;
	public static ViRMA_InputModule instance
	{
		get
		{
			if (_instance == null)
				_instance = GameObject.FindObjectOfType<ViRMA_InputModule>();

			return _instance;
		}
	}
	public override bool ShouldActivateModule()
	{
		if (!base.ShouldActivateModule())
			return false;

		return submitObject != null;
	}
	public void HoverBegin(GameObject gameObject)
	{
		if (contactUIEnabled)
        {
			PointerEventData pointerEventData = new PointerEventData(eventSystem);
			ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.pointerEnterHandler);
		}		
	}
	public void HoverEnd(GameObject gameObject)
	{
		if (contactUIEnabled)
        {
			PointerEventData pointerEventData = new PointerEventData(eventSystem);
			pointerEventData.selectedObject = null;
			ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.pointerExitHandler);
		}		
	}
	public void Submit(GameObject gameObject)
	{
		if (contactUIEnabled)
        {
			submitObject = gameObject;
		}	
	}


	// --- custom UI interaction for pointer and SteamVR hand --- \\
	public PointerEventData GetData()
	{
		return pointerData;
	}
	private void processPress(PointerEventData data)
	{
		// set raycast
		data.pointerPressRaycast = data.pointerCurrentRaycast;

		// check for object hit, get the down handler, call it
		GameObject newPointerPress = ExecuteEvents.ExecuteHierarchy(pointerTargetObject, data, ExecuteEvents.pointerDownHandler);

		// if no down handler, try and get click handler
		if (newPointerPress == null)
		{
			newPointerPress = ExecuteEvents.GetEventHandler<IPointerEnterHandler>(pointerTargetObject);
		}

		// set data
		data.pressPosition = data.position;
		data.pointerPress = newPointerPress;
		data.rawPointerPress = pointerTargetObject;
	}
	private void processRelease(PointerEventData data)
	{
		// execute process up
		ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

		// check for click handler
		GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(pointerTargetObject);

		// check if actual
		if (data.pointerPress == pointerUpHandler)
		{
			ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerClickHandler);
		}

		// clear selected gameobject
		eventSystem.SetSelectedGameObject(null);

		// reset data
		data.pressPosition = Vector2.zero;
		data.pointerPress = null;
		data.rawPointerPress = null;
	}
	public override void Process()
	{
		if (contactUIEnabled)
		{
			if (submitObject)
			{
				BaseEventData data = GetBaseEventData();
				data.selectedObject = submitObject;
				ExecuteEvents.Execute(submitObject, data, ExecuteEvents.submitHandler);

				submitObject = null;
			}
		}

		if (pointerUIEnabled)
		{
			// reset data
			pointerData.Reset();

			// set camera
			pointerData.position = new Vector2(pointerCamera.pixelWidth / 2, pointerCamera.pixelHeight / 2);

			// raycast
			eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
			pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
			pointerTargetObject = pointerData.pointerCurrentRaycast.gameObject;
			m_RaycastResultCache.Clear();

			// hover
			HandlePointerExitAndEnter(pointerData, pointerTargetObject);

			// press
			if (pointerClickAction.GetStateDown(pointerTargetSource))
			{
				processPress(pointerData);
			}

			// release
			if (pointerClickAction.GetStateUp(pointerTargetSource))
			{
				processRelease(pointerData);
			}
		}
	}
}