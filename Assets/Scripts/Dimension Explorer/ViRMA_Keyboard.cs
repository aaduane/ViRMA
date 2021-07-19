using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class ViRMA_Keyboard : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    Button[] keys;
    public string typedString = "";

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
        string submittedChar = key.GetComponentInChildren<Text>().text;

        if (submittedChar == "←")
        {
            if (typedString.Length > 0)
            {
                typedString = typedString.Substring(0, typedString.Length - 1);
            }       
        }
        else
        {
            typedString += submittedChar;
        }

        Debug.Log(typedString);

        StartCoroutine(ViRMA_APIController.SearchHierachies("computer", (nodes) => {
            StartCoroutine(globals.dimExplorer.LoadDimExplorer(nodes));
        }));

    }

}
