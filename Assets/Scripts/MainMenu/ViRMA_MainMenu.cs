using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class ViRMA_MainMenu : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    private bool xParentChainFetched;
    private bool yParentChainFetched;
    private bool zParentChainFetched;

    // main menu sections (set in editor)
    public GameObject ui_projectedDimensions;

    private Button[] allBtns;

    private List<Tag> xParentChain;
    private List<Tag> yParentChain;
    private List<Tag> zParentChain;

    private Transform xBtnWrapper;
    private Transform yBtnWrapper;
    private Transform zBtnWrapper;

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    private void Start()
    {
        // debugging
        transform.parent = null;
        transform.localPosition = new Vector3(0, 9999, 0);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        StartCoroutine(ToggleMainMenu(true));
    }

    private void Update()
    {
        if (xParentChainFetched && yParentChainFetched && zParentChainFetched)
        {
            UpdateProjFilerBtns();

            xParentChainFetched = false;
            yParentChainFetched = false;
            zParentChainFetched = false;          
        }
    }

    // projected filters
    public void FetchProjectedFilterMetadata()
    {
        xParentChain = new List<Tag>();
        yParentChain = new List<Tag>();
        zParentChain = new List<Tag>();

        xParentChainFetched = false;
        yParentChainFetched = false;
        zParentChainFetched = false;

        AxesLabels axesLabels = globals.vizController.activeAxesLabels;

        if (axesLabels.X != null)
        {
            if (axesLabels.X.Type == "node")
            {
                StartCoroutine(GetHierarchyParentChain(axesLabels.X.Id, "X"));
            }
            else if (axesLabels.X.Type == "tagset")
            {
                Tag tagsetParent = new Tag();
                tagsetParent.Id = axesLabels.X.Id;
                tagsetParent.Label = axesLabels.X.Label;
                xParentChain = new List<Tag>() { tagsetParent };
                xParentChainFetched = true;
            }
        }
        else
        {
            xParentChainFetched = true;
        }

        if (axesLabels.Y != null)
        {
            if (axesLabels.Y.Type == "node")
            {
                StartCoroutine(GetHierarchyParentChain(axesLabels.Y.Id, "Y"));
            }
            else if (axesLabels.Y.Type == "tagset")
            {
                Tag tagsetParent = new Tag();
                tagsetParent.Id = axesLabels.Y.Id;
                tagsetParent.Label = axesLabels.Y.Label;
                yParentChain = new List<Tag>() { tagsetParent };
                yParentChainFetched = true;
            }
        }
        else
        {
            yParentChainFetched = true;
        }

        if (axesLabels.Z != null)
        {
            if (axesLabels.Z.Type == "node")
            {
                StartCoroutine(GetHierarchyParentChain(axesLabels.Z.Id, "Z"));
            }
            else if (axesLabels.Z.Type == "tagset")
            {
                Tag tagsetParent = new Tag();
                tagsetParent.Id = axesLabels.Z.Id;
                tagsetParent.Label = axesLabels.Z.Label;
                zParentChain = new List<Tag>() { tagsetParent };
                zParentChainFetched = true;
            }
        }
        else
        {
            zParentChainFetched = true;
        }
    }
    private IEnumerator GetHierarchyParentChain(int targetId, string axis)
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

        if (axis == "X")
        {
            xParentChain = allParents;
            xParentChainFetched = true;
        }
        if (axis == "Y")
        {
            yParentChain = allParents;
            yParentChainFetched = true;
        }
        if (axis == "Z")
        {
            zParentChain = allParents;
            zParentChainFetched = true;
        }
    }
    private void UpdateProjFilerBtns()
    {
        ViRMA_UIScrollable[] scrollableUis = ui_projectedDimensions.GetComponentsInChildren<ViRMA_UIScrollable>();

        xBtnWrapper = scrollableUis[0].transform.GetChild(0).transform;
        yBtnWrapper = scrollableUis[1].transform.GetChild(0).transform;
        zBtnWrapper = scrollableUis[2].transform.GetChild(0).transform;

        GameObject xTemplateBtn = xBtnWrapper.transform.GetChild(0).gameObject;
        GameObject yTemplateBtn = yBtnWrapper.transform.GetChild(0).gameObject;
        GameObject zTemplateBtn = zBtnWrapper.transform.GetChild(0).gameObject;

        xParentChain.Reverse();
        for (int i = 0; i < xParentChain.Count; i++)
        {
            GameObject newBtn = Instantiate(xTemplateBtn, xBtnWrapper);
            newBtn.GetComponentInChildren<Text>().text = xParentChain[i].Label;
            newBtn.name = xParentChain[i].Label + "_" + xParentChain[i].Id;
            if (i == xParentChain.Count - 1)
            {
                Destroy(xTemplateBtn);
            }
        }
        xBtnWrapper.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;

        yParentChain.Reverse();
        for (int i = 0; i < yParentChain.Count; i++)
        {
            GameObject newBtn = Instantiate(yTemplateBtn, yBtnWrapper);
            newBtn.GetComponentInChildren<Text>().text = yParentChain[i].Label;
            newBtn.name = yParentChain[i].Label + "_" + yParentChain[i].Id;
            if (i == yParentChain.Count - 1)
            {
                Destroy(yTemplateBtn);
            }
        }

        zParentChain.Reverse();
        for (int i = 0; i < zParentChain.Count; i++)
        {
            GameObject newBtn = Instantiate(zTemplateBtn, zBtnWrapper);
            newBtn.GetComponentInChildren<Text>().text = zParentChain[i].Label;
            newBtn.name = zParentChain[i].Label + "_" + zParentChain[i].Id;
            if (i == zParentChain.Count - 1)
            {
                Destroy(zTemplateBtn);
            }
        }

        SetProjFilterBtnStates();

        StartCoroutine(ScrollBtnWrappersToBottom());
    }
    private IEnumerator ScrollBtnWrappersToBottom()
    {
        yield return new WaitForEndOfFrame();

        xBtnWrapper.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
        yBtnWrapper.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
        zBtnWrapper.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
    }
    private void SetProjFilterBtnStates()
    {
        ViRMA_UIScrollable[] scrollableUis = ui_projectedDimensions.GetComponentsInChildren<ViRMA_UIScrollable>();
        for (int i = 0; i < scrollableUis.Length; i++)
        {
            Button[] buttons = scrollableUis[i].GetComponentsInChildren<Button>();
            for (int j = 0; j < buttons.Length; j++)
            {
                GameObject buttonObj = buttons[j].gameObject;
                Text btnText = buttonObj.GetComponentInChildren<Text>();
                Image btnBackground = buttonObj.GetComponent<Image>();

                if (i == 0)
                {
                    // x
                    btnBackground.color = ViRMA_Colors.axisLightRed;
                    btnText.color = Color.white;
                    if (j == buttons.Length - 1)
                    {
                        btnBackground.color = ViRMA_Colors.axisRed;
                    }
                }
                else if (i == 1)
                {
                    // y 
                    btnBackground.color = ViRMA_Colors.axisLightGreen;
                    btnText.color = Color.white;
                    if (j == buttons.Length - 1)
                    {
                        btnBackground.color = ViRMA_Colors.axisGreen;
                    }
                }
                else if (i == 2)
                {
                    // z
                    btnBackground.color = ViRMA_Colors.axisLightBlue;
                    btnText.color = Color.white;
                    if (j == buttons.Length - 1)
                    {
                        btnBackground.color = ViRMA_Colors.axisBlue;
                    }
                }

                if (btnText.text == "Loading")
                {
                    buttonObj.name = "placeholder";
                    btnText.text = "None";
                    btnBackground.color = Color.grey;
                }
            }
        }
    }

    // general
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
        ui_projectedDimensions.SetActive(false);
    }

    

}
