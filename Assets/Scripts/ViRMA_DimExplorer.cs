using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorer : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    Rigidbody horizontalRigidbody;

    public List<Rigidbody> verticalRigidbodies;

    public Rigidbody verticalRigidbody;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        StartCoroutine(LateStart());

        horizontalRigidbody = GameObject.Find("Horizontal").GetComponent<Rigidbody>();

        verticalRigidbodies = new List<Rigidbody>();
        foreach (Transform child in horizontalRigidbody.transform)
        {
            if (child.GetComponent<Rigidbody>())
            {
                verticalRigidbodies.Add(child.GetComponent<Rigidbody>());
            }
        }
    }

    private void Update()
    {
        if (globals.testActions_triggerTest.GetState(SteamVR_Input_Sources.Any))
        {
            if (verticalRigidbody == null)
            {
                horizontalRigidbody.velocity = Vector3.zero;
                horizontalRigidbody.angularVelocity = Vector3.zero;

                foreach (var verticalRigidbody in verticalRigidbodies)
                {
                    verticalRigidbody.isKinematic = true;
                }
                horizontalRigidbody.isKinematic = false;

                Vector3 localVelocity = transform.InverseTransformDirection(Player.instance.rightHand.GetTrackedObjectVelocity());
                localVelocity.y = 0;
                localVelocity.z = 0;
                horizontalRigidbody.velocity = transform.TransformDirection(localVelocity) * 2f;
            }
            else
            {
                verticalRigidbody.velocity = Vector3.zero;
                verticalRigidbody.angularVelocity = Vector3.zero;

                horizontalRigidbody.isKinematic = true;
                verticalRigidbody.isKinematic = false;

                Vector3 localVelocity = transform.InverseTransformDirection(Player.instance.rightHand.GetTrackedObjectVelocity());
                localVelocity.x = 0;
                localVelocity.z = 0;
                verticalRigidbody.velocity = transform.TransformDirection(localVelocity) * 2f;
            }
            
        }
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);

        placeInFrontOfPlayer();
    }

    public void positionDimExplorer(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Debug.Log("Test!");

        Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
        flattenedVector.y = 0;
        flattenedVector.Normalize();
        Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.5f;
        transform.position = spawnPos;
        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);

        globals.vizNavActions.Deactivate();

        foreach (Hand hand in Player.instance.hands)
        {
            //ControllerButtonHints.ShowButtonHint(hand, globals.actionClicked);
        }

    }

    public void placeInFrontOfPlayer()
    {
        Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
        flattenedVector.y = 0;
        flattenedVector.Normalize();
        Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.5f;
        transform.position = spawnPos;
        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
    }


}
