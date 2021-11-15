using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class ViRMA_UiElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
	// SteamVR: used for UI interaction with controller
	//private ViRMA_GlobalsAndActions globals;
	private ViRMA_Keyboard keyboard;

	public CustomEvents.UnityEventHand onHandClick;
	protected Hand currentHand;
	private BoxCollider col;

	// used for custom UI interaction button states
	private Image btnBackground;
	private Text btnText;
	private RawImage btnIcon;
	public Button btn;

	public Color defaultBackgroundColor;
	public Color defaultTextColor;

	public Color hoverBackgroundColor;
	public Color hoverTextColor;

	public Color clickedBackgroundColor;
	public Color clickedTextColor;

	public Color toggledBackgroundColor;
	public Color toggledTextColor;

	public bool buttonFaded;
	public bool toggle;
	public bool isToggled;

	public object buttonData; // (to do)

	protected virtual void Awake()
	{
		//globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
		btnBackground = GetComponent<Image>();
		btnText = GetComponentInChildren<Text>();
		btnIcon = GetComponentInChildren<RawImage>();
		btn = GetComponent<Button>();
		if (btn)
		{
			// SteamVR: assign function to button when it is clicked by hand script
			btn.onClick.AddListener(OnButtonClick);

			// disable default btn nav states
			Navigation disableNav = new Navigation();
			disableNav.mode = Navigation.Mode.None;
			btn.navigation = disableNav;
			btn.transition = Selectable.Transition.None;
		}
		keyboard = GetComponentInParent<ViRMA_Keyboard>();

		GenerateBtnDefaults(ViRMA_Colors.darkBlue, Color.white);
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

		// reverse default colours to simulate toggled status
		BtnToggleController();
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

		// hack: force SteamVR UI hand interation states to match custom pointer UI interaction states
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
		float width = btn.GetComponent<RectTransform>().rect.width;
		float height = btn.GetComponent<RectTransform>().rect.height;
		col = btn.gameObject.GetComponentInChildren<BoxCollider>();
		col.size = new Vector3(width, height, 25);
	}
	public void Hide(bool toHide)
    {
		if (toHide)
        {
			if (btn && btn.interactable)
			{
				btn.interactable = false;
			}
			if (btnBackground && btnBackground.enabled)
            {
				btnBackground.enabled = false;
			}
			if (btnText && btnText.enabled)
            {
				btnText.enabled = false;
			}
			if (btnIcon && btnIcon.enabled)
            {
				btnIcon.enabled = false;
			}
		}
		else
        {
			if (btn && !btn.interactable)
			{
				btn.interactable = true;
			}
			if (btnBackground && !btnBackground.enabled)
			{
				btnBackground.enabled = true;
			}
			if (btnText && !btnText.enabled)
			{
				btnText.enabled = true;
			}
			if (btnIcon && !btnIcon.enabled)
			{
				btnIcon.enabled = true;
			}
		}	
    }
	public void Toggle(bool toToggle)
    {
		toggle = toToggle;
	}

	// button states
	public void GenerateBtnDefaults(Color bgColor, Color textColor)
	{
		// default
		defaultBackgroundColor = bgColor;
		defaultTextColor = textColor;

		// hover
		if (bgColor == Color.white)
        {
			hoverBackgroundColor = ViRMA_Colors.DarkenColor(bgColor);
		}
		else
        {
			hoverBackgroundColor = ViRMA_Colors.BrightenColor(bgColor);
		}	
		hoverTextColor = ViRMA_Colors.BrightenColor(textColor);

		// clicked
		clickedBackgroundColor = textColor;
		clickedTextColor = bgColor;

		SetBtnNormalState();
	}
	private void SetBtnNormalState()
    {
		btnBackground.color = defaultBackgroundColor;
		btnText.color = defaultTextColor;

		if (btnIcon)
		{
			btnIcon.color = btnText.color;
		}
	}
	private void SetBtnHighlightState()
    {
		btnBackground.color = hoverBackgroundColor;
		btnText.color = hoverTextColor;

		if (btnIcon)
		{
			btnIcon.color = btnText.color;
		}
	}	
	private void SetBtnDownState()
    {
		btnBackground.color = clickedBackgroundColor;
		btnText.color = clickedTextColor;
		if (btnIcon)
		{
			btnIcon.color = btnText.color;
		}
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

		if (btnBackground)
        {
			if (btnBackground.color.a != alpha)
			{
				btnBackground.color = new Color(btnBackground.color.r, btnBackground.color.g, btnBackground.color.b, alpha);
			}
		}
		
		if (btnText)
        {
			if (btnText.color.a != alpha)
			{
				btnText.color = new Color(btnText.color.r, btnText.color.g, btnText.color.b, alpha);
			}
		}
		
		if (btnIcon)
        {
			if (btnIcon.color.a != alpha)
			{
				btnIcon.color = new Color(btnIcon.color.r, btnIcon.color.g, btnIcon.color.b, alpha);
			}
		}	
	}
	private void BtnToggleController()
    {
		if (toggle == true)
		{
			if (isToggled == false)
			{
				GenerateBtnDefaults(clickedBackgroundColor, clickedTextColor);
				isToggled = true;
			}
		}

		if (toggle == false)
		{
			if (isToggled == true)
			{
				GenerateBtnDefaults(clickedBackgroundColor, clickedTextColor);
				isToggled = false;
			}
		}
	}

	

}
