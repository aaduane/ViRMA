using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class ViRMA_UiElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
	// SteamVR: used for UI interaction with controller
	private ViRMA_GlobalsAndActions globals;

	public CustomEvents.UnityEventHand onHandClick;
	protected Hand currentHand;

	// used for custom UI interaction button states
	private Image btnBackground;
	private Text btnText;
	
	protected virtual void Awake()
	{
		globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

		btnBackground = GetComponent<Image>();
		btnText = GetComponentInChildren<Text>();

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

    private void Start()
    {
		// set default color of button
		SetKeyboardBtnNormalState();
	}


    // --- SteamVR: UI interaction with Hand script --- \\
    protected virtual void OnHandHoverBegin(Hand hand)
	{
		// assign howevered hand as current hand
		currentHand = hand;

		// trigger button initial hover state
		ViRMA_InputModule.instance.HoverBegin(gameObject);

		// trigger controller hint for UI interaction
		//ControllerButtonHints.ShowButtonHint(hand, globals.menuInteraction_Select); // not highlighting any button
	}
	protected virtual void OnHandHoverEnd(Hand hand)
	{
		// trigger button hover end state
		ViRMA_InputModule.instance.HoverEnd(gameObject);

		// hide controller hint for UI interaction
		//ControllerButtonHints.HideButtonHint(hand, globals.menuInteraction_Select); // not highlighting any button

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
			// ControllerButtonHints.HideButtonHint(hand, globals.menuInteraction_Select); // not highlighting any button
		}

		// force SteamVR UI hand interation states to match custom pointer UI interaction states
		if (hand.uiInteractAction.active && ViRMA_InputModule.instance.contactUIEnabled)
        {
			if (hand.uiInteractAction.stateDown)
			{
				SetKeyboardBtnDownState();
			}
			if (hand.uiInteractAction.stateUp)
			{
				SetKeyboardBtnNormalState();
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
		SetKeyboardBtnHighlightState();
	}
    public void OnPointerExit(PointerEventData eventData)
    {
		SetKeyboardBtnNormalState();
	}
    public void OnPointerUp(PointerEventData eventData)
    {
		// do nothing
	}
	public void OnPointerDown(PointerEventData eventData)
	{
		SetKeyboardBtnDownState();
	}
	public void OnPointerClick(PointerEventData eventData)
    {
		SetKeyboardBtnNormalState();
	}


	// button states
	public void SetKeyboardBtnNormalState()
    {
		if (btnText.gameObject.transform.parent.name == "CLEAR")
		{
			btnBackground.color = new Color32(192, 57, 43, 255);
			btnText.color = Color.white;
		}
		else if (btnText.gameObject.transform.parent.name == "DELETE")
		{
			btnBackground.color = new Color32(211, 84, 0, 255);
			btnText.color = Color.white;
		}
		else
		{
			btnBackground.color = globals.lightBlack;
			btnText.color = Color.white;
		}	
    }
	private void SetKeyboardBtnHighlightState()
    {
		//btnBackground.color = globals.BrightenColor(globals.lightBlack);
		//btnText.color = Color.white;

		btnBackground.color = globals.BrightenColor(btnBackground.color);
		btnText.color = globals.BrightenColor(btnText.color);
	}
	private void SetKeyboardBtnDownState()
    {
		//btnBackground.color = Color.white;
		//btnText.color = globals.lightBlack;

		Color32 originalBgColor = btnBackground.color;
		Color32 originalTextColor = btnText.color;

		btnBackground.color = originalTextColor;
		btnText.color = originalBgColor;
	}
}
