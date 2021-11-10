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

    // diect filters
    public List<Query.Filter> activeFilters = new List<Query.Filter>();

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        buildingQuery = new Query();
    }

    private void Start()
    {
        buildingQuery.SetAxis("X", 1770, "node"); // computer
        buildingQuery.SetAxis("Y", 3733, "node"); // desk
        //buildingQuery.SetAxis("Z", 690, "node"); // domestic animal

        //buildingQuery.SetAxis("X", 690, "node"); // domestic animal
        //buildingQuery.SetAxis("Y", 691, "node"); // dog

        //buildingQuery.SetAxis("Z", 5, "tagset"); // day of the week (string)
        //buildingQuery.SetAxis("Z", 13, "tagset"); // timezone

        //buildingQuery.AddFilter(147, "tag", 100); // 6 (Saturday)
        //buildingQuery.AddFilter(132, "tag", 100); // 7 (Sunday

        //buildingQuery.AddFilter(690, "node"); // domestic animal
        buildingQuery.AddFilter(49, "node"); // domestic animal

        //StartCoroutine(LateStart()); // testing
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
        else
        {
            activeXAxisId = -1;
            activeXAxisType = null;
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
        else
        {
            activeYAxisId = -1;
            activeYAxisType = null;
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
        else
        {
            activeZAxisId = -1;
            activeZAxisType = null;
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

            // ConsoleLogCurrentQuery(); // testing

            //Debug.Log("Loading new viz!");
        }
        else
        {
            //Debug.Log("Query aready loading!");
        }
    }

    private void ConsoleLogCurrentQuery()
    {
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
    }

}

