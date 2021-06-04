using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class ViRMA_Keyboard : MonoBehaviour
{
    Button[] keys;
    public string typedString = "";

    private void Awake()
    {
        keys = GetComponentsInChildren<Button>();
    }

    private void Start()
    {
        foreach (Button key in keys)
        {
            key.onClick.AddListener(() => submitKey(key));

            setKeyColliderSize(key);
        }

        // TESTING: late start for after Player instance is loaded
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);

        placeInFrontOfPlayer();
    }

    private void placeInFrontOfPlayer()
    {
        Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
        flattenedVector.y = 0;
        flattenedVector.Normalize();
        Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.5f;
        spawnPos.y = spawnPos.y * 0.9f;
        transform.position = spawnPos;
        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
    }
    private void setKeyColliderSize(Button key)
    {
        float width = key.GetComponent<RectTransform>().rect.width;
        float height = key.GetComponent<RectTransform>().rect.height;
        BoxCollider keyCollider = key.gameObject.GetComponentInChildren<BoxCollider>();
        keyCollider.size = new Vector3(width, height, 25);
    }
    private void submitKey(Button key)
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
    }

}
