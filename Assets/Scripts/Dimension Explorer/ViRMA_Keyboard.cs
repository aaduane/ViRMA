using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class ViRMA_Keyboard : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private Button[] keys;
    public string typedWordString = "";
    public TextMeshProUGUI typedWordTMP;
    public Image typedWordBg;
    public GameObject loadingIcon;
    private Coroutine activeQueryCoroutine;
    public Hand handInteractingWithKeyboard;

    // flags
    public bool dimExQueryLoading;
    public bool keyboardLoaded;
    public bool keyboardFaded;
    public bool keyboardMoving;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        keys = GetComponentsInChildren<Button>();   
    }

    private void Start()
    {
        SetBtnDefaultState();

        typedWordTMP.text = typedWordString;

        StartCoroutine(LateStart());
    }

    private void Update()
    {
        LoadingIndicator();

        KeyboardRepositioning();
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);

        // delayed things

        //ToggleDimExKeyboard(true); // for testing
    }

    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            FadeKeyboard(false);
        }
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            // Debug.Log("EXIT - Keyboard triggered!");
        }
    }

    public void SetBtnDefaultState()
    {
        foreach (Button key in keys)
        {
            key.onClick.AddListener(() => SubmitKey(key));

            Text keyText = key.GetComponentInChildren<Text>();
            Image keyBackground = key.GetComponent<Image>();
            ViRMA_UiElement virmaBtn = key.GetComponent<ViRMA_UiElement>();

            if (keyText && keyBackground)
            {
                Color32 bgCol;
                Color32 textCol;

                if (keyText.gameObject.transform.parent.name == "CLEAR")
                {
                    bgCol = ViRMA_Colors.flatOrange;
                    textCol = Color.white;
                }
                else if (keyText.gameObject.transform.parent.name == "CLOSE")
                {
                    bgCol = new Color32(192, 57, 43, 255);
                    textCol = Color.white;
                }
                else if (keyText.gameObject.transform.parent.name == "BACKSPACE")
                {
                    bgCol = ViRMA_Colors.darkBlue;
                    textCol = Color.white;
                }
                else if (keyText.gameObject.transform.parent.name == "MOVE")
                {
                    bgCol = new Color32(99, 110, 114, 255);
                    textCol = Color.white;
                }
                else if (keyText.gameObject.transform.parent.name == "SUBMIT")
                {
                    bgCol = new Color32(39, 174, 96, 255);
                    textCol = Color.white;
                }
                else
                {
                    bgCol = ViRMA_Colors.darkBlue;
                    textCol = Color.white;
                }

                virmaBtn.GenerateBtnDefaults(bgCol, textCol);
            }
        }
    }
    public void ToggleDimExKeyboard(bool onOff)
    {
        // scaling and appearance
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        FadeKeyboard(false);

        if (onOff)
        {
            Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
            flattenedVector.y = 0;
            flattenedVector.Normalize();
            Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.4f;
            spawnPos.y = spawnPos.y * 0.75f;
            transform.position = spawnPos;
            transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
            keyboardLoaded = true;
        }
        else
        {
            transform.position = new Vector3(0, 9999, 0);
            StartCoroutine(globals.dimExplorer.ClearDimExplorer());
            keyboardLoaded = false;
        }
    }
    public void FadeKeyboard(bool toFade)
    {
        if (toFade == true)
        {     
            if (keyboardFaded == false)
            {
                Collider[] keyboardColliders = GetComponentsInChildren<Collider>();
                foreach (var col in keyboardColliders)
                {
                    if (col.gameObject != gameObject)
                    {
                        col.enabled = false;
                    }
                }

                ViRMA_UiElement[] btnElements = GetComponentsInChildren<ViRMA_UiElement>();
                foreach (var btnElement in btnElements)
                {
                    btnElement.buttonFaded = true;
                }

                typedWordTMP.color = new Color(typedWordTMP.color.r, typedWordTMP.color.g, typedWordTMP.color.b, 0.15f);
                typedWordBg.color = new Color(typedWordBg.color.r, typedWordBg.color.g, typedWordBg.color.b, 0.15f);

                keyboardFaded = true;
            }        
        }
        else
        {
            if (keyboardFaded == true)
            {
                Collider[] keyboardColliders = GetComponentsInChildren<Collider>();
                foreach (var col in keyboardColliders)
                {
                    col.enabled = true;
                }

                ViRMA_UiElement[] btnElements = GetComponentsInChildren<ViRMA_UiElement>();
                foreach (var btnElement in btnElements)
                {
                    btnElement.buttonFaded = false;
                }

                typedWordTMP.color = new Color(typedWordTMP.color.r, typedWordTMP.color.g, typedWordTMP.color.b, 1.0f);
                typedWordBg.color = new Color(typedWordBg.color.r, typedWordBg.color.g, typedWordBg.color.b, 1.0f);

                globals.dimExplorer.horizontalRigidbody.velocity = Vector3.zero;

                keyboardFaded = false;
            }    
        }
    }
    private void KeyboardRepositioning()
    {
        if (keyboardMoving)
        {
            if (globals.menuInteraction_Select.GetState(handInteractingWithKeyboard.handType))
            {
                if (handInteractingWithKeyboard)
                {
                    if (transform.parent != handInteractingWithKeyboard)
                    {
                        transform.parent = handInteractingWithKeyboard.transform;
                    }

                    if (keyboardFaded)
                    {
                        FadeKeyboard(false);
                    }

                    if (globals.dimExplorerActions.IsActive())
                    {
                        globals.dimExplorerActions.Deactivate();
                    }
                }      
            }
            else
            {
                if (handInteractingWithKeyboard)
                {
                    if (transform.parent == handInteractingWithKeyboard.transform)
                    {
                        transform.parent = null;
                        keyboardMoving = false;
                    }
                }
            }
        }
    }
    private void SubmitKey(Button key)
    {
        string buttonName = key.gameObject.name;
        string submittedChar = key.GetComponentInChildren<Text>().text;

        if (buttonName == "SUBMIT")
        {
            if (typedWordString.Length > 0)
            {
                if (activeQueryCoroutine != null)
                {
                    StopCoroutine(activeQueryCoroutine);
                }

                dimExQueryLoading = true;
                key.enabled = false;
                StartCoroutine(globals.dimExplorer.ClearDimExplorer());           

                activeQueryCoroutine = StartCoroutine(ViRMA_APIController.SearchHierachies(typedWordString.ToLower(), (nodes) => {             
                    StartCoroutine(globals.dimExplorer.LoadDimExplorer(nodes));
                    activeQueryCoroutine = null;
                    key.enabled = true;
                }));
            }      
        }
        else if (buttonName == "CLOSE")
        {

            if (activeQueryCoroutine != null)
            {
                StopCoroutine(activeQueryCoroutine);
            }
            dimExQueryLoading = false;
            ToggleDimExKeyboard(false);
        }
        else if (buttonName == "BACKSPACE")
        {
            if (typedWordString.Length > 0)
            {
                typedWordString = typedWordString.Substring(0, typedWordString.Length - 1);
            }       
        }
        else if (buttonName == "CLEAR")
        {
            typedWordString = "";
        }
        else if (buttonName == "MOVE")
        {
            handInteractingWithKeyboard = key.GetComponent<ViRMA_UiElement>().handINteractingWithUi;
            keyboardMoving = true;
        }
        else if (buttonName == "SPACE")
        {
            if (typedWordString.Length > 0)
            {
                if (typedWordString.Substring(typedWordString.Length - 1) != " ")
                {
                    typedWordString += " ";
                }
            }             
        }
        else
        {
            typedWordString += submittedChar;
        }

        typedWordTMP.text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(typedWordString.ToLower());
    }
    private void LoadingIndicator()
    {
        if (dimExQueryLoading)
        {
            if (!loadingIcon.transform.parent.gameObject.activeSelf)
            {
                loadingIcon.transform.parent.gameObject.SetActive(true);
            }          
            loadingIcon.transform.Rotate(0, 0, -300f * Time.deltaTime);
        }
        else
        {
            if (loadingIcon.transform.parent.gameObject.activeSelf)
            {
                loadingIcon.transform.parent.gameObject.SetActive(false);
            }        
        }
        
    }

}
