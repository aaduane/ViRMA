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

    public Rigidbody activeVerticalRigidbody;

    public Bounds dimExBounds;

    private bool boundLimitChecker;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        horizontalRigidbody = GetComponent<Rigidbody>();
        verticalRigidbodies = new List<Rigidbody>(horizontalRigidbody.gameObject.GetComponentsInChildren<Rigidbody>());
        verticalRigidbodies.Remove(horizontalRigidbody);

        StartCoroutine(LateStart());
    }

    private void Start()
    {
        CalculateBounds();
    }

    private void FixedUpdate()
    {
        DimExGroupLimiter();

        if (globals.testActions_triggerTest.GetState(SteamVR_Input_Sources.Any))
        {
            if (activeVerticalRigidbody == null)
            {
                horizontalRigidbody.velocity = Vector3.zero;
                horizontalRigidbody.angularVelocity = Vector3.zero;

                foreach (var verticalRigidbody in verticalRigidbodies)
                {
                    verticalRigidbody.isKinematic = true;
                }
                horizontalRigidbody.isKinematic = false;

                if (!boundLimitChecker)
                {
                    Vector3 rightHandVelocity = transform.InverseTransformDirection(Player.instance.rightHand.GetTrackedObjectVelocity());
                    rightHandVelocity.y = 0;
                    rightHandVelocity.z = 0;
                    horizontalRigidbody.velocity = transform.TransformDirection(rightHandVelocity);
                }        
            }
            else
            {
                activeVerticalRigidbody.velocity = Vector3.zero;
                activeVerticalRigidbody.angularVelocity = Vector3.zero;

                horizontalRigidbody.isKinematic = true;
                activeVerticalRigidbody.isKinematic = false;

                Vector3 rightHandVelocity = transform.InverseTransformDirection(Player.instance.rightHand.GetTrackedObjectVelocity());
                rightHandVelocity.x = 0;
                rightHandVelocity.z = 0;
                activeVerticalRigidbody.velocity = transform.TransformDirection(rightHandVelocity);
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

    private void CalculateBounds()
    {
        Renderer[] meshes = GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer mesh in meshes)
        {
            bounds.Encapsulate(mesh.bounds);
        }

        dimExBounds = bounds;

        Debug.Log("Calculating bounds...");
    }

    private void DimExGroupLimiter()
    {
        if (Player.instance)
        {
            Vector3 adjustVelocity = horizontalRigidbody.velocity;

            float maxDistanceX = dimExBounds.extents.x;

            if (Vector3.Distance(transform.position, Player.instance.hmdTransform.transform.position) > maxDistanceX)
            {

                //adjustVelocity.x = 0;
                //adjustVelocity.z = 0;

                adjustVelocity.x = adjustVelocity.x * -0.2f;
                adjustVelocity.z = adjustVelocity.z * -0.2f;

                horizontalRigidbody.velocity = adjustVelocity;

                boundLimitChecker = true;
            }
            else
            {
                boundLimitChecker = false;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        //CalculateBounds();
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(dimExBounds.size.x, dimExBounds.size.y, dimExBounds.size.z));
    }
}
