using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class vr_inputModule : BaseInputModule
{
	public Camera pointerCamera;
	public SteamVR_Input_Sources pointerTargetSource;
	public SteamVR_Action_Boolean pointerClickAction;

	private GameObject pointerTargetObject = null;
	private PointerEventData pointerData = null;

	protected override void Awake()
	{
		Debug.Log("awake!");

		base.Awake();

		pointerData = new PointerEventData(eventSystem);
	}


	public PointerEventData getData()
	{
		Debug.Log("getData!");
		return pointerData;
	}

	private void processPress(PointerEventData data)
	{
		Debug.Log("processPress!");
	}

	private void processRelease(PointerEventData data)
	{
		Debug.Log("processRelease!");
	}

	//-------------------------------------------------
	public override void Process()
	{
		Debug.Log("process!");

		pointerData.Reset();
		pointerData.position = new Vector2(pointerCamera.pixelWidth / 2, pointerCamera.pixelHeight / 2);


		eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
		pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
		pointerTargetObject = pointerData.pointerCurrentRaycast.gameObject;


		m_RaycastResultCache.Clear();


		HandlePointerExitAndEnter(pointerData, pointerTargetObject);


		if (pointerClickAction.GetStateDown(pointerTargetSource))
		{
			processPress(pointerData);
		}


		if (pointerClickAction.GetStateUp(pointerTargetSource))
		{
			processRelease(pointerData);
		}

	}


}