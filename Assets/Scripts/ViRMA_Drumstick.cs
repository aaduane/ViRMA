using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_Drumstick : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private Collider col;
    public Hand hand;

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        col = GetComponent<Collider>();
    }

    void Start()
    {
        col.isTrigger = true;

        if (hand.gameObject.transform.Find("HoverPoint"))
        {
            GameObject steamVRHoverPoint = hand.gameObject.transform.Find("HoverPoint").gameObject;
            steamVRHoverPoint.transform.position = transform.position;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("COLLISION ENTER!");
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("COLLISION EXIT!");
    }

    private void OnTriggerEnter(Collider trigger)
    {
        //Debug.Log("TRIGGER ENTER! " + trigger.gameObject.name);

        if (trigger.gameObject.GetComponent<Rigidbody>())
        {
            if (globals.dimExplorer.verticalRigidbodies.Contains(trigger.gameObject.GetComponent<Rigidbody>()))
            {
                globals.dimExplorer.verticalRigidbody = trigger.gameObject.GetComponent<Rigidbody>();
            }
        }


        if (trigger.gameObject.GetComponent<ViRMA_Cell>())
        {
            //trigger.gameObject.GetComponent<ViRMA_Cell>().OnHoverStart(hand);
        }
    }

    private void OnTriggerExit(Collider trigger)
    {
        //Debug.Log("TRIGGER EXIT! " + trigger.gameObject.name);

        if (trigger.gameObject.GetComponent<Rigidbody>())
        {
            if (globals.dimExplorer.verticalRigidbodies.Contains(trigger.gameObject.GetComponent<Rigidbody>()))
            {
                globals.dimExplorer.verticalRigidbody = null;
            }      
        }

        if (trigger.gameObject.GetComponent<ViRMA_Cell>())
        {
            //trigger.gameObject.GetComponent<ViRMA_Cell>().OnHoverEnd(hand);
        }
    }

}
