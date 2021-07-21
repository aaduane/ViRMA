using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class ViRMA_Keyboard : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private Coroutine activeQueryCoroutine;
    private Button[] keys;
    public string typedWordString = "";
    public TextMeshProUGUI typedWordTMP;
    private bool keyboardFaded;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        keys = GetComponentsInChildren<Button>();
    }

    private void Start()
    {
        foreach (Button key in keys)
        {
            key.onClick.AddListener(() => SubmitKey(key));

            SetKeyColliderSize(key);
        }

        typedWordTMP.text = typedWordString;

        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);

        PlaceInFrontOfPlayer();

        transform.localScale = transform.localScale * 0.5f;

        globals.menuInteractionActions.Activate();
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

    private void PlaceInFrontOfPlayer()
    {
        Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
        flattenedVector.y = 0;
        flattenedVector.Normalize();
        Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.4f;
        spawnPos.y = spawnPos.y * 0.75f;
        transform.position = spawnPos;
        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
    }
    private void SetKeyColliderSize(Button key)
    {
        float width = key.GetComponent<RectTransform>().rect.width;
        float height = key.GetComponent<RectTransform>().rect.height;
        BoxCollider keyCollider = key.gameObject.GetComponentInChildren<BoxCollider>();
        keyCollider.size = new Vector3(width, height, 25);
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

                activeQueryCoroutine = StartCoroutine(ViRMA_APIController.SearchHierachies(typedWordString.ToLower(), (nodes) => {
                    StartCoroutine(globals.dimExplorer.LoadDimExplorer(nodes));
                }));

                //FadeKeyboard(true);
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

}
