using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_QueryController : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    [HideInInspector] public bool queryLoading;

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
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
        */

        StartCoroutine(ViRMA_APIController.SearchHierachies("entity", (nodes) => {

            foreach (Tag node in nodes)
            {
                if (node.Parent == null)
                {
                    //Debug.Log("No parent!");
                }
                else
                {
                    //Debug.Log(node.Parent.Name);
                }      
            }

        }));
    }


    public void testReloadViz(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Debug.Log("Test action fired!");
        if (queryLoading == false)
        {
            queryLoading = true;

            Query dummyQuery = new Query();
            dummyQuery.SetAxis("X", 3, "Tagset");
            dummyQuery.SetAxis("Y", 7, "Tagset");
            //dummyQuery.SetAxis("Z", 77, "Hierarchy");
            //dummyQuery.AddFilter(115, "Hierarchy");
            //dummyQuery.AddFilter(116, "Hierarchy");

            globals.vizController.GetComponent<ViRMA_VizController>().ClearViz();
            StartCoroutine(globals.vizController.SubmitVizQuery(dummyQuery));
        }
    }

}

