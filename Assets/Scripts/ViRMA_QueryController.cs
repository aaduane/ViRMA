using UnityEngine;
using Valve.VR.InteractionSystem;
using System.Collections.Generic;

public class ViRMA_QueryController : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Query buildingQuery;
    [HideInInspector] public bool vizQueryLoading;

    // active query parameters
    int activeXAxisId;
    string activeXAxisType;
    int activeYAxisId;
    string activeYAxisType;
    int activeZAxisId;
    string activeZAxisType;
    List<string> activeFilters;

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        buildingQuery = new Query();
    }

    private void Start()
    {
        /*
        
        StartCoroutine(ViRMA_APIController.GetTagsets((tagsets) => {
            foreach (var tagset in tagsets)
            {
                //Debug.Log("Tagset: " + tagset.Id + " | " + tagset.Name); // debugging
            }
        }));

        StartCoroutine(ViRMA_APIController.GetHierarchies((hierarchies) => {
            foreach (var hierarchy in hierarchies)
            {
                //Debug.Log("Hierarchy: " + hierarchy.Id + " | " + hierarchy.Name); // debugging
            }
        }));       

        StartCoroutine(ViRMA_APIController.SearchHierachies("computer", (nodes) => {
            StartCoroutine(globals.dimExplorer.LoadDimExplorer(nodes));
        }));

        */

        buildingQuery.SetAxis("X", 1770, "Hierarchy");
        buildingQuery.SetAxis("Y", 3733, "Hierarchy");
        buildingQuery.SetAxis("Z", 5, "Tagset");
    }

    private void Update()
    {
        /*
        if (buildingQuery.X != null)
        {
            Debug.Log("X: " + buildingQuery.X.Id);
        }
        if (buildingQuery.Y != null)
        {
            Debug.Log("Y: " + buildingQuery.Y.Id);
        }
        if (buildingQuery.Z != null)
        {
            Debug.Log("Z: " + buildingQuery.Z.Id);
        }
        if (buildingQuery.Filters.Count > 0)
        {
            Debug.Log(buildingQuery.Filters.Count + " direct filters!");
        }
        */

        int counter = 0;

        if (buildingQuery.X != null)
        {
            if (buildingQuery.X.Id != activeXAxisId || buildingQuery.X.Type != activeXAxisType)
            {
                counter++;
                activeXAxisId = buildingQuery.X.Id;
                activeXAxisType = buildingQuery.X.Type;        
            }
        }

        if (buildingQuery.Y != null)
        {
            if (buildingQuery.Y.Id != activeYAxisId || buildingQuery.Y.Type != activeYAxisType)
            {
                counter++;
                activeYAxisId = buildingQuery.Y.Id;
                activeYAxisType = buildingQuery.Y.Type;
            }
        }

        if (buildingQuery.Z != null)
        {
            if (buildingQuery.Z.Id != activeZAxisId || buildingQuery.Z.Type != activeZAxisType)
            {
                counter++;
                activeZAxisId = buildingQuery.Z.Id;
                activeZAxisType = buildingQuery.Z.Type;
            }
        }

        if (counter > 0)
        {
            ReloadViz();
        }
    }

    public void ReloadViz()
    {
        if (vizQueryLoading == false)
        {
            Debug.Log("Loading new query... ");

            vizQueryLoading = true;

            //StartCoroutine(globals.dimExplorer.ClearDimExplorer());

            globals.vizController.GetComponent<ViRMA_VizController>().ClearViz();

            StartCoroutine(globals.vizController.SubmitVizQuery(buildingQuery));
        }
        else
        {
            Debug.Log("Query aready loading!");
        }
    }

}

