using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_NavController : MonoBehaviour
{
    public SteamVR_ActionSet NodeNavigationControls;
    public SteamVR_Action_Boolean NodeNavigationToggle;

    private Rigidbody rigidBody;

    private float previousDistanceBetweenHands; 

    private Vector3 maxNodeScale = new Vector3(1.0f, 1.0f, 1.0f);
    private Vector3 minNodeScale = new Vector3(0.1f, 0.1f, 0.1f);
    private float defaultNodeSize = 0.1f;
    private float defaultNodeSpacing = 1.5f;

    private Bounds maximumScrollBounds;
    private GameObject boundingBox;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        string testURL1 = "cell?xAxis={'AxisType': 'Hierarchy', 'HierarchyNodeId' :7}";
        string testURL2 = "&yAxis={'AxisType': 'Tagset', 'TagsetId': 7}";
        string testURL3 = "&zAxis={'AxisType': 'Tagset', 'TagsetId': 7}";

        StartCoroutine(ViRMA_APIController.CallAPI(testURL1 + testURL2 + testURL3, (response) => {

            Debug.Log(response.Count + " results");
            foreach (var obj in response)
            {
                Vector3 blueprint = new Vector3(obj.Value["x"], obj.Value["y"], obj.Value["z"]);
                GenerateNode(blueprint);
            }

            

            //CenterGridOnNodes();

            CalculateBoundingBox();



            boundingBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(boundingBox.GetComponent<Collider>());
            boundingBox.GetComponent<Renderer>().material = Resources.Load("Materials/BasicTransparent") as Material;
            boundingBox.GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 130);
            boundingBox.name = "BoundingBox";
            boundingBox.transform.localScale = new Vector3(maximumScrollBounds.max.x, maximumScrollBounds.max.y, maximumScrollBounds.max.z);
            boundingBox.transform.localPosition = maximumScrollBounds.center;
            boundingBox.transform.SetParent(transform);        



            //PlaceInFrontOfPlayer(5f);
            NodeNavigationControls.Activate();

            //StartCoroutine(LateStart());
        }));
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);
    }

    private void FixedUpdate()
    {
        NodeNavigationController();
        NodeNavigationLimiter();
    }

    public void GenerateNode(Vector3 blueprint)
    {
        Object[] imagesAsTextures = Resources.LoadAll("Test Images", typeof(Material)); // tesing

        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cube);
        node.transform.localScale = new Vector3(defaultNodeSize, defaultNodeSize, defaultNodeSize);
        float spacingMultipler = defaultNodeSize + (defaultNodeSize * defaultNodeSpacing);
        Vector3 nodePosition = new Vector3(blueprint.x, blueprint.y, blueprint.z) * spacingMultipler;
        node.transform.position = nodePosition;
        node.transform.parent = transform;

        // apply textures 
        int imageId = Random.Range(0, imagesAsTextures.Length);
        node.GetComponent<Renderer>().material = imagesAsTextures[imageId] as Material;
    }

    
    // node navigation (position, rotation, scale)
    private void NodeNavigationController()
    {
        if (NodeNavigationToggle[SteamVR_Input_Sources.LeftHand].state && NodeNavigationToggle[SteamVR_Input_Sources.RightHand].state)
        {
            // both triggers held down
            ToggleNodeScaling();
        }
        else if (NodeNavigationToggle[SteamVR_Input_Sources.LeftHand].state || NodeNavigationToggle[SteamVR_Input_Sources.RightHand].state)
        {
            // one trigger held down
            if (NodeNavigationToggle[SteamVR_Input_Sources.RightHand].state)
            {
                // right trigger held down
                ToggleNodePositioning();
            }

            if (NodeNavigationToggle[SteamVR_Input_Sources.LeftHand].state)
            {
                // left trigger held down
                ToggleNodeRotation();
            }
        }
        else
        {
            if (previousDistanceBetweenHands != 0)
            {
                previousDistanceBetweenHands = 0;
            }
        }    
    }
    private void NodeNavigationLimiter()
    {
        if (Player.instance)
        {
            Vector3 currentVelocity = rigidBody.velocity;

            // x and z
            float boundary;
            if (maximumScrollBounds.size.x > maximumScrollBounds.size.z)
            {
                boundary = maximumScrollBounds.size.x;
            }
            else
            {
                boundary = maximumScrollBounds.size.z;
            }
            if (Vector3.Distance(transform.position, Player.instance.hmdTransform.transform.position) > boundary) 
            {
                Vector3 normalisedDirection = (transform.position - Player.instance.hmdTransform.transform.position).normalized;
                Vector3 v = rigidBody.velocity;
                float d = Vector3.Dot(v, normalisedDirection);
                if (d > 0f) v -= normalisedDirection * d;
                rigidBody.velocity = v;
            }

            // y max
            float maxDistanceY = Player.instance.eyeHeight + maximumScrollBounds.extents.y;       
            if (transform.position.y >= maxDistanceY && currentVelocity.y > 0)
            {
                currentVelocity.y = 0;
                rigidBody.velocity = currentVelocity;
            }

            // y min
            float minDistanceY = Player.instance.eyeHeight - maximumScrollBounds.extents.y;
            if (transform.position.y <= minDistanceY && currentVelocity.y < 0)
            {
                currentVelocity.y = 0;
                rigidBody.velocity = currentVelocity;
            }
            
        }
    }
    private void ToggleNodePositioning()
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        if (Player.instance.rightHand.GetTrackedObjectVelocity().magnitude > 0.5f)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(Player.instance.rightHand.GetTrackedObjectVelocity());
            //localVelocity.x = 0;
            //localVelocity.y = 0;
            //localVelocity.z = 0;
            rigidBody.velocity = transform.TransformDirection(localVelocity) * 2f;
        }
    }
    private void ToggleNodeRotation()
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        Vector3 localAngularVelocity = transform.InverseTransformDirection(Player.instance.leftHand.GetTrackedObjectAngularVelocity());
        localAngularVelocity.x = 0;
        //localAngularVelocity.y = 0;
        localAngularVelocity.z = 0;
        rigidBody.angularVelocity = transform.TransformDirection(localAngularVelocity) * 0.1f;
    }
    private void ToggleNodeScaling()
    {
        Vector3 leftHandPosition = Player.instance.leftHand.transform.position;
        Vector3 rightHandPosition = Player.instance.rightHand.transform.position;
        float thisFramedistance = Mathf.Round(Vector3.Distance(leftHandPosition, rightHandPosition) * 100.0f) * 0.01f;

        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        if (previousDistanceBetweenHands == 0)
        {
            previousDistanceBetweenHands = thisFramedistance;
        }
        else
        {
            if (thisFramedistance > previousDistanceBetweenHands)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, maxNodeScale, 2f * Time.deltaTime);
            }
            if (thisFramedistance < previousDistanceBetweenHands)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, minNodeScale, 2f * Time.deltaTime);
            }
            previousDistanceBetweenHands = thisFramedistance;
        }

        // calculate bounding box again
        CalculateBoundingBox();
    }
  
    // general classes
    private void CenterGridOnNodes()
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        Vector3 newPosition = Vector3.zero;
        foreach (var child in children)
        {
            newPosition += child.position;
            child.parent = null;
        }
        newPosition /= children.Length;
        gameObject.transform.position = newPosition;
        foreach (var child in children)
        {
            child.parent = gameObject.transform;
        }
    }
    private void CalculateBoundingBox()
    {

        // calculate bounding box
        Renderer[] meshes = GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer mesh in meshes)
        {
            bounds.Encapsulate(mesh.bounds);
        }
        maximumScrollBounds = bounds;

    }
    private void PlaceInFrontOfPlayer(float distance)
    {
        Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
        flattenedVector.y = 0;
        flattenedVector.Normalize();
        Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * distance;
        transform.position = spawnPos;
        transform.LookAt(Player.instance.hmdTransform.position);
    }

    // testing
    private void TestRotationTechnique () {

        // need to be global
        Quaternion initialCubesRotation = Quaternion.identity;
        Quaternion initialControllerRotation = Quaternion.identity;
        bool set = false;

        if (NodeNavigationToggle[SteamVR_Input_Sources.LeftHand].state && NodeNavigationToggle[SteamVR_Input_Sources.RightHand].state)
        {
            Debug.Log("Two controllers!");
        }
        else if (NodeNavigationToggle[SteamVR_Input_Sources.LeftHand].state || NodeNavigationToggle[SteamVR_Input_Sources.RightHand].state)
        {
            Debug.Log("One controller!");
            if (NodeNavigationToggle[SteamVR_Input_Sources.RightHand].state)
            {
                if (set == false)
                {
                    initialCubesRotation = transform.rotation;
                    initialControllerRotation = Player.instance.rightHand.transform.rotation;
                    set = true;
                }
                Quaternion controllerAngularDifference = initialControllerRotation * Quaternion.Inverse(Player.instance.rightHand.transform.rotation);
                transform.rotation = controllerAngularDifference * Quaternion.Inverse(initialCubesRotation);
            }
        }
        else
        {
            set = false;
        }
    }
    private void GenerateTestGrid(Vector3 blueprint)
    {
        Object[] imagesAsTextures = Resources.LoadAll("Test Images", typeof(Material));

        // x
        for (int x = 0; x < blueprint.x; x++)
        {
            // y
            for (int y = 0; y < blueprint.y; y++)
            {
                // z
                for (int z = 0; z < blueprint.z; z++)
                {
                    // create cubes and give them spaced out positions
                    GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    node.transform.localScale = new Vector3(defaultNodeSize, defaultNodeSize, defaultNodeSize);
                    float spacingMultipler = defaultNodeSize + (defaultNodeSize * defaultNodeSpacing);
                    Vector3 nodePosition = new Vector3(x, y, z) * spacingMultipler;
                    node.transform.position = nodePosition;
                    node.transform.parent = transform;

                    // apply textures 
                    int imageId = Random.Range(0, imagesAsTextures.Length);
                    node.GetComponent<Renderer>().material = imagesAsTextures[imageId] as Material;
                }
            }
        }

        // center children on parent 
        Transform parent = gameObject.transform;
        Transform[] children = GetComponentsInChildren<Transform>();
        var pos = Vector3.zero;
        foreach (var child in children)
        {
            pos += child.position;
            child.parent = null;
        }
        pos /= children.Length;
        parent.position = pos;
        foreach (var child in children)
        {
            child.parent = parent;
            child.gameObject.SetActive(false);
        }
        parent.gameObject.SetActive(true);
    }
}
