using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_Timeline : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private Rigidbody timelineRb;
    public GameObject timelineChildrenWrapper;
    public GameObject nextBtn;
    public GameObject prevBtn;
    public GameObject hoveredChild;
    public GameObject hoveredContextMenuBtn;

    public GameObject timelineChildPrefab;
    public GameObject timelineNavPrefab;

    public Cell timelineCellData;
    public AxesLabels activeVizLabels;
    public List<KeyValuePair<int, string>> timelineResults;

    public float timelineScale;
    public float childRelativeSpacing;
    public float timelinePositionDistance;

    private Vector3 activeTimelinePosition;
    private Quaternion activeTImelineRotation;

    int resultsRenderSize;
    int resultsPagesTotal;
    int currentResultsPage;

    public Bounds timelineBounds;
    private Vector3 maxRight;
    private Vector3 maxLeft;
    private float distToMaxRight;
    private float distToMaxLeft;
    public bool timelineLoaded;   

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        timelineRb = GetComponent<Rigidbody>();
        timelineLoaded = false;

        timelineScale = 0.3f; // global scale of timeline
        childRelativeSpacing = 0.25f; // % width of the child to space by
        timelinePositionDistance = 0.6f; // how far away to place the timeline

        resultsRenderSize = 10; // max results to render at a time

        activeTimelinePosition = Vector3.one * Mathf.Infinity;
        activeTImelineRotation = Quaternion.identity;
    }
    private void Start()
    {
        globals.timelineActions.Activate();

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
    public void CalculateBounds()
    {
        Renderer[] meshes = GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer mesh in meshes)
        {
            bounds.Encapsulate(mesh.bounds);
        }
        timelineBounds = bounds;
    }
    public void ClearTimeline(bool hardReset = false)
    {
        // flad as timeline as unloaded
        timelineLoaded = false;

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
    private void PositionTimeline(int newPageIndex)
    {
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

        // original
        //float maxDistanceX = timelineBounds.size.x;
        //Vector3 rightOffset = transform.right * maxDistanceX;
        //maxRight = transform.position - rightOffset;
        //maxLeft = transform.position;


        // testing
        float maxDistanceX = Vector3.Distance(transform.position, timelineChildrenWrapper.transform.GetChild(timelineChildrenWrapper.transform.childCount - 1).transform.position);
        Vector3 rightOffset = transform.right * maxDistanceX;
        maxRight = transform.position - rightOffset;
        maxLeft = transform.position;


        // capture first and last timeline child world positions on load for offset ???
        // remove calculatebounds ? 


        // if going backwards, position timeline page at end
        if (newPageIndex < currentResultsPage)
        {
            transform.position = maxRight;
        }
    }
    public void LoadTimelineData(GameObject submittedCell)
    {
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
                int cellXAxisId = activeVizLabels.X.Labels[cellXPosition].Value;
                string cellXAxisType = activeVizLabels.X.Type;

                Query.Filter projFilterX = new Query.Filter(cellXAxisType, new List<int>() { cellXAxisId });
                cellFiltersForTimeline.Add(projFilterX);
            }

            // if Y axis exits, find location of submitted call on it and grab data
            if (activeVizLabels.Y != null)
            {
                int cellYPosition = (int)timelineCellData.Coordinates.y - 1;
                int cellYAxisId = activeVizLabels.Y.Labels[cellYPosition].Value;
                string cellYAxisType = activeVizLabels.Y.Type;

                Query.Filter projFilterY = new Query.Filter(cellYAxisType, new List<int>() { cellYAxisId });
                cellFiltersForTimeline.Add(projFilterY);
            }

            // if Z axis exits, find location of submitted call on it and grab data
            if (activeVizLabels.Z != null)
            {
                int cellZPosition = (int)timelineCellData.Coordinates.z - 1;
                int cellZAxisId = activeVizLabels.Z.Labels[cellZPosition].Value;
                string cellZAxisType = activeVizLabels.Z.Type;

                Query.Filter projFilterZ = new Query.Filter(cellZAxisType, new List<int>() { cellZAxisId });
                cellFiltersForTimeline.Add(projFilterZ);
            }

            // get timeline image data from server and load it
            StartCoroutine(ViRMA_APIController.GetTimeline(cellFiltersForTimeline, (results) => {

                timelineResults = results;

                if (timelineResults.Count >= resultsRenderSize)
                {
                    if (timelineResults.Count % resultsRenderSize == 0)
                    {
                        resultsPagesTotal = (timelineResults.Count / resultsRenderSize);
                    }
                    else
                    {
                        resultsPagesTotal = (timelineResults.Count / resultsRenderSize) + 1;
                    }
                }
                else
                {
                    resultsPagesTotal = 1;
                }

                LoadTimelineSection(0);

            }));
        }
    }
    private void LoadTimelineSection(int pageIndex)
    {
        // hide main viz
        globals.vizController.HideViz(true);

        // clear old timeline if one exists
        ClearTimeline(true);

        // create wrapper
        timelineChildrenWrapper = new GameObject("TimelineChildrenWrapper");
        timelineChildrenWrapper.transform.parent = transform;

        // calculate correct start and end indexes to render correct section results
        int startIndex = pageIndex * resultsRenderSize;
        int resultsToShow = resultsRenderSize;
        if (startIndex + resultsToShow > timelineResults.Count)
        {
            resultsToShow = timelineResults.Count - startIndex;
        }   

        // get section range from results and render it
        List<KeyValuePair<int, string>> currentResultsSection = timelineResults.GetRange(startIndex, resultsToShow);
        for (int i = 0; i < currentResultsSection.Count; i++)
        {
            // create timeline child game object using cell prefab
            GameObject timelineChild = Instantiate(timelineChildPrefab);
            timelineChild.AddComponent<ViRMA_TimelineChild>();

            // add available metadata to game object script (id and filename)
            timelineChild.GetComponent<ViRMA_TimelineChild>().id = currentResultsSection[i].Key;
            timelineChild.GetComponent<ViRMA_TimelineChild>().fileName = currentResultsSection[i].Value;
            timelineChild.name = currentResultsSection[i].Value + "_" + currentResultsSection[i].Key;

            // set wrapper as parent and adjust scale for image aspect ratio
            timelineChild.transform.parent = timelineChildrenWrapper.transform;
            timelineChild.transform.localScale = new Vector3(1.5f, 1, 0.01f);

            // set the distance between children as a % width of the child
            float childWidth = timelineChild.transform.localScale.x;
            float offset = childWidth * childRelativeSpacing * i;
            float xPos = offset + (childWidth * i);
            timelineChild.transform.position = new Vector3(xPos, 0, 0);

            // load timeline child texture
            timelineChild.GetComponent<ViRMA_TimelineChild>().GetTimelineChildTexture();
        }

        ToggleTimelineSectionNavigation(pageIndex);

        timelineChildrenWrapper.transform.localScale = Vector3.one * timelineScale;

        //CalculateBounds();

        PositionTimeline(pageIndex);

        currentResultsPage = pageIndex;

        timelineLoaded = true;
    }
    private void ToggleTimelineSectionNavigation(int newPageIndex)
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

        if (newPageIndex < resultsPagesTotal - 1)
        {
            nextBtn = Instantiate(timelineNavPrefab);
            nextBtn.AddComponent<ViRMA_TimelineChild>().isNextBtn = true;
            nextBtn.GetComponentInChildren<TextMesh>().text = "Next";
            nextBtn.GetComponent<Renderer>().material.SetColor("_Color", ViRMA_Colors.axisTextBlue);
            nextBtn.name = "NextBtn";
            Transform lastChild = timelineChildrenWrapper.transform.GetChild(timelineChildrenWrapper.transform.childCount - 1);
            nextBtn.transform.parent = timelineChildrenWrapper.transform;
            nextBtn.transform.SetAsLastSibling();
            nextBtn.transform.localScale = new Vector3(1, 1, 0.01f);
            Vector3 nextBtnPos = new Vector3(lastChild.transform.localPosition.x + lastChild.transform.localScale.x, lastChild.transform.localPosition.y, lastChild.transform.localPosition.z);
            nextBtn.transform.localPosition = nextBtnPos;
        }

        if (newPageIndex > 0)
        {
            prevBtn = Instantiate(timelineNavPrefab);
            prevBtn.AddComponent<ViRMA_TimelineChild>().isPrevBtn = true;
            prevBtn.GetComponentInChildren<TextMesh>().text = "Previous";
            prevBtn.GetComponent<Renderer>().material.SetColor("_Color", ViRMA_Colors.axisTextBlue);
            prevBtn.name = "PrevBtn";
            Transform firstChild = timelineChildrenWrapper.transform.GetChild(0);
            prevBtn.transform.parent = timelineChildrenWrapper.transform;
            prevBtn.transform.SetAsFirstSibling();
            prevBtn.transform.localScale = new Vector3(1, 1, 0.01f);
            float adjustment = timelineChildrenWrapper.transform.GetChild(1).transform.localScale.x;
            for (int i = 0; i < timelineChildrenWrapper.transform.childCount; i++)
            {
                Transform adjustedChild = timelineChildrenWrapper.transform.GetChild(i);
                adjustedChild.transform.localPosition = new Vector3(adjustedChild.transform.localPosition.x + adjustment, adjustedChild.transform.localPosition.y, adjustedChild.transform.localPosition.z);
            }
            prevBtn.transform.localPosition = Vector3.zero;
        }
    }

    // steamVR actions
    public void SubmitChildForContextMenu(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (hoveredChild != null)
        {
            if (!hoveredChild.GetComponent<ViRMA_TimelineChild>().isNextBtn && !hoveredChild.GetComponent<ViRMA_TimelineChild>().isPrevBtn)
            {
                hoveredChild.GetComponent<ViRMA_TimelineChild>().LoadTImelineContextMenu();
            }
            else if (hoveredChild.GetComponent<ViRMA_TimelineChild>().isNextBtn)
            {
                LoadTimelineSection(currentResultsPage + 1);
            }
            else if (hoveredChild.GetComponent<ViRMA_TimelineChild>().isPrevBtn)
            {
                LoadTimelineSection(currentResultsPage - 1);
            }       
        }
    }
    public void SubmitContextMenuBtn(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (hoveredContextMenuBtn != null)
        {
            Debug.Log(hoveredContextMenuBtn.GetComponent<ViRMA_TimeLineContextMenuBtn>().btnType);
            Debug.Log(hoveredContextMenuBtn.GetComponent<ViRMA_TimeLineContextMenuBtn>().id);
            Debug.Log(hoveredContextMenuBtn.GetComponent<ViRMA_TimeLineContextMenuBtn>().fileName);
        }
    }


}
