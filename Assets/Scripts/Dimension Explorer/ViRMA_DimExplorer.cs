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

    public bool dimexplorerLoaded;
    public Bounds dimExBounds;
    private Vector3 maxRight;
    private Vector3 maxLeft;
    private float distToMaxRight;
    private float distToMaxLeft;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        horizontalRigidbody = GetComponent<Rigidbody>();
        verticalRigidbodies = new List<Rigidbody>(horizontalRigidbody.gameObject.GetComponentsInChildren<Rigidbody>());
        verticalRigidbodies.Remove(horizontalRigidbody);      
    }

    private void Start()
    {
        //CalculateBounds();

        //StartCoroutine(LateStart());
    }

    private void FixedUpdate()
    {
        if (dimexplorerLoaded)
        {
            DimExGroupLimiter();
            DimExplorerMovement();
        }    
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);

        PlaceInFrontOfPlayer();
    }

    // general
    public void LoadDimExplorer(List<Tag> nodes)
    {
        //Debug.Log(nodes.Count + " tags found!");
        foreach (var node in nodes)
        {
            //Debug.Log("Name: " + node.Name + " ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ");
            //Debug.Log("Parent: " + node.Parent.Name);
            //Debug.Log("Sibling Count: " + node.Siblings.Count);
            //Debug.Log("Children Count: " + node.Children.Count);
        }

        // clear any current children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // create dimension explorer button groupings
        float dimExGrpPos = 0;
        foreach (var node in nodes)
        {
            // target tag's parent
            if (node.Parent != null)
            {
                GameObject dimExpGrpParent = new GameObject("DimExpGrpParent");
                dimExpGrpParent.transform.parent = transform;
                dimExpGrpParent.transform.localPosition = new Vector3(dimExGrpPos, 0, 0);
                dimExpGrpParent.AddComponent<ViRMA_DimExplorerGroup>().tagsInGroup = new List<Tag>() { node.Parent };
            }
            
            // target tag and siblings
            if (node.Siblings != null && node.Siblings.Count > 0)
            {
                dimExGrpPos += 0.2f;
                GameObject dimExpSiblings = new GameObject("DimExpGrpSiblings");
                dimExpSiblings.transform.parent = transform;
                dimExpSiblings.transform.localPosition = new Vector3(dimExGrpPos, 0, 0);
                dimExpSiblings.AddComponent<ViRMA_DimExplorerGroup>().tagsInGroup = node.Siblings;
            }

            // target tag's children
            if (node.Children != null && node.Children.Count > 0)
            {
                dimExGrpPos += 0.2f;
                GameObject dimExpChildren = new GameObject("DimExpGrpChildren");
                dimExpChildren.transform.parent = transform;
                dimExpChildren.transform.localPosition = new Vector3(dimExGrpPos, 0, 0);
                dimExpChildren.AddComponent<ViRMA_DimExplorerGroup>().tagsInGroup = node.Children;
            }

            dimExGrpPos += 1;
        }


    }
    public void PositionDimExplorer(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
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
    public void PlaceInFrontOfPlayer()
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

    // fixed update 
    private void DimExGroupLimiter()
    {
        if (Player.instance)
        {

            Vector3 adjustVelocity = horizontalRigidbody.velocity;

            int DimExPosChecker = 0;

            // check if dim explorer is moving horizontally toward it's max right position
            float distToMaxRightTemp = Vector3.Distance(maxRight, transform.position);
            if (distToMaxRightTemp < distToMaxRight)
            {
                distToMaxRight = distToMaxRightTemp;
            }
            else if (distToMaxRightTemp > distToMaxRight)
            {
                distToMaxRight = distToMaxRightTemp;
                DimExPosChecker++;
            }

            // check if dim explorer is moving horizontally toward it's max left position
            float distToMaxLeftTemp = Vector3.Distance(maxLeft, transform.position);
            if (distToMaxLeftTemp < distToMaxLeft)
            {
                distToMaxLeft = distToMaxLeftTemp;
            }
            else if (distToMaxLeftTemp > distToMaxLeft)
            {
                distToMaxLeft = distToMaxLeftTemp;
                DimExPosChecker++;
            }

            // if dim explorer is moving away from both it's max positions, set it's velocity to zero
            if (DimExPosChecker > 1)
            {
                adjustVelocity = Vector3.zero;
            }

            horizontalRigidbody.velocity = adjustVelocity;

        }
    }
    private void DimExplorerMovement()
    {
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

    // editor
    void OnDrawGizmosSelected()
    {
        //CalculateBounds();
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(dimExBounds.size.x, dimExBounds.size.y, dimExBounds.size.z));
    }
}
