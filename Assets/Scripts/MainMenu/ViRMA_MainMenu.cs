using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_MainMenu : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    private Button[] allBtns;

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    private void Start()
    {
        globals.menuInteractionActions.Activate();

        SetBtnDefaultState();

        // StartCoroutine(TestPosition()); // testing
    }

    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }

    public void SetBtnDefaultState()
    {
        allBtns = GetComponentsInChildren<Button>();
        foreach (Button btn in allBtns)
        {
            Text btnText = btn.GetComponentInChildren<Text>();
            Image btnBackground = btn.GetComponent<Image>();

            if (btnText && btnBackground)
            {
                btnBackground.color = globals.lightBlack;
                btnText.color = Color.white;
            }
        }
    }

    private IEnumerator TestPosition()
    {
        yield return new WaitForSeconds(1);

        transform.parent = Player.instance.leftHand.mainRenderModel.transform;

        transform.localPosition = new Vector3(0.2f, 0.05f, 0.0f);
        transform.localRotation = Quaternion.identity;
        transform.localRotation = Quaternion.Euler(90, 0, 0);
        //transform.localScale = Vector3.one * 0.2f;

        /*
        // place in front
        Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
        flattenedVector.y = 0;
        flattenedVector.Normalize();
        Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.6f;
        transform.position = spawnPos;
        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
        */

    }

}
