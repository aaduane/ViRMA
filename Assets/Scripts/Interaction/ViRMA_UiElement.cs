using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class ViRMA_UiElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
	// SteamVR: used for UI interaction with controller
	private ViRMA_GlobalsAndActions globals;
	private ViRMA_Keyboard keyboard;
	private ViRMA_MainMenu mainMenu;

	public CustomEvents.UnityEventHand onHandClick;
	protected Hand currentHand;

	// used for custom UI interaction button states
	private Image btnBackground;
	private Text btnText;
	private RawImage btnIcon;

	public Color normalBackgroundColor;
	public Color normalTextColor;

	public bool buttonFaded;

	protected virtual void Awake()
	{
		globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
		btnBackground = GetComponent<Image>();
		btnText = GetComponentInChildren<Text>();
		if (GetComponentInChildren<RawImage>())
        {
			btnIcon = GetComponentInChildren<RawImage>();

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

		// find out if UI element is part of a ViRMA Keyboard
		Transform checkKeyboard = transform;
		while (checkKeyboard.parent != null)
        {
			if (checkKeyboard.parent.GetComponent<ViRMA_Keyboard>())
            {
				keyboard = checkKeyboard.parent.GetComponent<ViRMA_Keyboard>();
				break;
			}
			checkKeyboard = checkKeyboard.parent.transform;
        }

		// find out if UI element is part of a main menu
		/*
		Transform checkMainMenu = transform;
		while (checkMainMenu.parent != null)
		{
			if (checkMainMenu.parent.GetComponent<ViRMA_MainMenu>())
			{
				mainMenu = checkMainMenu.parent.GetComponent<ViRMA_MainMenu>();
				break;
			}
			checkMainMenu = checkMainMenu.parent.transform;
		}
		*/
	}

    private void Start()
    {
		// set correct box collider size for UI interactions
		SetKeyColliderSize();
	}

    private void Update()
    {
		// override all button stats when button is faded
		BtnFadeController();
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
			if (keyboard)
            {
				keyboard.handInteractingWithKeyboard = hand;
            }		

			// SteamVR: hide controller hint
			// ControllerButtonHints.HideButtonHint(hand, globals.menuInteraction_Select); // not highlighting any button
		}

		// minor hack: force SteamVR UI hand interation states to match custom pointer UI interaction states
		if (hand.uiInteractAction.active && ViRMA_InputModule.instance.contactUIEnabled)
        {
			if (hand.uiInteractAction.stateDown)
			{
				SetBtnDownState();
			}
			if (hand.uiInteractAction.stateUp)
			{
				SetBtnNormalState();
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
		SetBtnHighlightState();
	}
    public void OnPointerExit(PointerEventData eventData)
    {
		SetBtnNormalState();
	}
    public void OnPointerUp(PointerEventData eventData)
    {
		// do nothing
	}
	public void OnPointerDown(PointerEventData eventData)
	{
		SetBtnDownState();
	}
	public void OnPointerClick(PointerEventData eventData)
    {
		SetBtnNormalState();
	}

	// general
	private void SetKeyColliderSize()
	{
		Button btn = GetComponent<Button>();
		float width = btn.GetComponent<RectTransform>().rect.width;
		float height = btn.GetComponent<RectTransform>().rect.height;
		BoxCollider keyCollider = btn.gameObject.GetComponentInChildren<BoxCollider>();
		keyCollider.size = new Vector3(width, height, 25);
	}

	// button states
	public void SetBtnNormalState()
    {
		btnBackground.color = normalBackgroundColor;
		btnText.color = normalTextColor;
	}
	private void SetBtnHighlightState()
    {
		normalBackgroundColor = btnBackground.color;
		normalTextColor = btnText.color;

		btnBackground.color = ViRMA_Colors.BrightenColor(btnBackground.color);
		btnText.color = ViRMA_Colors.BrightenColor(btnText.color);
	}	
	private void SetBtnDownState()
    {
		Color32 originalBgColor = btnBackground.color;
		Color32 originalTextColor = btnText.color;

		btnBackground.color = originalTextColor;
		btnText.color = originalBgColor;
	}
	private void BtnFadeController()
    {
		float alpha;
		if (buttonFaded)
		{
			alpha = 0.15f;
		}
		else
		{
			alpha = 1.0f;
		}

		if (btnBackground.color.a != alpha)
		{
			btnBackground.color = new Color(btnBackground.color.r, btnBackground.color.g, btnBackground.color.b, alpha);
		}
		if (btnText.color.a != alpha)
		{
			btnText.color = new Color(btnText.color.r, btnText.color.g, btnText.color.b, alpha);
		}
		if (btnIcon)
        {
			if (btnIcon.color.a != alpha)
			{
				btnIcon.color = new Color(btnIcon.color.r, btnIcon.color.g, btnIcon.color.b, alpha);
			}
		}	
	}

}
