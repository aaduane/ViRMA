using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_MainMenu : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    ScrollRect scrollRect;

    float current = 0.0f;

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();


        GameObject content = GameObject.Find("GridWithOurElementsOrOptions");

        GameObject testBtn = content.transform.GetChild(0).gameObject;
        testBtn.name = "Button 1";
        for (int i = 0; i < 9; i++)
        {
            GameObject newBtn = Instantiate(testBtn, content.transform);
            string btnName = "Button " + (i + 2);
            newBtn.name = btnName;
            newBtn.GetComponentInChildren<Text>().text = btnName;
        }

        scrollRect = GetComponentInChildren<ScrollRect>();
    }

    private void Start()
    {
        StartCoroutine(TestPosition());

        globals.menuInteractionActions.Activate();
    }

    private void Update()
    {



        float joyStickDirection = globals.menuInteraction_Scroll.GetAxis(SteamVR_Input_Sources.Any).y;
        if (joyStickDirection != 0)
        {
            float multiplier = joyStickDirection * 10f;
            scrollRect.verticalNormalizedPosition = (scrollRect.verticalNormalizedPosition + multiplier * Time.deltaTime);
        }

        
    }

    private IEnumerator TestPosition()
    {
        yield return new WaitForSeconds(1);

        Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
        flattenedVector.y = 0;
        flattenedVector.Normalize();
        Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.6f;
        transform.position = spawnPos;
        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
    }

}
