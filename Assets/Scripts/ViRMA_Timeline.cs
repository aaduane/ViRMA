using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_Timeline : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    public void LoadTimeline()
    {
        Debug.Log("Loading timeline..."); 
    }

}
