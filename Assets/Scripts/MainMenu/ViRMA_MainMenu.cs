using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class ViRMA_MainMenu : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    // main menu sections (set in editor)
    public GameObject ui_projectedDimensions;

    private Button[] allBtns;

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    private void Start()
    {
        GenerateTestScrollBtns();

        SetAllBtnDefaultStates();


        // debugging
        transform.parent = null;
        transform.localPosition = new Vector3(0, 9999, 0);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        //StartCoroutine(ToggleMainMenu(true));
        //DisableSections(); 
    }

    public void SetAllBtnDefaultStates()
    {
        allBtns = GetComponentsInChildren<Button>();
        foreach (Button btn in allBtns)
        {
            Text btnText = btn.GetComponentInChildren<Text>();
            Image btnBackground = btn.GetComponent<Image>();

            if (btnText && btnBackground)
            {
                btnBackground.color = ViRMA_Colors.lightBlack;
                btnText.color = Color.white;
            }
        }
    }

    private IEnumerator ToggleMainMenu(bool toShow)
    {
        yield return new WaitForSeconds(1);

        if (toShow)
        {
            /*
            // parent to left hand
            transform.parent = Player.instance.leftHand.mainRenderModel.transform;
            transform.localPosition = new Vector3(0.2f, 0.05f, 0.0f);
            transform.localRotation = Quaternion.identity;
            transform.localRotation = Quaternion.Euler(90, 0, 0);
            transform.localScale = Vector3.one * 0.3f;
            */


            // place in front of player
            Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
            flattenedVector.y = 0;
            flattenedVector.Normalize();
            Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.5f;
            transform.position = spawnPos;
            transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
        }
        else
        {
            transform.parent = null;
            transform.localPosition = new Vector3(0, 9999, 0);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }      
        
    }

    public void UpdateFiltersDisplay()
    {
        AxesLabels axesLabels = globals.vizController.activeAxesLabels;

        if (axesLabels.X != null)
        {
            if (axesLabels.X.Type == "node")
            {
                StartCoroutine(GetAllParents(axesLabels.X.Id, "X"));
            }
        }

        if (axesLabels.Y != null)
        {
            if (axesLabels.Y.Type == "node")
            {
                StartCoroutine(GetAllParents(axesLabels.X.Id, "Y"));
            }
        }

        if (axesLabels.Z != null)
        {
            if (axesLabels.Z.Type == "node")
            {
                StartCoroutine(GetAllParents(axesLabels.X.Id, "Z"));
            }
        }
    }

    private IEnumerator GetAllParents(int targetId, string test)
    {
        List<Tag> allParents = new List<Tag>();

        bool traveseringHierarchy = true;
        int idChecker = targetId;
        while (traveseringHierarchy)
        {
            yield return StartCoroutine(ViRMA_APIController.GetHierarchyParent(idChecker, (newParent) => {
                if (newParent != null)
                {
                    allParents.Add(newParent);
                    idChecker = newParent.Id;
                }
                else
                {
                    traveseringHierarchy = false;
                }
            }));
        }

        foreach (var parent in allParents)
        {
            Debug.Log(test + " : " + parent.Label);
        }   
    }

    // testing
    private void GenerateTestScrollBtns()
    {
        ViRMA_UIScrollable[] scrollableUis = ui_projectedDimensions.GetComponentsInChildren<ViRMA_UIScrollable>();
        foreach (ViRMA_UIScrollable scrollableUi in scrollableUis)
        {
            GameObject btnParent = scrollableUi.transform.GetChild(0).gameObject;
            GameObject testBtn = btnParent.transform.GetChild(0).gameObject;
            testBtn.name = "Button 1";
            for (int i = 0; i < 9; i++)
            {
                GameObject newBtn = Instantiate(testBtn, btnParent.transform);
                string btnName = "Button " + (i + 2);
                newBtn.name = btnName;
                newBtn.GetComponentInChildren<Text>().text = btnName;
                //Debug.Log(scrollableUi.name + " | " + i);
            }
        }      
    }
    private void DisableSections()
    {
        //ui_projectedDimensions.SetActive(false);
    }

}
