using UnityEngine;
using Valve.VR.InteractionSystem;
using System.Collections.Generic;
using System.Collections;
using System;

public class ViRMA_QueryController : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Query buildingQuery;
    [HideInInspector] public bool vizQueryLoading;

    // active query parameters
    public int activeXAxisId;
    public string activeXAxisType;
    public int activeYAxisId;
    public string activeYAxisType;
    public int activeZAxisId;
    public string activeZAxisType;
    public List<Query.Filter> activeFilters = new List<Query.Filter>();

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

        //buildingQuery.SetAxis("X", 1770, "node");
        //buildingQuery.SetAxis("Y", 3733, "node");
        //buildingQuery.SetAxis("Z", 5, "tagset");

        // buildingQuery.SetAxis("X", 13, "tagset");
        // buildingQuery.SetAxis("Y", 691, "node");
        buildingQuery.SetAxis("X", 690, "node");

        buildingQuery.SetAxis("Y", 1770, "node");
        buildingQuery.SetAxis("Z", 3733, "node");

        //buildingQuery.AddFilter(1770, "node");
        //buildingQuery.AddFilter(3733, "node");

        // buildingQuery.AddFilter(147, "tag", 100);
        // buildingQuery.AddFilter(132, "tag", 100);

        //buildingQuery.AddFilter(45, "tag", 99);
        //buildingQuery.RemoveFilter(147, "tag", 100);
        //buildingQuery.RemoveFilter(132, "tag", 100);
        //buildingQuery.RemoveFilter(1770, "node");
        //StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(2);

        buildingQuery.RemoveFilter(1770, "node");

        yield return new WaitForSeconds(2);

        buildingQuery.AddFilter(1770, "node");
        buildingQuery.AddFilter(3733, "node");
        buildingQuery.AddFilter(1771, "node");

        yield return new WaitForSeconds(2);

        buildingQuery.RemoveFilter(147, "tag", 100);
    }

    private void Update()
    {
        QueryReloadController();

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
    }

    private void QueryReloadController()
    {
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

        if (buildingQuery.Filters != null)
        {
            if (buildingQuery.Filters.Count != activeFilters.Count)
            {
                //Debug.Log("Filter counts don't match!");
                counter++;
                activeFilters = ObjectExtensions.Copy(buildingQuery.Filters);
            }
            else
            {
                for (int i = 0; i < buildingQuery.Filters.Count; i++)
                {
                    // check FilterId
                    if (buildingQuery.Filters[i].FilterId != activeFilters[i].FilterId)
                    {
                        //Debug.Log("FilterId in Filters have changed!");
                        counter++;
                        activeFilters = ObjectExtensions.Copy(buildingQuery.Filters);
                        break;
                    }

                    // check Type
                    if (buildingQuery.Filters[i].Type != activeFilters[i].Type)
                    {
                        //Debug.Log("Type in Filters have changed!");
                        counter++;
                        activeFilters = ObjectExtensions.Copy(buildingQuery.Filters);
                        break;
                    }

                    // check IDs
                    string buildingQueryChecker = string.Join(",", buildingQuery.Filters[i].Ids);
                    string activeFiltersChecker = string.Join(",", activeFilters[i].Ids);
                    if (buildingQueryChecker != activeFiltersChecker)
                    {
                        //Debug.Log("IDs in filters have changed!");
                        counter++;
                        activeFilters = ObjectExtensions.Copy(buildingQuery.Filters);
                        break;
                    }
                }
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
            //StartCoroutine(globals.dimExplorer.ClearDimExplorer());

            globals.vizController.GetComponent<ViRMA_VizController>().ClearViz();

            StartCoroutine(globals.vizController.SubmitVizQuery(buildingQuery));

            //Debug.Log("Loading new viz!");
        }
        else
        {
            //Debug.Log("Query aready loading!");
        }
    }

}

