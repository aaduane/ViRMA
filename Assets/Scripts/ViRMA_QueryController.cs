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

    // enable and/or setting for concept tags
    public bool queryModeOrSetting;

    // --- active query parameters --- \\

    // x
    public int activeXAxisId;
    public string activeXAxisType;

    // y
    public int activeYAxisId;
    public string activeYAxisType;

    // z
    public int activeZAxisId;
    public string activeZAxisType;

    // direct filters
    public List<Query.Filter> activeFilters = new List<Query.Filter>();

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        buildingQuery = new Query();

        activeXAxisId = -1;
        activeXAxisType = null;

        activeYAxisId = -1;
        activeYAxisType = null;

        activeZAxisId = -1;
        activeZAxisType = null;
    }

    private void Start()
    { 
        queryModeOrSetting = false; // EXPERIMENTAL ---> setting to True enables "or" instead of "and" in hierarchy concept tag filtering

        buildingQuery.SetAxis("X", 3056, "node"); // computer
        buildingQuery.SetAxis("Y", 5505, "node"); // desk

        //buildingQuery.SetAxis("X", 40, "node"); // entity
        //buildingQuery.SetAxis("X", 1606, "node"); // food fish
        //buildingQuery.SetAxis("Y", 6967, "node"); // kitchen appliance

        buildingQuery.AddFilter(56, "tag", 15); // work location
        //buildingQuery.AddFilter(130, "tag", 15); // home location

        buildingQuery.AddFilter(22, "tag", 6); // Monmday
        buildingQuery.AddFilter(2001, "tag", 6); // Tuesday
        //buildingQuery.AddFilter(6649, "tag", 0); // June (only 4 images)

        StartCoroutine(LateStart()); // debugging
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(5);

        buildingQuery.AddFilter(1869, "tag", 6); // add Wednesday

        yield return new WaitForSeconds(5);

        buildingQuery.RemoveFilter(2001, "tag", 6); // remove Tuesday


        //yield return new WaitForSeconds(2);

    }

    private void Update()
    {
        QueryReloadController();

        //ConsoleLogCurrentQuery(); // debugging
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
            if (activeXAxisId == -1)
            {
                buildingQuery.X = null;
            }
            if (activeYAxisId == -1)
            {
                buildingQuery.Y = null;
            }
            if (activeZAxisId == -1)
            {
                buildingQuery.Z = null;
            }

            ReloadViz();
        }
    }
    public void ReloadViz()
    {
        if (vizQueryLoading == false)
        {
            //StartCoroutine(globals.dimExplorer.ClearDimExplorer());

            globals.mainMenu.ToggleLoadingIndicator();

            globals.vizController.GetComponent<ViRMA_VizController>().ClearViz();

            StartCoroutine(globals.vizController.SubmitVizQuery(buildingQuery));

            //Debug.Log("Loading new viz!");
        }
        else
        {
            //Debug.Log("Query aready loading!");
        }
    }
    private void ConsoleLogCurrentQuery()
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

        Debug.Log("----------PROJECTED FILTERS-----------");
        Debug.Log("X: " + activeXAxisId + " / " + activeXAxisType);
        Debug.Log("Y: " + activeYAxisId + " / " + activeYAxisType);
        Debug.Log("Z: " + activeZAxisId + " / " + activeZAxisType);
        Debug.Log("------------DIRECT FILTERS------------");
        foreach (var activeFilter in activeFilters)
        {
            Debug.Log(activeFilter.Ids[0] + " / " + activeFilter.Type);
        }
    }

}

