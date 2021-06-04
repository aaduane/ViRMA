using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class ViRMA_UiElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
	// SteamVR: used for UI interaction with controller
	public CustomEvents.UnityEventHand onHandClick;
	protected Hand currentHand;

	// used for custom UI interaction button states
	Image image;
	private Color32 normalColor = Color.white;
	private Color32 hoverColor = Color.grey;
	private Color32 downColor = Color.green;
	

	protected virtual void Awake()
	{
		// set default color of button
		image = GetComponent<Image>();
		if (image)
        {
			image.color = normalColor;
		}

		// SteamVR: assign function to button when it is clicked by hand script
		Button button = GetComponent<Button>();
		if (button)
		{
			button.onClick.AddListener(OnButtonClick);

			Navigation disableNav = new Navigation();
			disableNav.mode = Navigation.Mode.None;
			button.navigation = disableNav;

			button.transition = Selectable.Transition.None;
		}
	}


	// --- SteamVR: UI interaction with Hand script --- \\
	protected virtual void OnHandHoverBegin(Hand hand)
	{
		// assign howevered hand as current hand
		currentHand = hand;

		// trigger button initial hover state
		ViRMA_InputModule.instance.HoverBegin(gameObject);

		// trigger controller hint for UI interaction
		ControllerButtonHints.ShowButtonHint(hand, hand.uiInteractAction);
	}
	protected virtual void OnHandHoverEnd(Hand hand)
	{
		// trigger button hover end state
		ViRMA_InputModule.instance.HoverEnd(gameObject);

		// hide controller hint for UI interaction
		ControllerButtonHints.HideButtonHint(hand, hand.uiInteractAction);

		// clear current hand status
		currentHand = null;
	}
	protected virtual void HandHoverUpdate(Hand hand)
	{
		if (hand.uiInteractAction != null && hand.uiInteractAction.GetStateDown(hand.handType))
		{
			// SteamVR: submit button to be invoked
			ViRMA_InputModule.instance.Submit(gameObject);

			// SteamVR: hide controller hint
			ControllerButtonHints.HideButtonHint(hand, hand.uiInteractAction);
		}

		// force SteamVR UI hand interation states to match custom pointer UI interaction states
		if (hand.uiInteractAction.active && ViRMA_InputModule.instance.contactUIEnabled)
        {
			if (hand.uiInteractAction.stateDown)
			{
				image.color = downColor;
			}
			if (hand.uiInteractAction.stateUp)
			{
				image.color = normalColor;
			}
		}

		
	}
	protected virtual void OnButtonClick()
	{
		onHandClick.Invoke(currentHand);
	}


	// --- custom interaction states for pointer and SteamVR hand --- \\
    public void OnPointerEnter(PointerEventData eventData)
    {
		image.color = hoverColor;
	}
    public void OnPointerExit(PointerEventData eventData)
    {
		image.color = normalColor;
	}
    public void OnPointerUp(PointerEventData eventData)
    {
		// do nothing
	}
	public void OnPointerDown(PointerEventData eventData)
	{
		image.color = downColor;
	}
	public void OnPointerClick(PointerEventData eventData)
    {
		image.color = normalColor;
	}
}
