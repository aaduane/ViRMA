using System.Collections;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_QueryController : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Query activeQuery;
    public Query previousQuery;
    [HideInInspector] public bool queryLoading;

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    private void Start()
    {
        activeQuery = new Query();
        previousQuery = new Query();

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

    }

    private void Update()
    {
        /*
        if (activeQuery.X != null)
        {
            Debug.Log("X: " + activeQuery.X.Id);
        }
        if (activeQuery.Y != null)
        {
            Debug.Log("Y: " + activeQuery.Y.Id);
        }
        if (activeQuery.Z != null)
        {
            Debug.Log("Z: " + activeQuery.Z.Id);
        }
        if (activeQuery.Filters.Count > 0)
        {
            Debug.Log(activeQuery.Filters.Count + " direct filters!");
        }
        */

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// NOT WORKING
        if (activeQuery.X != previousQuery.X || activeQuery.Y != previousQuery.Y || activeQuery.Z != previousQuery.Z)
        {
            ReloadViz();
            previousQuery = activeQuery; 
        }
    }

    public void ReloadViz()
    {
        Debug.Log("LOADING VIZ!");

        if (queryLoading == false)
        {
            queryLoading = true;

            globals.vizController.GetComponent<ViRMA_VizController>().ClearViz();

            StartCoroutine(globals.vizController.SubmitVizQuery(activeQuery));
        }
    }

}

