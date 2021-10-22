using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_Timeline : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private Rigidbody timelineRb;
    public GameObject hoveredChild;
    public GameObject hoveredContextMenuBtn;

    public Cell timelineCellData;
    public AxesLabels activeVizLabels;
    public List<KeyValuePair<int, string>> timelineImageIdPaths;

    public float timelineScale;
    public float childRelativeSpacing;
    public float timelinePositionDistance;

    public bool timelineLoaded;

    public Bounds timelineBounds;
    private Vector3 maxRight;
    private Vector3 maxLeft;
    private float distToMaxRight;
    private float distToMaxLeft;

    

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        timelineRb = GetComponent<Rigidbody>();
        timelineLoaded = false;

        timelineScale = 0.3f; // global scale of timeline
        childRelativeSpacing = 0.25f; // % width of the child to space by
        timelinePositionDistance = 0.6f; // how far away to place the timeline
    }

    private void Start()
    {
        globals.timelineActions.Activate();
    }

    private void Update()
    {
        if (timelineLoaded)
        {
            TimelineMovement();

            TimelineMovementLimiter();
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
                LoadTimeline(timelineImageIdPaths);
            }));
        }
    }
    private void LoadTimeline(List<KeyValuePair<int, string>> results)
    {
        globals.vizController.HideViz(true);

        GameObject timelineChildPrefab = Resources.Load("Prefabs/CellPrefab") as GameObject;
        GameObject timelineChildrenWrapper = new GameObject("TimelineChildrenWrapper");
        timelineChildrenWrapper.transform.parent = transform;

        int childIndex = 0;
        foreach (KeyValuePair<int, string> result in results)
        {
            // create timeline child game object using cell prefab
            GameObject timelineChild = Instantiate(timelineChildPrefab);
            timelineChild.AddComponent<ViRMA_TimelineChild>();

            // add available metadata to game object script (id and filename)
            timelineChild.GetComponent<ViRMA_TimelineChild>().id = result.Key;
            timelineChild.GetComponent<ViRMA_TimelineChild>().fileName = result.Value;
            timelineChild.name = result.Value + "_" + result.Key;

            // set wrapper as parent and adjust scale for image aspect ratio
            timelineChild.transform.parent = timelineChildrenWrapper.transform;
            timelineChild.transform.localScale = new Vector3(1.5f, 1, 0.01f);

            // set the distance between children as a % width of the child
            float childWidth = timelineChild.transform.localScale.x;
            float offset = childWidth * childIndex * childRelativeSpacing;
            float xPos = offset + (childWidth * childIndex);
            timelineChild.transform.position = new Vector3(xPos, 0, 0);

            // load timeline child texture
            timelineChild.GetComponent<ViRMA_TimelineChild>().GetTimelineChildTexture();

            childIndex++;
        }

        // set overall scale via wrapper
        timelineChildrenWrapper.transform.localScale = Vector3.one * timelineScale;

        CalculateBounds();

        PositionTimeline();

        timelineLoaded = true;
    }
    private void PositionTimeline()
    {
        float distance = timelinePositionDistance;
        Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
        flattenedVector.y = 0;
        flattenedVector.Normalize();
        Vector3 spawnPos = Player.instance.hmdTransform.position + (flattenedVector * distance);
        transform.position = spawnPos;
        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position); // flip viz 180 degrees from 'LookAt'

        // calculate max left and right positions of timeline
        float maxDistanceX = timelineBounds.extents.x * 0.85f;
        Vector3 movement = transform.right * maxDistanceX;
        maxRight = transform.position - (movement * 2);
        maxLeft = transform.position;
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
    public void SubmitChildForContextMenu(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (hoveredChild != null)
        {
            hoveredChild.GetComponent<ViRMA_TimelineChild>().LoadTImelineContextMenu();
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
