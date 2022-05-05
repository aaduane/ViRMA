using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;
using TMPro;
using Valve.VR;

public class ViRMA_MainMenu : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private List<GameObject> menuSections;
    public GameObject activeSection;
    private Vector3 mainMenuPosition;
    private float mainMenuAngle;
    public bool mainMenuPositionSet;
    public bool mainMenuLoaded;
    public bool mainMenuMoving;
    public Hand handInteractingWithMainMenu;

    private int frameSkipper; // increase UPDATE performance for some functions

    ///////////////////////////////////////////////// dimension browser
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

    ///////////////////////////////////////////////// time picker
    public GameObject section_timePicker;

    public List<ViRMA_UiElement> allTimeOptions;
    
    public GameObject ui_weekdays;
    public GameObject ui_hours;
    public GameObject ui_dates;
    public GameObject ui_months;
    public GameObject ui_years;

    ///////////////////////////////////////////////// location picker
    public GameObject section_locationPicker;

    private List<ViRMA_UiElement> allLocationOptions;
    private List<ViRMA_UiElement> toggledLocationUiElements;
    
    ///////////////////////////////////////////////// custom buttons
    public List<GameObject> customButtons; 

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        menuSections = new List<GameObject>();
        menuSections.Add(section_dimensionBrowser);
        menuSections.Add(section_timePicker);
        menuSections.Add(section_locationPicker);

        // ensure all UI sections are enabled
        foreach (GameObject menuSection in menuSections)
        {
            menuSection.SetActive(true);
        }

        SetupCustomButtons();

        transform.localScale = Vector3.one * 0.75f;
    }

    private void Start()
    {
        SetupBrowseFiltersOptions();

        StartCoroutine(SetupTimePicker());

        SetupLocationPicker();

        ToggleMenuSection(section_dimensionBrowser);
    }

    private void Update()
    {
        MainMenuRepositioning();

        ProjFilterGenerateBtns();

        CheckActiveDirectFilterOptions();

        TimePickerToggleController();
    }

    // browse filters
    private void SetupBrowseFiltersOptions()
    {
        ViRMA_UiElement[] browseFilterOptions = ui_browseFilters.GetComponentsInChildren<ViRMA_UiElement>();
        foreach (ViRMA_UiElement browseFilterOption in browseFilterOptions)
        {
            if (browseFilterOption.name == "TagsBtn")
            {
                browseFilterOption.GenerateBtnDefaults(ViRMA_Colors.darkBlue, Color.white);
                browseFilterOption.GetComponent<Button>().onClick.AddListener(() => globals.dimExplorer.dimExKeyboard.ToggleDimExKeyboard(true));
                browseFilterOption.GetComponent<Button>().onClick.AddListener(() => ToggleMainMenu(false));
            }

            if (browseFilterOption.name == "TimeBtn")
            {
                browseFilterOption.GenerateBtnDefaults(ViRMA_Colors.darkBlue, Color.white);
                browseFilterOption.GetComponent<Button>().onClick.AddListener(() => ToggleMenuSection(section_timePicker));
            }

            if (browseFilterOption.name == "LocationsBtn")
            {
                //browseFilterOption.GenerateBtnDefaults(ViRMA_Colors.darkBlue, Color.white);
                //browseFilterOption.GetComponent<Button>().onClick.AddListener(() => ToggleMenuSection(section_locationPicker));
            }
        }
    }
    public void ToggleMenuSection(GameObject targetMenuSection)
    {
        foreach (GameObject menuSection in menuSections)
        {
            if (menuSection == targetMenuSection)
            {
                menuSection.transform.localPosition = new Vector3(0, 0, 0);
                activeSection = menuSection;
            }
            else
            {
                menuSection.transform.localPosition = new Vector3(0, 0, 999999);
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
    private void ProjFilterGenerateBtns()
    {
        if (xParentChainFetched && yParentChainFetched && zParentChainFetched)
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
                newBtn.GetComponentInChildren<TMP_Text>().text = xParentChain[i].Label;
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
                newBtn.GetComponentInChildren<TMP_Text>().text = yParentChain[i].Label;
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
                newBtn.GetComponentInChildren<TMP_Text>().text = zParentChain[i].Label;
                newBtn.name = zParentChain[i].Label + "_" + zParentChain[i].Id;
            }

            // set appearance and state of buttons
            SetProjFilterBtnStates();

            // scroll to the bottom of the container by default
            StartCoroutine(ScrollBtnWrappersToBottom());

            xParentChainFetched = false;
            yParentChainFetched = false;
            zParentChainFetched = false;
        }   
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
                    directFilterObj.GetComponent<ViRMA_DirectFilterOption>().filterType = "node";
                })); 
            }
            else if (activeFilter.Type == "tag")
            {
                int parentIdIndex = activeFilter.FilterId.IndexOf("_");
                string parentId = activeFilter.FilterId.Substring(parentIdIndex + 1);
                StartCoroutine(ViRMA_APIController.GetTagset(parentId, (tagsetData) => {
                    foreach (Tag tagData in tagsetData)
                    {
                        foreach (int id in activeFilter.Ids)
                        {
                            if (tagData.Id == id)
                            {
                                GameObject directFilterObj = Instantiate(directFilterPrefab, directFilterParent);
                                directFilterObj.GetComponent<ViRMA_DirectFilterOption>().filterType = "tag";
                                directFilterObj.GetComponent<ViRMA_DirectFilterOption>().directFilterData = tagData;

                                string adjustLabel = tagData.Label;

                                // adjust appearance of hour tags as direct filters
                                if (tagData.Parent.Label == "Hour")
                                {
                                    if (tagData.Label.Length == 1)
                                    {
                                        adjustLabel = "0" + tagData.Label + ":00";
                                    }
                                    else
                                    {
                                        adjustLabel = tagData.Label + ":00";
                                    }
                                }

                                // adjust the appearance of date tags as direct filters
                                if (tagData.Parent.Label == "Day within month")
                                {
                                    if (tagData.Label == "1") 
                                    {
                                        adjustLabel = "1st";
                                    }
                                    else if (tagData.Label == "2")
                                    {
                                        adjustLabel = "2nd";
                                    }
                                    else if (tagData.Label == "3")
                                    {
                                        adjustLabel = "3rd";
                                    }
                                    else
                                    {
                                        adjustLabel = tagData.Label + "th";
                                    }
                                }

                                directFilterObj.GetComponent<ViRMA_DirectFilterOption>().labelText.text = adjustLabel;
                            }
                        }
                    }
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

    private IEnumerator SetupTimePicker()
    {
        // current targset time picker tagsets
        List<string> timeTagsets = new List<string> { "Day of week (string)", "Month (string)", "Year", "Hour", "Day within month" };

        // get all time tagsets corresponding to list above 
        List<Tag> timeTagsetsData = new List<Tag>();
        yield return StartCoroutine(ViRMA_APIController.GetTagset("", (tagsetsData) => {
            for (int i = tagsetsData.Count - 1; i > -1; i--)
            {
                if (!timeTagsets.Contains(tagsetsData[i].Label))
                {
                    tagsetsData.RemoveAt(i);
                }
            }
            timeTagsetsData = tagsetsData;
        }));

        // populate tagsets with their children
        foreach (Tag tagsetData in timeTagsetsData)
        {
            yield return StartCoroutine(ViRMA_APIController.GetTagset(tagsetData.Id.ToString(), (tagData) => {
                tagsetData.Children = tagData;
            }));
        }

        foreach (Tag timeTagset in timeTagsetsData)
        {
            // generate day of the week options
            if (timeTagset.Label == "Day of week (string)")
            {
                foreach (Transform weekdayObj in ui_weekdays.transform)
                {
                    ViRMA_UiElement uiElement = weekdayObj.GetComponent<ViRMA_UiElement>();
                    allTimeOptions.Add(uiElement);

                    string weekdayLabel = weekdayObj.GetComponentInChildren<Text>().text;
                    foreach (Tag weekdayTag in timeTagset.Children)
                    {
                        if (weekdayTag.Label.Substring(0, 3) == weekdayLabel)
                        {         
                            uiElement.buttonData = weekdayTag;
                            weekdayObj.GetComponent<Button>().onClick.AddListener(() => ToggleTimeOption(uiElement));                  
                        }
                    }
                }
            }

            // generate hour options
            if (timeTagset.Label == "Hour")
            {
                foreach (Transform hourObj in ui_hours.transform)
                {
                    ViRMA_UiElement uiElement = hourObj.GetComponent<ViRMA_UiElement>();
                    allTimeOptions.Add(uiElement);
                    string hourLabel = hourObj.GetComponentInChildren<Text>().text;
                    int hour = int.Parse(hourLabel.Substring(0, hourLabel.IndexOf(":")));
                    foreach (Tag hourTag in timeTagset.Children)
                    {
                        if (hourTag.Label == hour.ToString())
                        {              
                            uiElement.buttonData = hourTag;
                            hourObj.GetComponent<Button>().onClick.AddListener(() => ToggleTimeOption(uiElement));   
                        }
                    }
                }
            }

            // generate date options
            if (timeTagset.Label == "Day within month")
            {
                foreach (Transform dateObj in ui_dates.transform)
                {
                    ViRMA_UiElement uiElement = dateObj.GetComponent<ViRMA_UiElement>();
                    allTimeOptions.Add(uiElement);
                    string dateLabel = dateObj.GetComponentInChildren<Text>().text;
                    int date = int.Parse(dateLabel);
                    foreach (Tag dateTag in timeTagset.Children)
                    {
                        if (dateTag.Label == date.ToString())
                        {
                            uiElement.buttonData = dateTag;
                            dateObj.GetComponent<Button>().onClick.AddListener(() => ToggleTimeOption(uiElement));
                        }
                    }
                }
            }

            // generate month options
            if (timeTagset.Label == "Month (string)")
            {
                foreach (Transform monthObj in ui_months.transform)
                {
                    ViRMA_UiElement uiElement = monthObj.GetComponent<ViRMA_UiElement>();
                    allTimeOptions.Add(uiElement);
                    string monthLabel = monthObj.GetComponentInChildren<Text>().text;
                    foreach (Tag monthTag in timeTagset.Children)
                    {
                        string shortMonthLabel = monthTag.Label.Substring(0, 3);
                        if (shortMonthLabel == monthLabel)
                        {                
                            uiElement.buttonData = monthTag;
                            monthObj.GetComponent<Button>().onClick.AddListener(() => ToggleTimeOption(uiElement));
                        }
                    }
                }
            }

            // generate year options
            if (timeTagset.Label == "Year")
            {
                foreach (Transform yearObj in ui_years.transform)
                {
                    ViRMA_UiElement uiElement = yearObj.GetComponent<ViRMA_UiElement>();
                    allTimeOptions.Add(uiElement);
                    string yearLabel = yearObj.GetComponentInChildren<Text>().text;
                    foreach (Tag yearTag in timeTagset.Children)
                    {
                        if (yearLabel == yearTag.Label)
                        {
                            uiElement.buttonData = yearTag;
                            yearObj.GetComponent<Button>().onClick.AddListener(() => ToggleTimeOption(uiElement));
                        }
                    }
                }
            }

            // if any time options do not exist in the DB, then reflect that in the button style
            foreach (ViRMA_UiElement timeOption in allTimeOptions)
            {
                if (timeOption.buttonData == null)
                {
                    timeOption.GenerateBtnDefaults(ViRMA_Colors.lightGrey, Color.white, true);
                }
            }
        }
    }
    private void TimePickerToggleController()
    {
        // do every second frame for performance
        frameSkipper++;
        if (frameSkipper < 2)
        {
            return;
        }
        frameSkipper = 0;

        foreach (ViRMA_UiElement timeOption in allTimeOptions)
        {
            bool toToggle = false;
            foreach (Query.Filter activeFilter in globals.queryController.buildingQuery.Filters)
            {
                if (activeFilter.FilterId != "null_0")
                {
                    foreach (int activeTagId in activeFilter.Ids)
                    {
                        if (timeOption.buttonData != null)
                        {
                            Tag timeTagData = (Tag)timeOption.buttonData;
                            if (timeTagData.Id == activeTagId)
                            {
                                toToggle = true;
                            }
                        }           
                    }
                }
            }

            timeOption.toggle = toToggle;
        }
    }
    public void ToggleTimeOption(ViRMA_UiElement uiElement)
    {
        if (uiElement.buttonData != null)
        {
            Tag timeTagData = (Tag)uiElement.buttonData;
            if (uiElement.isToggled)
            {
                globals.queryController.buildingQuery.RemoveFilter(timeTagData.Id, "tag", timeTagData.Parent.Id);
            }
            else
            {
                globals.queryController.buildingQuery.AddFilter(timeTagData.Id, "tag", timeTagData.Parent.Id);
            }      
        }
    }

    // location picker
    private void SetupLocationPicker()
    {
        allLocationOptions = new List<ViRMA_UiElement>();
        toggledLocationUiElements = new List<ViRMA_UiElement>();
    }

    // general
    public void ToggleMainMenu(bool toShow)
    {
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
            
            // save menu position if already moved
            if (mainMenuPositionSet)
            {
                transform.position = new Vector3(spawnPos.x, mainMenuPosition.y, spawnPos.z);    
                
                transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
                Vector3 currentRot = transform.rotation.eulerAngles;
                currentRot.x = mainMenuAngle;
                Quaternion newRot = Quaternion.Euler(currentRot);
                transform.rotation = newRot;
            }
            else
            {
                transform.position = spawnPos;
                transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
            }
        }
        else
        {
            transform.parent = null;
            transform.position = new Vector3(0, 9999, 0);
            transform.rotation = Quaternion.identity;
        }

        mainMenuLoaded = toShow;
    }
    public void ToggleMainMenuAlias(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        // toggle main menu as long as timeline isn't loaded as it shares the same button
        if (globals.timeline.timelineLoaded)
        {
            ToggleMainMenu(false);
        }
        else
        {
            if (mainMenuLoaded)
            {
                // if not on dim browser, use back button to return to dim browser
                if (activeSection != section_dimensionBrowser)
                {
                    ToggleMenuSection(section_dimensionBrowser);
                }
                else
                {
                    mainMenuLoaded = !mainMenuLoaded;
                    ToggleMainMenu(mainMenuLoaded);
                }
            }
            else
            {
                mainMenuLoaded = !mainMenuLoaded;
                ToggleMainMenu(mainMenuLoaded);
            }
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
        xBtn.GetComponentInChildren<TMP_Text>().color = Color.white;
        xBtn.GetComponentInChildren<TMP_Text>().text = "loading";
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
        yBtn.GetComponentInChildren<TMP_Text>().color = Color.white;
        yBtn.GetComponentInChildren<TMP_Text>().text = "loading";
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
        zBtn.GetComponentInChildren<TMP_Text>().color = Color.white;
        zBtn.GetComponentInChildren<TMP_Text>().text = "loading";
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
    private void SetupCustomButtons()
    {
        foreach (GameObject customMenuBtn in customButtons)
        {
            customMenuBtn.GetComponent<Button>().onClick.AddListener(() => SubmitCustomMenuBtn(customMenuBtn));

            if (customMenuBtn.name == "RepositionBtn")
            {
                customMenuBtn.GetComponent<ViRMA_UiElement>().GenerateBtnDefaults(ViRMA_Colors.grey, Color.white);
            }
            if (customMenuBtn.name == "TimeBackBtn")
            {
                customMenuBtn.GetComponent<ViRMA_UiElement>().GenerateBtnDefaults(ViRMA_Colors.grey, Color.white);
            }
            if (customMenuBtn.name == "ClearTimesBtn")
            {
                customMenuBtn.GetComponent<ViRMA_UiElement>().GenerateBtnDefaults(ViRMA_Colors.flatOrange, Color.white);
            }
            if (customMenuBtn.name == "ClearAllBtn")
            {
                customMenuBtn.GetComponent<ViRMA_UiElement>().GenerateBtnDefaults(ViRMA_Colors.flatOrange, Color.white);
            }
        }
    }
    public void SubmitCustomMenuBtn(GameObject customMenuBtn)
    {
        if (customMenuBtn.name == "RepositionBtn")
        {
            handInteractingWithMainMenu = customMenuBtn.GetComponent<ViRMA_UiElement>().handINteractingWithUi;
            mainMenuMoving = true;
        }
        if (customMenuBtn.name == "TimeBackBtn")
        {
            ToggleMenuSection(section_dimensionBrowser);
        }
        if (customMenuBtn.name == "ClearAllBtn")
        {
            globals.queryController.buildingQuery.ClearAxis("X", true);
            globals.queryController.buildingQuery.ClearAxis("Y", true);
            globals.queryController.buildingQuery.ClearAxis("Z", true);
            globals.queryController.buildingQuery.ClearFilters();
        }
        if (customMenuBtn.name == "ClearTimesBtn")
        {
            foreach (ViRMA_UiElement timeOption in allTimeOptions)
            {
                if (timeOption.isToggled)
                {
                    Tag timeTagData = (Tag)timeOption.buttonData;
                    globals.queryController.buildingQuery.RemoveFilter(timeTagData.Id, "tag", timeTagData.Parent.Id);
                    timeOption.toggle = false;
                }
            }
        }
    }
    private void MainMenuRepositioning()
    {
        if (mainMenuMoving)
        {
            if (globals.menuInteraction_Select.GetState(handInteractingWithMainMenu.handType))
            {
                if (handInteractingWithMainMenu)
                {
                    if (transform.parent != handInteractingWithMainMenu)
                    {
                        transform.parent = handInteractingWithMainMenu.transform;

                        mainMenuPosition = transform.position;
                        mainMenuAngle = transform.rotation.eulerAngles.x;
                        mainMenuPositionSet = true;
                    }
                }
            }
            else
            {
                if (handInteractingWithMainMenu)
                {
                    if (transform.parent == handInteractingWithMainMenu.transform)
                    {
                        transform.parent = null;
                        mainMenuMoving = false;
                    }
                }
            }
        }
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
