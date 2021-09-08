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
    private bool keyboardFaded;
    public bool queryLoading;
    public GameObject loadingIndicator;
    private Coroutine activeQueryCoroutine;

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
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);

        // delayed things

        ToggleDimExKeyboard(true); // testing
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

            if (keyText && keyBackground)
            {
                if (keyText.gameObject.transform.parent.name == "CLEAR")
                {
                    keyBackground.color = new Color32(192, 57, 43, 255);
                    keyText.color = Color.white;
                }
                else if (keyText.gameObject.transform.parent.name == "DELETE")
                {
                    keyBackground.color = new Color32(211, 84, 0, 255);
                    keyText.color = Color.white;
                }
                else
                {
                    keyBackground.color = globals.lightBlack;
                    keyText.color = Color.white;
                }
            }
        }
    }
    public void ToggleDimExKeyboard(bool onOff)
    {
        // scaling
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        if (onOff)
        {
            Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
            flattenedVector.y = 0;
            flattenedVector.Normalize();
            Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.4f;
            spawnPos.y = spawnPos.y * 0.75f;
            transform.position = spawnPos;
            transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);

            globals.menuInteractionActions.Activate();

        }
        else
        {
            transform.position = new Vector3(0, 9999, 0);
            globals.dimExplorer.ClearDimExplorer();

            globals.menuInteractionActions.Deactivate();

        }
    }
    public void FadeKeyboard(bool toFade)
    {
        if (toFade == true)
        {     
            if (keyboardFaded == false)
            {
                Collider[] colliders = GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                {
                    if (col.gameObject != gameObject)
                    {
                        col.enabled = false;
                    }
                }

                Image[] backgrounds = GetComponentsInChildren<Image>();
                foreach (var bgs in backgrounds)
                {
                    bgs.color = new Color(bgs.color.r, bgs.color.g, bgs.color.b, 0.15f);
                }

                Text[] texts = GetComponentsInChildren<Text>();
                foreach (var text in texts)
                {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, 0.15f);
                }
                typedWordTMP.color = new Color(typedWordTMP.color.r, typedWordTMP.color.g, typedWordTMP.color.b, 0.15f);

                keyboardFaded = true;
            }        
        }
        else
        {
            if (keyboardFaded == true)
            {
                Collider[] colliders = GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                {
                    col.enabled = true;
                }

                Image[] backgrounds = GetComponentsInChildren<Image>();
                foreach (var bgs in backgrounds)
                {
                    bgs.color = new Color(bgs.color.r, bgs.color.g, bgs.color.b, 1.0f);
                    if (bgs.gameObject.name == "Background")
                    {
                        bgs.color = new Color32(0, 0, 0, 100);
                    }
                }

                Text[] texts = GetComponentsInChildren<Text>();
                foreach (var text in texts)
                {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, 1.0f);
                }
                typedWordTMP.color = new Color(typedWordTMP.color.r, typedWordTMP.color.g, typedWordTMP.color.b, 1.0f);

                keyboardFaded = false;
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

                queryLoading = true;
                key.enabled = false;
                StartCoroutine(globals.dimExplorer.ClearDimExplorer());           

                activeQueryCoroutine = StartCoroutine(ViRMA_APIController.SearchHierachies(typedWordString.ToLower(), (nodes) => {             
                    StartCoroutine(globals.dimExplorer.LoadDimExplorer(nodes));
                    activeQueryCoroutine = null;
                    key.enabled = true;
                }));
            }      
        }
        else if (buttonName == "DELETE")
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
        else if (buttonName == "SPACE")
        {
            if (typedWordString.Substring(typedWordString.Length - 1) != " ") {
                typedWordString += " ";
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
        if (queryLoading)
        {
            if (!loadingIndicator.transform.parent.gameObject.activeSelf)
            {
                loadingIndicator.transform.parent.gameObject.SetActive(true);
            }          
            loadingIndicator.transform.Rotate(0, 0, -300f * Time.deltaTime);
        }
        else
        {
            if (loadingIndicator.transform.parent.gameObject.activeSelf)
            {
                loadingIndicator.transform.parent.gameObject.SetActive(false);
            }        
        }
    }

}
