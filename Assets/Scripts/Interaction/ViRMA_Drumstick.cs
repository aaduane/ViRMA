using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_Drumstick : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Hand hand; 

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    void Start()
    {
        if (hand)
        {
            if (hand.gameObject.transform.Find("HoverPoint"))
            {
                GameObject steamVRHoverPoint = hand.gameObject.transform.Find("HoverPoint").gameObject;
                steamVRHoverPoint.transform.position = transform.position;
            }
        }
        //GetComponent<Renderer>().material.renderQueue = 3001;
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
