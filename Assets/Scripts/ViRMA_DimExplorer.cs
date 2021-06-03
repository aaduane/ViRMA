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

    private Vector3 maxRight;
    private Vector3 maxLeft;
    private float dist1;
    private float dist2;

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

                Vector3 rightHandVelocity = transform.InverseTransformDirection(Player.instance.rightHand.GetTrackedObjectVelocity());
                rightHandVelocity.y = 0;
                rightHandVelocity.z = 0;
                horizontalRigidbody.velocity = transform.TransformDirection(rightHandVelocity);
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
        Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.6f;
        transform.position = spawnPos;
        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);


        float maxDistanceX = dimExBounds.extents.x * 1.1f;
        Vector3 movement = transform.right * maxDistanceX;
        maxRight = transform.position + movement;
        maxLeft = transform.position - movement;
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
    }

    private void DimExGroupLimiter()
    {
        if (Player.instance)
        {
            //float distanceFromPlayer = Vector3.Distance(transform.position, Player.instance.hmdTransform.transform.position);


            Vector3 adjustVelocity = horizontalRigidbody.velocity;

            int DimExPosChecker = 0;

            float distTemp1 = Vector3.Distance(maxRight, transform.position);
            if (distTemp1 < dist1)
            {
                dist1 = distTemp1;
            }
            else if (distTemp1 > dist1)
            {
                dist1 = distTemp1;
                DimExPosChecker++;
            }

            float distTemp2 = Vector3.Distance(maxLeft, transform.position);
            if (distTemp2 < dist2)
            {
                dist2 = distTemp2;
            }
            else if (distTemp2 > dist2)
            {
                dist2 = distTemp2;
                DimExPosChecker++;
            }

            if (DimExPosChecker > 1)
            {
                adjustVelocity = Vector3.zero;
            }

            horizontalRigidbody.velocity = adjustVelocity;

        }
    }

    void OnDrawGizmosSelected()
    {
        //CalculateBounds();
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(dimExBounds.size.x, dimExBounds.size.y, dimExBounds.size.z));
    }
}
