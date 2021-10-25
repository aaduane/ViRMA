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
    GameObject timelineChildPrefab;
    GameObject timelineChildrenWrapper;
    public GameObject hoveredChild;
    public GameObject hoveredContextMenuBtn;

    public Cell timelineCellData;
    public AxesLabels activeVizLabels;
    public List<KeyValuePair<int, string>> timelineImageIdPaths;

    public float timelineScale;
    public float childRelativeSpacing;
    public float timelinePositionDistance;

    int resultsRenderSize;
    int currentTargetChildIndex;

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
    }

    private void Start()
    {
        globals.timelineActions.Activate();

        timelineChildPrefab = Resources.Load("Prefabs/CellPrefab") as GameObject;
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

            if (timelinePosChecker > 1)
            {
                timelineRb.velocity = Vector3.zero;
            }
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
                timelineImageIdPaths = results;
                StartCoroutine(LoadTimeline(0, "next"));
            }));
        }
    }
    private IEnumerator LoadTimeline(int startIndex, string direction)
    {
        globals.vizController.HideViz(true);

        yield return StartCoroutine(ClearTimeline());

        timelineChildrenWrapper = new GameObject("TimelineChildrenWrapper");
        timelineChildrenWrapper.transform.parent = transform;

        if (direction == "next")
        {
            int positionIndex = 0;
            for (int i = startIndex; i < timelineImageIdPaths.Count; i++)
            {              
                if (i == startIndex + resultsRenderSize)
                {
                    break;
                }
                PositionChildInTimeline(i, positionIndex);
                positionIndex++;
            }

            currentTargetChildIndex = startIndex;

        }
        else if (direction == "prev")
        {
            int positionIndex = 0;
            for (int i = startIndex; i >= 0; i--)
            {
                if (i == startIndex - resultsRenderSize)
                {
                    break;
                }
                PositionChildInTimeline(i, positionIndex);
                positionIndex++;
            }

            currentTargetChildIndex = startIndex - (resultsRenderSize - 1);
        }

        TogglePrevNextBtns(startIndex);

        timelineChildrenWrapper.transform.localScale = Vector3.one * timelineScale;

        CalculateBounds();

        PositionTimeline(direction);

        timelineLoaded = true;
    }
    private void PositionChildInTimeline(int childIndex, int positionIndex)
    {
        // create timeline child game object using cell prefab
        GameObject timelineChild = Instantiate(timelineChildPrefab);
        timelineChild.AddComponent<ViRMA_TimelineChild>();

        // add available metadata to game object script (id and filename)
        timelineChild.GetComponent<ViRMA_TimelineChild>().id = timelineImageIdPaths[childIndex].Key;
        timelineChild.GetComponent<ViRMA_TimelineChild>().fileName = timelineImageIdPaths[childIndex].Value;
        timelineChild.name = timelineImageIdPaths[childIndex].Value + "_" + timelineImageIdPaths[childIndex].Key + " (" + (childIndex + 1) + "/" + timelineImageIdPaths.Count + ")";

        // set wrapper as parent and adjust scale for image aspect ratio
        timelineChild.transform.parent = timelineChildrenWrapper.transform;
        timelineChild.transform.localScale = new Vector3(1.5f, 1, 0.01f);

        // set the distance between children as a % width of the child
        float childWidth = timelineChild.transform.localScale.x;
        float offset = childWidth * childRelativeSpacing * positionIndex;
        float xPos = offset + (childWidth * positionIndex);
        timelineChild.transform.position = new Vector3(xPos, 0, 0);

        // load timeline child texture
        timelineChild.GetComponent<ViRMA_TimelineChild>().GetTimelineChildTexture();
    }
    private void PositionTimeline(string direction)
    {
        float distance = timelinePositionDistance;
        Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
        flattenedVector.y = 0;
        flattenedVector.Normalize();
        Vector3 spawnPos = Player.instance.hmdTransform.position + (flattenedVector * distance);
        transform.position = spawnPos;
        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position); // flip viz 180 degrees from 'LookAt'

        float maxDistanceX = timelineBounds.extents.x;
        Vector3 movement = transform.right * maxDistanceX;
        maxRight = transform.position - (movement * 2);
        maxLeft = transform.position;

        if (direction == "prev")
        {
            transform.position = maxRight;
        }      
    }
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
    private void TogglePrevNextBtns(int startIndex)
    {
        // Debug.Log(startIndex + " + " + resultsRenderSize + " < " + timelineImageIdPaths.Count);
        if (startIndex + resultsRenderSize < timelineImageIdPaths.Count)
        {
            GameObject nextBtnChild = Instantiate(timelineChildPrefab);
            nextBtnChild.AddComponent<ViRMA_TimelineChild>();
            nextBtnChild.name = "NextBtn";
            Transform lastChild = timelineChildrenWrapper.transform.GetChild(timelineChildrenWrapper.transform.childCount - 1);
            Vector3 nextBtnPos = new Vector3(lastChild.transform.localPosition.x + lastChild.transform.localScale.x, lastChild.transform.localPosition.y, lastChild.transform.localPosition.z);
            nextBtnChild.transform.parent = timelineChildrenWrapper.transform;
            nextBtnChild.transform.localScale = new Vector3(1, 1, 0.01f);
            nextBtnChild.transform.localPosition = nextBtnPos;

            nextBtnChild.GetComponent<ViRMA_TimelineChild>().isNextBtn = true;
        }

        // Debug.Log(startIndex + " - " + resultsRenderSize + " > 0");
        if (startIndex - resultsRenderSize >= 0)
        {
            GameObject prevBtnChild = Instantiate(timelineChildPrefab);
            prevBtnChild.AddComponent<ViRMA_TimelineChild>();
            prevBtnChild.name = "PrevBtn";
            Transform firstChild = timelineChildrenWrapper.transform.GetChild(0);
            Vector3 prevBtnPos = new Vector3(firstChild.transform.localPosition.x - firstChild.transform.localScale.x, firstChild.transform.localPosition.y, firstChild.transform.localPosition.z);
            prevBtnChild.transform.parent = timelineChildrenWrapper.transform;
            prevBtnChild.transform.localScale = new Vector3(1, 1, 0.01f);
            prevBtnChild.transform.localPosition = prevBtnPos;

            prevBtnChild.GetComponent<ViRMA_TimelineChild>().isPrevBtn = true;
        }    
    }
    public IEnumerator ClearTimeline()
    {
        if (timelineChildrenWrapper)
        {
            timelineLoaded = false;
            Destroy(timelineChildrenWrapper);
            yield return new WaitForEndOfFrame();
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;           
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
                int newTargetChildIndex = currentTargetChildIndex + resultsRenderSize;

                StartCoroutine(LoadTimeline(newTargetChildIndex, "next"));

                Debug.Log("Loading NEXT button | " + newTargetChildIndex);
            }
            else if (hoveredChild.GetComponent<ViRMA_TimelineChild>().isPrevBtn)
            {

                /*
                int newTargetChildIndex;
                if (timelineChildrenWrapper.transform.childCount - 1 < resultsRenderSize)
                {
                    newTargetChildIndex =  currentTargetChildIndex - (timelineChildrenWrapper.transform.childCount - 1);
                }
                else
                {
                    newTargetChildIndex = currentTargetChildIndex - resultsRenderSize;
                }
                */

                int newTargetChildIndex = currentTargetChildIndex - 1;

                StartCoroutine(LoadTimeline(newTargetChildIndex, "prev"));

                Debug.Log("Loading PREV button | " + newTargetChildIndex);
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
