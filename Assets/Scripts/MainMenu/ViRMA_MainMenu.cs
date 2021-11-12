﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;
using TMPro;

public class ViRMA_MainMenu : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private List<GameObject> menuSections;

    // dimension browser
    public GameObject section_dimensionBrowser;

    public GameObject ui_browseFilters;
    public GameObject ui_projectedFilters;
    public GameObject ui_directFilers;

    public GameObject hoveredDirectFilter;
    private ViRMA_UiElement[] directFilterOptions;

    private bool xParentChainFetched;
    private bool yParentChainFetched;
    private bool zParentChainFetched;

    private List<Tag> xParentChain;
    private List<Tag> yParentChain;
    private List<Tag> zParentChain;

    private Transform xBtnWrapper;
    private Transform yBtnWrapper;
    private Transform zBtnWrapper;

    // time picker
    public GameObject section_timePicker;

    public GameObject ui_weekday;
    public GameObject ui_hour;
    public GameObject ui_month;
    public GameObject ui_dates;

    // location picker
    public GameObject section_locationPicker;


    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        menuSections = new List<GameObject>();
        menuSections.Add(section_dimensionBrowser);
        menuSections.Add(section_timePicker);
        menuSections.Add(section_locationPicker);
    }

    private void Start()
    {
        SetupBrowseFiltersOptions();

        SetupTimePicker();

        ToggleMenuSection(section_dimensionBrowser);




        // testing
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

        CheckActiveDirectFilterOptions();
    }

    // browse filters
    private void SetupBrowseFiltersOptions()
    {
        ViRMA_UiElement[] browseFilterOptions = ui_browseFilters.GetComponentsInChildren<ViRMA_UiElement>();
        foreach (ViRMA_UiElement browseFilterOption in browseFilterOptions)
        {
            browseFilterOption.GenerateBtnDefaults(ViRMA_Colors.darkBlue, Color.white);

            if (browseFilterOption.name == "TagsBtn")
            {
                browseFilterOption.GetComponent<Button>().onClick.AddListener(() => globals.dimExplorer.dimExKeyboard.ToggleDimExKeyboard(true));
            }

            if (browseFilterOption.name == "TimeBtn")
            {
                browseFilterOption.GetComponent<Button>().onClick.AddListener(() => ToggleMenuSection(section_timePicker));
            }

            if (browseFilterOption.name == "LocationsBtn")
            {
                browseFilterOption.GetComponent<Button>().onClick.AddListener(() => ToggleMenuSection(section_locationPicker));
            }
        }
    }
    public void ToggleMenuSection(GameObject targetMenuSection)
    {
        foreach (GameObject menuSection in menuSections)
        {
            menuSection.SetActive(true);
            if (menuSection == targetMenuSection)
            {
                menuSection.transform.localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                menuSection.transform.localPosition = new Vector3(0, 9999, 0);
            }
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
                Tag firstNodeParent = new Tag();
                firstNodeParent.Id = axesLabels.X.Id;
                firstNodeParent.Label = axesLabels.X.Label;
                xParentChain = new List<Tag>() { firstNodeParent };

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
                Tag firstNodeParent = new Tag();
                firstNodeParent.Id = axesLabels.Y.Id;
                firstNodeParent.Label = axesLabels.Y.Label;
                yParentChain = new List<Tag>() { firstNodeParent };

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
                Tag firstNodeParent = new Tag();
                firstNodeParent.Id = axesLabels.Z.Id;
                firstNodeParent.Label = axesLabels.Z.Label;
                zParentChain = new List<Tag>() { firstNodeParent };

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
            xParentChain.AddRange(allParents);
            xParentChainFetched = true;
        }
        if (axis == "Y")
        {
            yParentChain.AddRange(allParents);
            yParentChainFetched = true;
        }
        if (axis == "Z")
        {
            zParentChain.AddRange(allParents);
            zParentChainFetched = true;
        }
    }
    private void UpdateProjFilerBtns()
    {
        GameObject templateBtn = Resources.Load("Prefabs/ProjectedFilterBtn") as GameObject;
        ViRMA_UIScrollable[] scrollableUis = ui_projectedFilters.GetComponentsInChildren<ViRMA_UIScrollable>();

        //////////////////////////////////////////////////////////////////// x 
        xBtnWrapper = scrollableUis[0].transform.GetChild(0).transform;
        foreach (Transform child in xBtnWrapper)
        {
            Destroy(child.gameObject);
        }
        xBtnWrapper.DetachChildren();

        xParentChain.Reverse();
        for (int i = 0; i < xParentChain.Count; i++)
        {
            GameObject newBtn = Instantiate(templateBtn, xBtnWrapper);
            newBtn.GetComponentInChildren<Text>().text = xParentChain[i].Label;
            newBtn.name = xParentChain[i].Label + "_" + xParentChain[i].Id;
        }
        xBtnWrapper.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;

        //////////////////////////////////////////////////////////////////// y
        yBtnWrapper = scrollableUis[1].transform.GetChild(0).transform;
        foreach (Transform child in yBtnWrapper)
        {
            Destroy(child.gameObject);
        }
        yBtnWrapper.DetachChildren();

        yParentChain.Reverse();
        for (int i = 0; i < yParentChain.Count; i++)
        {
            GameObject newBtn = Instantiate(templateBtn, yBtnWrapper);
            newBtn.GetComponentInChildren<Text>().text = yParentChain[i].Label;
            newBtn.name = yParentChain[i].Label + "_" + yParentChain[i].Id;
        }

        //////////////////////////////////////////////////////////////////// z
        zBtnWrapper = scrollableUis[2].transform.GetChild(0).transform;
        foreach (Transform child in zBtnWrapper)
        {
            Destroy(child.gameObject);
        }
        zBtnWrapper.DetachChildren();

        zParentChain.Reverse();
        for (int i = 0; i < zParentChain.Count; i++)
        {
            GameObject newBtn = Instantiate(templateBtn, zBtnWrapper);
            newBtn.GetComponentInChildren<Text>().text = zParentChain[i].Label;
            newBtn.name = zParentChain[i].Label + "_" + zParentChain[i].Id;
        }

        // set appearance and state of buttons
        SetProjFilterBtnStates();

        // scroll to the bottom of the container by default
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
        ViRMA_UIScrollable[] scrollableUis = ui_projectedFilters.GetComponentsInChildren<ViRMA_UIScrollable>();
        for (int i = 0; i < scrollableUis.Length; i++)
        {
            Button[] buttons = scrollableUis[i].GetComponentsInChildren<Button>();
            for (int j = 0; j < buttons.Length; j++)
            {
                GameObject buttonObj = buttons[j].gameObject;
                buttonObj.transform.localScale = Vector3.one * 0.9f;
                Text btnText = buttonObj.GetComponentInChildren<Text>();
                Image btnBackground = buttonObj.GetComponent<Image>();
                ViRMA_UiElement vrUiElement = buttonObj.GetComponent<ViRMA_UiElement>();               

                if (i == 0)
                {
                    // x
                    if (j == buttons.Length - 1)
                    {
                        buttonObj.transform.localScale = Vector3.one;
                        vrUiElement.GenerateBtnDefaults(ViRMA_Colors.axisRed, Color.white);
                        buttonObj.GetComponent<Button>().onClick.AddListener(() => RemoveAxis(buttonObj));
                    }
                    else
                    {
                        vrUiElement.GenerateBtnDefaults(ViRMA_Colors.BrightenColor(ViRMA_Colors.axisRed), Color.white);
                        buttonObj.GetComponent<Button>().onClick.AddListener(() => RollUpAxis(buttonObj));
                    }
                }
                else if (i == 1)
                {
                    // y
                    if (j == buttons.Length - 1)
                    {
                        buttonObj.transform.localScale = Vector3.one;
                        vrUiElement.GenerateBtnDefaults(ViRMA_Colors.axisGreen, Color.white);
                        buttonObj.GetComponent<Button>().onClick.AddListener(() => RemoveAxis(buttonObj));
                    }
                    else
                    {
                        vrUiElement.GenerateBtnDefaults(ViRMA_Colors.BrightenColor(ViRMA_Colors.axisGreen), Color.white);
                        buttonObj.GetComponent<Button>().onClick.AddListener(() => RollUpAxis(buttonObj));
                    }
                }
                else if (i == 2)
                {
                    // z
                    if (j == buttons.Length - 1)
                    {
                        buttonObj.transform.localScale = Vector3.one;
                        vrUiElement.GenerateBtnDefaults(ViRMA_Colors.axisBlue, Color.white);
                        buttonObj.GetComponent<Button>().onClick.AddListener(() => RemoveAxis(buttonObj));
                    }
                    else
                    {
                        vrUiElement.GenerateBtnDefaults(ViRMA_Colors.BrightenColor(ViRMA_Colors.axisBlue), Color.white);
                        buttonObj.GetComponent<Button>().onClick.AddListener(() => RollUpAxis(buttonObj));
                    }
                }
            }
        }
    }

    // onClick options
    private void RollUpAxis(GameObject buttonObj)
    {
        int idIndex = buttonObj.name.IndexOf("_");
        int targetId = int.Parse(buttonObj.name.Substring(idIndex + 1));
        string axisType = buttonObj.transform.parent.parent.name.Substring(0, 1);

        globals.queryController.buildingQuery.SetAxis(axisType, targetId, "node");
    }
    private void RemoveAxis(GameObject buttonObj)
    {
        string axisType = buttonObj.transform.parent.parent.name.Substring(0, 1);

        globals.queryController.buildingQuery.ClearAxis(axisType);
    }

    // direct filters
    public void FetchDirectFilterMetadata()
    {
        GameObject directFilterPrefab = Resources.Load("Prefabs/DirectFilterOptn") as GameObject;

        Transform directFilterParent = ui_directFilers.GetComponentInChildren<ViRMA_UIScrollable>().transform.GetChild(0);
        foreach (Transform child in directFilterParent)
        {
            Destroy(child.gameObject);
        }
        directFilterParent.DetachChildren();

        foreach (Query.Filter activeFilter in globals.queryController.activeFilters)
        {
            if (activeFilter.Type == "node")
            {
                int targetId = activeFilter.Ids[0];
                StartCoroutine(ViRMA_APIController.GetHierarchyTag(targetId, (tagData) => {
                    GameObject directFilterObj = Instantiate(directFilterPrefab, directFilterParent);
                    directFilterObj.GetComponent<ViRMA_DirectFilterOption>().directFilterData = tagData;
                    directFilterObj.GetComponent<ViRMA_DirectFilterOption>().labelText.text = tagData.Label;
                })); 
            }
        }
    }
    private void CheckActiveDirectFilterOptions()
    {
        directFilterOptions = ui_directFilers.GetComponentsInChildren<ViRMA_UiElement>();

        if (hoveredDirectFilter)
        {
            foreach (ViRMA_UiElement filterOption in directFilterOptions)
            {
                if (filterOption.transform.parent.gameObject == hoveredDirectFilter)
                {
                    filterOption.Hide(false);
                }
                else
                {
                    filterOption.Hide(true);
                }
            }
        }
        else
        {
            foreach (ViRMA_UiElement filterOption in directFilterOptions)
            {
                filterOption.Hide(true);
            }
        }
    }

    // time picker
    private void SetupTimePicker()
    {
        Debug.Log("Setting up time picker...");
    }

    // general
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
    public void ToggleLoadingIndicator()
    {
        GameObject templateBtn = Resources.Load("Prefabs/ProjectedFilterBtn") as GameObject;
        ViRMA_UIScrollable[] scrollableUis = ui_projectedFilters.GetComponentsInChildren<ViRMA_UIScrollable>();

        // x
        xBtnWrapper = scrollableUis[0].transform.GetChild(0).transform;
        foreach (Transform child in xBtnWrapper)
        {
            Destroy(child.gameObject);
        }
        xBtnWrapper.DetachChildren();

        GameObject xBtn = Instantiate(templateBtn, xBtnWrapper);
        xBtn.GetComponentInChildren<Image>().color = Color.grey;
        xBtn.GetComponentInChildren<Text>().color = Color.white;
        xBtn.GetComponentInChildren<Text>().text = "loading";
        xBtn.name = "loadingBtn";
        Destroy(xBtn.GetComponentInChildren<ViRMA_UiElement>());
        Destroy(xBtn.GetComponentInChildren<Outline>());

        // y
        yBtnWrapper = scrollableUis[1].transform.GetChild(0).transform;
        foreach (Transform child in yBtnWrapper)
        {
            Destroy(child.gameObject);
        }
        yBtnWrapper.DetachChildren();

        GameObject yBtn = Instantiate(templateBtn, yBtnWrapper);
        yBtn.GetComponentInChildren<Image>().color = Color.grey;
        yBtn.GetComponentInChildren<Text>().color = Color.white;
        yBtn.GetComponentInChildren<Text>().text = "loading";
        yBtn.name = "loadingBtn";
        Destroy(yBtn.GetComponentInChildren<ViRMA_UiElement>());
        Destroy(yBtn.GetComponentInChildren<Outline>());

        // z
        zBtnWrapper = scrollableUis[2].transform.GetChild(0).transform;
        foreach (Transform child in zBtnWrapper)
        {
            Destroy(child.gameObject);
        }
        zBtnWrapper.DetachChildren();

        GameObject zBtn = Instantiate(templateBtn, zBtnWrapper);
        zBtn.GetComponentInChildren<Image>().color = Color.grey;
        zBtn.GetComponentInChildren<Text>().color = Color.white;
        zBtn.GetComponentInChildren<Text>().text = "loading";
        zBtn.name = "loadingBtn";
        Destroy(zBtn.GetComponentInChildren<ViRMA_UiElement>());
        Destroy(zBtn.GetComponentInChildren<Outline>());

        // filter queries
        GameObject directFilterPrefab = Resources.Load("Prefabs/DirectFilterOptn") as GameObject;
        Transform directFilterParent = ui_directFilers.GetComponentInChildren<ViRMA_UIScrollable>().transform.GetChild(0);
        foreach (Transform child in directFilterParent)
        {
            Destroy(child.gameObject);
        }
        directFilterParent.DetachChildren();

        GameObject directFilterObj = Instantiate(directFilterPrefab, directFilterParent);
        directFilterObj.GetComponent<ViRMA_DirectFilterOption>().labelText.text = "loading";
        directFilterObj.GetComponentInChildren<Image>().color = Color.grey;
        directFilterObj.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        Destroy(directFilterObj.GetComponentInChildren<ViRMA_UiElement>());
    }

    // testing
    private void GenerateTestScrollBtns()
    {
        ViRMA_UIScrollable[] scrollableUis = ui_projectedFilters.GetComponentsInChildren<ViRMA_UIScrollable>();
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
        ui_projectedFilters.SetActive(false);
    }

    

}
