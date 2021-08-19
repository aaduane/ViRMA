using System.Collections;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_QueryController : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Query activeQuery;
    [HideInInspector] public bool queryLoading;

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    private void Start()
    {
        activeQuery = new Query();

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
        */


        //StartCoroutine(ViRMA_APIController.SearchHierachies("computer", (nodes) => {
        //    StartCoroutine(globals.dimExplorer.LoadDimExplorer(nodes));
        //}));    


        StartCoroutine(TestPosition());

        globals.menuInteractionActions.Activate();

        Debug.Log(globals.menuInteractionActions.IsActive());
    }

    private IEnumerator TestPosition()
    {
        yield return new WaitForSeconds(1);

        Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
        flattenedVector.y = 0;
        flattenedVector.Normalize();
        Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.6f;
        transform.position = spawnPos;
        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
    }

    private void Update()
    {

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
    }

    public void ReloadViz()
    {
        Debug.Log("Reloading viz...");

        if (queryLoading == false)
        {
            queryLoading = true;

            globals.vizController.GetComponent<ViRMA_VizController>().ClearViz();

            StartCoroutine(globals.vizController.SubmitVizQuery(activeQuery));
        }
    }

}

