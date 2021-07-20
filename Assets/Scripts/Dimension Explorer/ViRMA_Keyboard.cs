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
    public GameObject typedWordObj;
    

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

        typedWordObj.GetComponent<TextMeshProUGUI>().text = typedWordString;

        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);

        PlaceInFrontOfPlayer();

        transform.localScale = transform.localScale * 0.5f;

        globals.menuInteractionActions.Activate();
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
    private void SubmitKey(Button key)
    {
        string buttonName = key.gameObject.name;
        string submittedChar = key.GetComponentInChildren<Text>().text;      

        if (buttonName == "SUBMIT")
        {
            if (typedWordString.Length > 0)
            {
                StartCoroutine(ViRMA_APIController.SearchHierachies(typedWordString.ToLower(), (nodes) => {
                    StartCoroutine(globals.dimExplorer.LoadDimExplorer(nodes));
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

        typedWordObj.GetComponent<TextMeshProUGUI>().text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(typedWordString.ToLower());
    }

}
