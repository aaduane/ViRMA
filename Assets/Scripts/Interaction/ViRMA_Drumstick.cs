using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_Drumstick : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Hand hand; // assigned at creation
    private Collider col; 

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        // assign collider to global
        col = GetComponent<Collider>();
    }

    void Start()
    {
        if (hand.gameObject.transform.Find("HoverPoint"))
        {
            GameObject steamVRHoverPoint = hand.gameObject.transform.Find("HoverPoint").gameObject;
            steamVRHoverPoint.transform.position = transform.position;
        }

        col.isTrigger = true;

        GetComponent<Renderer>().material.color = Color.red;        
    }

    private void OnTriggerEnter(Collider triggeredCol)
    {
        //Debug.Log("TRIGGER ENTER! " + triggeredCol.name);
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        //Debug.Log("TRIGGER EXIT! " + triggeredCol.name);
    }
    private void OnTriggerStay(Collider triggeredCol)
    {
        //Debug.Log("TRIGGER STAYING! " + triggeredCol.name);
    }

}
