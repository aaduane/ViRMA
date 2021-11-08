using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_DirectFilterOption : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private BoxCollider col;
    private Rigidbody rigidBody;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        col = gameObject.AddComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            //Debug.Log("ENTER | " + triggeredCol.name); // testing



        }
    }
    private void OnTriggerStay(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            //Debug.Log("STAY | " + triggeredCol.name); // testing



        }
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            //Debug.Log("EXIT | " + triggeredCol.name); // testing



        }
    }

}
