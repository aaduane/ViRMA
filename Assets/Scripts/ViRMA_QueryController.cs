using UnityEngine;

public class ViRMA_QueryController : MonoBehaviour
{
    private void Start()
    {

        StartCoroutine(ViRMA_APIController.GetTagsets((tagsets) => {
            foreach (var tagset in tagsets)
            {
                Debug.Log("Tagset: " + tagset.Id + " | " + tagset.Name);
            }
        }));

        StartCoroutine(ViRMA_APIController.GetHierarchies((hierarchies) => {
            foreach (var hierarchy in hierarchies)
            {
                Debug.Log("Hierarchy: " + hierarchy.Id + " | " + hierarchy.Name);
            }
        }));

    }
}
