using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using TMPro;

public class ViRMA_Timeline : MonoBehaviour
{
    public float timelineScale;
    public float childRelativeSpacing;
    public float timelinePositionDistance;
    public int resultsRenderSize;
    public int contextTimelineTimespan;
    private Coroutine metadataFetcher;

    private ViRMA_GlobalsAndActions globals;
    public Rigidbody timelineRb;

    public Cell timelineCellData;
    public AxesLabels activeVizLabels;

    public int currentTimelineSection;
    public int totalTimeLineSections;
    public int totalContextTimelineSections;
    public List<KeyValuePair<int, string>> cellContentResults;
    public List<KeyValuePair<int, string>> contextTimelineResults;
    private int targetContextTimelineChildId;
    public GameObject targetContextTimelineChild;
    public int savedTimelineSection;
    public int savedTimelineChildId;

    public GameObject timelineChildrenWrapper;
    public GameObject timelineChildPrefab;
    public GameObject timelineNavPrefab;  
    public GameObject nextBtn;
    public GameObject prevBtn;
    private GameObject firstRealChild;
    private GameObject lastRealChild;
    public List<GameObject> timelineSectionChildren;
    public GameObject hoveredChild;
    public GameObject hoveredContextMenuBtn; 

    private Vector3 activeTimelinePosition;
    private Quaternion activeTImelineRotation;
    private Vector3 maxLeft;
    private Vector3 maxRight;
    private float distToMaxRight;
    private float distToMaxLeft;  

    public bool timelineLoaded;
    public bool isContextTimeline;

    private GameObject feedback;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        timelineRb = GetComponent<Rigidbody>();
        timelineLoaded = false;

        activeTimelinePosition = Vector3.one * Mathf.Infinity;
        activeTImelineRotation = Quaternion.identity;

        timelineScale = 0.3f; // global scale of timeline
        childRelativeSpacing = 0.25f; // % width of the child to space by
        timelinePositionDistance = 0.6f; // how far away to place the timeline in front of user
        resultsRenderSize = 100; // max results to render at a time
        contextTimelineTimespan = 60; // number of minutes on each side of target for context timeline
    }
    private void Start()
    {
        timelineChildPrefab = Resources.Load("Prefabs/CellPrefab") as GameObject;
        timelineNavPrefab = Resources.Load("Prefabs/TimelineNavPrefab") as GameObject;
    }
    private void Update()
    {
        if (timelineLoaded)
        {
            TimelineMovement();

            TimelineMovementLimiter();
        }
    }
    private void TimelineMovement()
    {
        Hand activeHand = null;

        if (globals.timeline_Scroll.GetState(SteamVR_Input_Sources.RightHand))
        {
            activeHand = Player.instance.rightHand;
        }
        if (globals.timeline_Scroll.GetState(SteamVR_Input_Sources.LeftHand))
        {
            activeHand = Player.instance.leftHand;
        }
        if (globals.timeline_Scroll.GetState(SteamVR_Input_Sources.RightHand) && globals.dimExplorer_Scroll.GetState(SteamVR_Input_Sources.LeftHand))
        {
            activeHand = null;
        }

        if (activeHand)
        {
            Vector3 handVelocity = transform.InverseTransformDirection(activeHand.GetTrackedObjectVelocity());
            handVelocity.y = 0;
            handVelocity.z = 0;       
            timelineRb.velocity = transform.TransformDirection(handVelocity);
        }

        if (timelineRb.velocity.magnitude < 0.2f)
        {
            timelineRb.velocity = Vector3.zero;
        }
    }
    private void TimelineMovementLimiter()
    {
        if (Player.instance)
        {
            int timelinePosChecker = 0;

            float distToMaxRightTemp = Vector3.Distance(maxLeft, transform.position);
            if (distToMaxRightTemp < distToMaxRight)
            {
                distToMaxRight = distToMaxRightTemp;
            }
            else if (distToMaxRightTemp > distToMaxRight)
            {
                distToMaxRight = distToMaxRightTemp;
                timelinePosChecker++;
            }

            float distToMaxLeftTemp = Vector3.Distance(maxRight, transform.position);
            if (distToMaxLeftTemp < distToMaxLeft)
            {
                distToMaxLeft = distToMaxLeftTemp;
            }
            else if (distToMaxLeftTemp > distToMaxLeft)
            {
                distToMaxLeft = distToMaxLeftTemp;
                timelinePosChecker++;
            }

            /*
            float distToMaxRightTemp = Vector3.Distance(maxRight, transform.position);
            if (distToMaxRightTemp < distToMaxRight)
            {
                distToMaxRight = distToMaxRightTemp;
            }
            else if (distToMaxRightTemp > distToMaxRight)
            {
                distToMaxRight = distToMaxRightTemp;
                timelinePosChecker++;
            }

            float distToMaxLeftTemp = Vector3.Distance(maxLeft, transform.position);
            if (distToMaxLeftTemp < distToMaxLeft)
            {
                distToMaxLeft = distToMaxLeftTemp;
            }
            else if (distToMaxLeftTemp > distToMaxLeft)
            {
                distToMaxLeft = distToMaxLeftTemp;
                timelinePosChecker++;
            }
            */

            if (timelinePosChecker > 1)
            {
                timelineRb.velocity = Vector3.zero;
            }
        }
    }


    // general
    public void ClearTimeline(bool hardReset = false)
    {        
        // flad as timeline as unloaded
        timelineLoaded = false;

        // clear list of active children
        timelineSectionChildren.Clear();

        // if still fetching metadata, stop that
        if (metadataFetcher != null)
        {
            StopCoroutine(metadataFetcher);
        }     

        // destroy wrapper
        if (timelineChildrenWrapper)
        {        
            Destroy(timelineChildrenWrapper);
            transform.DetachChildren();
            timelineChildrenWrapper = null;
        }

        // force reset of position and velocity
        timelineRb.velocity = Vector3.zero;
        timelineRb.angularVelocity = Vector3.zero;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        // hard reset of default timeline position after clear
        if (hardReset)
        {
            activeTimelinePosition = Vector3.one * Mathf.Infinity;
            activeTImelineRotation = Quaternion.identity;
        }
    }
    private void PositionTimeline(int newSectionIndex)
    {
        // if this is the first time positioning the timeline, pick a space in front of the user, otherwise use the active position and rotation
        if (activeTimelinePosition == Vector3.one * Mathf.Infinity || activeTImelineRotation == Quaternion.identity)
        {
            float distance = timelinePositionDistance;
            Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
            flattenedVector.y = 0;
            flattenedVector.Normalize();
            transform.position = Player.instance.hmdTransform.position + (flattenedVector * distance);
            transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);

            activeTimelinePosition = transform.position;
            activeTImelineRotation = transform.rotation;
        }
        else
        {
            transform.position = activeTimelinePosition;
            transform.rotation = activeTImelineRotation;
        }

        // generate max right and left positions for timeline to move between
        Transform lastChild = timelineChildrenWrapper.transform.GetChild(timelineChildrenWrapper.transform.childCount - 1);
        float offsetDistance = Vector3.Distance(transform.position, lastChild.position);
        Vector3 offset = transform.right * offsetDistance;
        maxLeft = transform.position - offset;
        maxRight = transform.position;

        // set timeline to correct position, and offset it slightly so nav buttons aren't the first thing seen
        if (newSectionIndex < currentTimelineSection)
        {
            if (nextBtn)
            {
                float distBetweenChildAndNav = Vector3.Distance(lastRealChild.transform.position, nextBtn.transform.position); 
                Vector3 childNavOffset = transform.right * distBetweenChildAndNav;
                transform.position = maxLeft + childNavOffset;
            }      
        }
        else
        {
            if (prevBtn)
            {
                float distBetweenChildAndNav = Vector3.Distance(firstRealChild.transform.position, prevBtn.transform.position);
                Vector3 childNavOffset = transform.right * distBetweenChildAndNav;
                transform.position = maxRight - childNavOffset;
            }       
        }

        // if the target context menu child is rendered, or if returning to cell contents timeline, use a child as the focus
        if (targetContextTimelineChild)
        {
            Transform firstChild = timelineChildrenWrapper.transform.GetChild(0);
            float distBetweenChildAndStart = Vector3.Distance(firstChild.position, targetContextTimelineChild.transform.position);
            Vector3 targetChildOffset = transform.right * distBetweenChildAndStart;
            transform.position = maxRight - targetChildOffset;
        }
        else if (savedTimelineChildId != 0)
        {
            for (int i = 0; i < timelineSectionChildren.Count; i++)
            {
                ViRMA_TimelineChild targetChild = timelineSectionChildren[i].GetComponent<ViRMA_TimelineChild>();
                if (targetChild.id == savedTimelineChildId)
                {
                    Transform firstChild = timelineChildrenWrapper.transform.GetChild(0);
                    float distBetweenChildAndStart = Vector3.Distance(firstChild.position, targetChild.transform.position);
                    Vector3 targetChildOffset = transform.right * distBetweenChildAndStart;
                    transform.position = maxRight - targetChildOffset;
                    //targetChild.ToggleBorder(true);
                    break;
                }
            }
        }
    }
    private IEnumerator GetTimelineSectionMetadata()
    {
        foreach (GameObject timelineSectionChild in timelineSectionChildren)
        {
            if (timelineSectionChild != null)
            {
                int targetId = timelineSectionChild.GetComponent<ViRMA_TimelineChild>().id;
                yield return metadataFetcher = StartCoroutine(ViRMA_APIController.GetTimelineMetadata(targetId, (metadata) => {

                    //var testing = String.Join(" | ", metadata.ToArray());
                    //Debug.Log(targetId + " : " + testing);

                    if (timelineSectionChild != null)
                    {
                        timelineSectionChild.GetComponent<ViRMA_TimelineChild>().tags = metadata;
                    }
                }));
            }          
        }
        metadataFetcher = null;
    }
    private void ToggleTimelineSectionNavigation(int newSectionIndex, int totalSections)
    {
        if (nextBtn)
        {
            nextBtn.transform.parent = null;
            Destroy(nextBtn);
            nextBtn = null;
        }

        if (prevBtn)
        {
            prevBtn.transform.parent = null;
            Destroy(prevBtn);
            prevBtn = null;
        }

        if (newSectionIndex < totalSections - 1)
        {
            nextBtn = Instantiate(timelineNavPrefab);
            nextBtn.AddComponent<ViRMA_TimelineChild>().isNextBtn = true;
            nextBtn.GetComponentInChildren<TextMesh>().text = "Next\n(" + (newSectionIndex + 2) + "/" + totalSections + ")";
            nextBtn.GetComponent<Renderer>().material.SetColor("_Color", ViRMA_Colors.axisDarkBlue);
            nextBtn.name = "NextBtn";
            nextBtn.transform.parent = timelineChildrenWrapper.transform;
            nextBtn.transform.SetAsLastSibling();
            nextBtn.transform.localScale = new Vector3(1, 1, 0.01f);
            Vector3 nextBtnPos = new Vector3(lastRealChild.transform.localPosition.x + lastRealChild.transform.localScale.x, lastRealChild.transform.localPosition.y, lastRealChild.transform.localPosition.z);
            nextBtn.transform.localPosition = nextBtnPos;
        }

        if (newSectionIndex > 0)
        {
            prevBtn = Instantiate(timelineNavPrefab);
            prevBtn.AddComponent<ViRMA_TimelineChild>().isPrevBtn = true;
            prevBtn.GetComponentInChildren<TextMesh>().text = "Previous\n(" + newSectionIndex + "/" + totalSections + ")";
            prevBtn.GetComponent<Renderer>().material.SetColor("_Color", ViRMA_Colors.axisDarkBlue);
            prevBtn.name = "PrevBtn";
            prevBtn.transform.parent = timelineChildrenWrapper.transform;
            prevBtn.transform.SetAsFirstSibling();
            prevBtn.transform.localScale = new Vector3(1, 1, 0.01f);
            float adjustment = firstRealChild.transform.localScale.x;
            for (int i = 0; i < timelineChildrenWrapper.transform.childCount; i++)
            {
                Transform adjustedChild = timelineChildrenWrapper.transform.GetChild(i);
                adjustedChild.transform.localPosition = new Vector3(adjustedChild.transform.localPosition.x + adjustment, adjustedChild.transform.localPosition.y, adjustedChild.transform.localPosition.z);
            }
            prevBtn.transform.localPosition = Vector3.zero;
        }
    }
    private void LoadTimelineSection(int sectionIndex, int totalSections)
    {
        // hide main menu and viz
        globals.mainMenu.ToggleMainMenu(false);
        globals.vizController.HideViz(true);

        // clear old timeline if one exists
        ClearTimeline();

        // create wrapper
        timelineChildrenWrapper = new GameObject("TimelineChildrenWrapper");
        timelineChildrenWrapper.transform.parent = transform;

        // choose correct data depending on type of timeline
        List<KeyValuePair<int, string>> loadedTimelineResults;
        if (isContextTimeline)
        {
            loadedTimelineResults = contextTimelineResults;
        }
        else
        {
            loadedTimelineResults = cellContentResults;
        }

        // calculate correct start and end indexes to render correct section results     
        int startIndex = sectionIndex * resultsRenderSize;
        int resultsToShow = resultsRenderSize;     
        if ((startIndex + resultsToShow) > loadedTimelineResults.Count)
        {
            resultsToShow = loadedTimelineResults.Count - startIndex;
        }

        // get section range from results and render it
        targetContextTimelineChild = null;
        List<KeyValuePair<int, string>> currentResultsSection = loadedTimelineResults.GetRange(startIndex, resultsToShow);
        for (int i = 0; i < currentResultsSection.Count; i++)
        {
            // create timeline child game object using cell prefab
            GameObject timelineChild = Instantiate(timelineChildPrefab);
            timelineChild.AddComponent<ViRMA_TimelineChild>().LoadTimelineChild(currentResultsSection[i].Key, currentResultsSection[i].Value);

            // if this is a context timeline, reference the target child to be focused on
            if (isContextTimeline && targetContextTimelineChildId == currentResultsSection[i].Key)
            {
                targetContextTimelineChild = timelineChild;
            }

            // set wrapper as parent and adjust scale for image aspect ratio
            timelineChild.transform.parent = timelineChildrenWrapper.transform;
            timelineChild.transform.localScale = new Vector3(1.5f, 1, 0.01f);

            // set the distance between children as a % width of the child
            float childWidth = timelineChild.transform.localScale.x;
            float offset = childWidth * childRelativeSpacing * i;
            float xPos = offset + (childWidth * i);
            timelineChild.transform.position = new Vector3(xPos, 0, 0);

            // grab first and last real children in timeline (i.e. not nav btns)
            if (i == 0)
            {
                firstRealChild = timelineChild;
            }
            else if (i == currentResultsSection.Count - 1)
            {
                lastRealChild = timelineChild;
            }

            // add child to list for reference
            timelineSectionChildren.Add(timelineChild);
        }

        // get associated metadata for each child in section (for non-concurrent fetch)
        //StartCoroutine(GetTimelineSectionMetadata()); 

        ToggleTimelineSectionNavigation(sectionIndex, totalSections);

        timelineChildrenWrapper.transform.localScale = Vector3.one * timelineScale;

        PositionTimeline(sectionIndex);

        currentTimelineSection = sectionIndex;

        timelineLoaded = true;
    }

    public void LoadCellContentData(GameObject submittedCell)
    {
        isContextTimeline = false;

        if (submittedCell.GetComponent<ViRMA_Cell>())
        {
            Query cellContentQuery = new Query();
            cellContentQuery.Filters = ObjectExtensions.Copy(globals.queryController.activeFilters);


            // get cell data and currently active viz axes labels
            timelineCellData = submittedCell.GetComponent<ViRMA_Cell>().thisCellData;
            activeVizLabels = globals.vizController.activeAxesLabels;

            // if X axis exits, find location of submitted call on it and grab data
            if (activeVizLabels.X != null)
            {
                int cellXPosition = (int)timelineCellData.Coordinates.x - 1;
                int cellXAxisId = activeVizLabels.X.Labels[cellXPosition].Id;
                string cellXAxisType = activeVizLabels.X.Type;

                cellContentQuery.SetAxis("X", cellXAxisId, cellXAxisType);
            }

            // if Y axis exits, find location of submitted call on it and grab data
            if (activeVizLabels.Y != null)
            {
                int cellYPosition = (int)timelineCellData.Coordinates.y - 1;
                int cellYAxisId = activeVizLabels.Y.Labels[cellYPosition].Id;
                string cellYAxisType = activeVizLabels.Y.Type;

                cellContentQuery.SetAxis("Y", cellYAxisId, cellYAxisType);
            }

            // if Z axis exits, find location of submitted call on it and grab data
            if (activeVizLabels.Z != null)
            {
                int cellZPosition = (int)timelineCellData.Coordinates.z - 1;
                int cellZAxisId = activeVizLabels.Z.Labels[cellZPosition].Id;
                string cellZAxisType = activeVizLabels.Z.Type;

                cellContentQuery.SetAxis("Z", cellZAxisId, cellZAxisType);
            }

            // get cell content image data from server and load it
            StartCoroutine(ViRMA_APIController.GetCellContents(cellContentQuery, (results) => {

                cellContentResults = results;

                if (cellContentResults.Count > 0)
                {
                    if (cellContentResults.Count >= resultsRenderSize)
                    {
                        if (cellContentResults.Count % resultsRenderSize == 0)
                        {
                            totalTimeLineSections = (cellContentResults.Count / resultsRenderSize);
                        }
                        else
                        {
                            totalTimeLineSections = (cellContentResults.Count / resultsRenderSize) + 1;
                        }
                    }
                    else
                    {
                        totalTimeLineSections = 1;
                    }

                    LoadTimelineSection(0, totalTimeLineSections);
                }

            }));
        }
    }

    public void LoadTimelineData(GameObject submittedCell)
    {
        isContextTimeline = false;

        if (submittedCell.GetComponent<ViRMA_Cell>())
        {
            // deep clone query filters for timeline API call
            List<Query.Filter> cellFiltersForTimeline = ObjectExtensions.Copy(globals.queryController.activeFilters);

            // get cell data and currently active viz axes labels
            timelineCellData = submittedCell.GetComponent<ViRMA_Cell>().thisCellData;
            activeVizLabels = globals.vizController.activeAxesLabels;
        
            // if X axis exits, find location of submitted call on it and grab data
            if (activeVizLabels.X != null)
            {
                int cellXPosition = (int) timelineCellData.Coordinates.x - 1;
                int cellXAxisId = activeVizLabels.X.Labels[cellXPosition].Id;
                string cellXAxisType = activeVizLabels.X.Type;

                Query.Filter projFilterX = new Query.Filter(cellXAxisType, new List<int>() { cellXAxisId });
                cellFiltersForTimeline.Add(projFilterX);
            }

            // if Y axis exits, find location of submitted call on it and grab data
            if (activeVizLabels.Y != null)
            {
                int cellYPosition = (int)timelineCellData.Coordinates.y - 1;
                int cellYAxisId = activeVizLabels.Y.Labels[cellYPosition].Id;
                string cellYAxisType = activeVizLabels.Y.Type;

                Query.Filter projFilterY = new Query.Filter(cellYAxisType, new List<int>() { cellYAxisId });
                cellFiltersForTimeline.Add(projFilterY);
            }

            // if Z axis exits, find location of submitted call on it and grab data
            if (activeVizLabels.Z != null)
            {
                int cellZPosition = (int)timelineCellData.Coordinates.z - 1;
                int cellZAxisId = activeVizLabels.Z.Labels[cellZPosition].Id;
                string cellZAxisType = activeVizLabels.Z.Type;

                Query.Filter projFilterZ = new Query.Filter(cellZAxisType, new List<int>() { cellZAxisId });
                cellFiltersForTimeline.Add(projFilterZ);
            }

            // get timeline image data from server and load it
            StartCoroutine(ViRMA_APIController.GetTimeline(cellFiltersForTimeline, (results) => {

                cellContentResults = results;

                if (cellContentResults.Count > 0)
                {
                    if (cellContentResults.Count >= resultsRenderSize)
                    {
                        if (cellContentResults.Count % resultsRenderSize == 0)
                        {
                            totalTimeLineSections = (cellContentResults.Count / resultsRenderSize);
                        }
                        else
                        {
                            totalTimeLineSections = (cellContentResults.Count / resultsRenderSize) + 1;
                        }
                    }
                    else
                    {
                        totalTimeLineSections = 1;
                    }

                    LoadTimelineSection(0, totalTimeLineSections);
                }          

            }));
        }
    }    
    private void LoadContextTimelineData(ViRMA_TimelineChild targetTimelineChild)
    {
        if (isContextTimeline == false)
        {
            savedTimelineSection = currentTimelineSection;
            savedTimelineChildId = targetTimelineChild.id;
        }

        StartCoroutine(ViRMA_APIController.GetContextTimeline(targetTimelineChild.timestamp, contextTimelineTimespan, (results) => {

            contextTimelineResults = results;

            if (contextTimelineResults.Count > 0)
            {
                if (contextTimelineResults.Count >= resultsRenderSize)
                {
                    if (contextTimelineResults.Count % resultsRenderSize == 0)
                    {
                        totalContextTimelineSections = (contextTimelineResults.Count / resultsRenderSize);
                    }
                    else
                    {
                        totalContextTimelineSections = (contextTimelineResults.Count / resultsRenderSize) + 1;
                    }
                }
                else
                {
                    totalContextTimelineSections = 1;
                }

                int targetSection = 0;
                for (int i = 0; i < contextTimelineResults.Count; i++)
                {
                    if (contextTimelineResults[i].Key == targetTimelineChild.id)
                    {
                        targetContextTimelineChildId = contextTimelineResults[i].Key;
                        targetSection = i / resultsRenderSize;

                        //Debug.Log(i + " divided by " + resultsRenderSize + " --> Target Section: " + targetSection);

                        break;
                    }
                }

                isContextTimeline = true;
                LoadTimelineSection(targetSection, totalContextTimelineSections);
            }        
        }));
    }
    private IEnumerator SubmissionFeedback(GameObject target, bool correct)
    {
        if (feedback == null)
        {
            GameObject feedbackPrefab = Resources.Load("Prefabs/CompetitionFeedback") as GameObject;

            feedback = Instantiate(feedbackPrefab, target.transform);
            feedback.transform.localPosition = new Vector3(0, 0.5f, -3f);
            feedback.transform.localScale = new Vector3(1 / target.transform.localScale.x, 1 / target.transform.localScale.y, 1) * 3f;

            Renderer feedbackBg = feedback.transform.GetChild(0).GetComponent<Renderer>();
            TMP_Text feedbackText = feedback.transform.GetChild(1).GetComponent<TMP_Text>();
            if (correct)
            {
                feedbackBg.material.color = ViRMA_Colors.axisDarkGreen;
                feedbackText.text = "Correct!";
            }
            else
            {
                feedbackBg.material.color = ViRMA_Colors.axisDarkRed;
                feedbackText.text = "Incorrect!";
            }

            yield return new WaitForSeconds(3);
            Destroy(feedback);
            feedback = null;
        }       
    }

    
    // steamVR actions
    public void SubmitChildForContextMenu(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (hoveredChild != null)
        {
            int totalSections = 0;
            if (isContextTimeline)
            {
                totalSections = totalContextTimelineSections;
            }
            else
            {
                totalSections = totalTimeLineSections;
            }


            if (!hoveredChild.GetComponent<ViRMA_TimelineChild>().isNextBtn && !hoveredChild.GetComponent<ViRMA_TimelineChild>().isPrevBtn)
            {
                //hoveredChild.GetComponent<ViRMA_TimelineChild>().LoadTImelineContextMenu();
            }
            else if (hoveredChild.GetComponent<ViRMA_TimelineChild>().isNextBtn)
            {
                LoadTimelineSection(currentTimelineSection + 1, totalSections);
            }
            else if (hoveredChild.GetComponent<ViRMA_TimelineChild>().isPrevBtn)
            {
                LoadTimelineSection(currentTimelineSection - 1, totalSections);
            }       
        }
    }
    public void SubmitContextMenuBtn(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (hoveredContextMenuBtn != null)
        {
            ViRMA_TimeLineContextMenuBtn btnOption = hoveredContextMenuBtn.GetComponent<ViRMA_TimeLineContextMenuBtn>();
            ViRMA_TimelineChild targetTimelineChild = btnOption.targetTimelineChild.GetComponent<ViRMA_TimelineChild>();

            if (btnOption.btnType.ToLower() == "context")
            {
                LoadContextTimelineData(targetTimelineChild);
            }

            if (btnOption.btnType.ToLower() == "submit")
            {
                string submissionId = btnOption.targetTimelineChild.GetComponent<ViRMA_TimelineChild>().fileName;
                StartCoroutine(ViRMA_CompetitionController.SubmitToLSC(submissionId, (result) => {
                    StartCoroutine(SubmissionFeedback(btnOption.targetTimelineChild, result));
                }));
            }
        }
    }
    public void BackButton(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (timelineLoaded)
        {
            if (isContextTimeline)
            {
                isContextTimeline = false;
                LoadTimelineSection(savedTimelineSection, totalTimeLineSections); 
                savedTimelineSection = 0;
                savedTimelineChildId = 0;
            }
            else
            {
                ClearTimeline(true);
                globals.vizController.HideViz(false);
            }
        }
    }

}
