using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_Timeline : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Cell timelineCellData;
    public AxesLabels activeVizLabels;
    public List<KeyValuePair<int, string>> timelineImageIdPaths;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
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

            // set wrapper as parent and adjust scale for image aspect ratio
            timelineChild.transform.parent = timelineChildrenWrapper.transform;
            timelineChild.transform.localScale = new Vector3(1.5f, 1, 0.01f);

            // set the distance between children as half the width of the child
            float targetSpacing = 0.5f;
            float widthOfChild = timelineChild.transform.localScale.x;
            float offset = (widthOfChild * targetSpacing) * childIndex;
            float xDistance = (childIndex * widthOfChild) + offset;
            timelineChild.transform.position = new Vector3(xDistance, 0, 0);

            // load timeline child texture
            timelineChild.GetComponent<ViRMA_TimelineChild>().GetTimelineChildTexture();

            childIndex++;
        }

        // set overall scale via wrapper
        timelineChildrenWrapper.transform.localScale = Vector3.one * 0.1f;
    }

}
