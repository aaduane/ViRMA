using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_Drumstick : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    public Hand hand; // assigned at creation

    private Collider col;

    //public GameObject drumstickSphere;  
    //private SphereCollider drumstickSphereCollider; 

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        // assign collider to global
        col = GetComponent<Collider>();
    }

    void Start()
    {
        /*
        transform.localPosition = new Vector3(0, 0, 0.05f);

        drumstickSphereCollider = gameObject.AddComponent<SphereCollider>();
        drumstickSphereCollider.radius = 0.05f;
        drumstickSphereCollider.center = Vector3.zero;
        drumstickSphereCollider.isTrigger = true;
        hand.gameObject.GetComponent<ViRMA_Hand>().drumstickCollider = drumstickSphereCollider;


        GameObject drumstickSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        drumstickSphere.transform.parent = transform;
        drumstickSphere.transform.localScale = Vector3.one * drumstickSphereCollider.radius;
        drumstickSphere.transform.localPosition = Vector3.zero;
        drumstickSphere.transform.localRotation = Quaternion.identity;
        drumstickSphere.GetComponent<Renderer>().material.color = Color.red;
        */

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
