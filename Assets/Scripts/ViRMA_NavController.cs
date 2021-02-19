using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_NavController : MonoBehaviour
{
    /* --- public --- */
    public SteamVR_ActionSet CellNavigationControls;
    public SteamVR_Action_Boolean CellNavigationToggle;

    /*--- private --- */

    // general
    private Rigidbody rigidBody;
    private float previousDistanceBetweenHands;
    private Bounds maximumScrollBounds;
    private GameObject boundingBox;

    // cell properties
    private float maxParentScale = 1.0f;
    private float minParentScale = 0.1f;
    private float defaultParentSize = 0.2f;
    private float defaultCellSpacingRatio = 1.5f;

    private void Awake()
    {
        // setup default components
        gameObject.AddComponent<Rigidbody>();
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.useGravity = false;
        rigidBody.drag = 0.1f;
        rigidBody.angularDrag = 0.5f;
    }

    private void Start()
    {
        // test dummy data
        List<ViRMA_APIController.CellParamHandler> paramHandlers = GenerateDummyData(); 

        // get an initial call too test
        StartCoroutine(ViRMA_APIController.GetCells(paramHandlers, (response) => {

            // retriece handled data from server
            List<ViRMA_APIController.Cell> cells = response;

            // generate cell axes
            //GenerateAxes(cells);

            // generate cells and their posiitons, centered on a parent
            GenerateCells(cells);

            // center the parent on all of the generated cubes
            CenterParentOnCells();

            // set initial size of parent
            //transform.localScale = Vector3.one * defaultParentSize;

            // calculate bounding box to set cells positional limits
            CalculateBoundingBox();

            // place cells in front of player
            //PlaceInFrontOfPlayer(8f);

            // activate navigation action controls
            //CellNavigationControls.Activate();


            // TESTING
            GameObject testSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(testSphere.GetComponent<Collider>());
            testSphere.transform.localScale = Vector3.one * 0.5f;
            testSphere.transform.position = boundingBox.transform.position;
            testSphere.transform.rotation = boundingBox.transform.rotation;
            testSphere.transform.parent = transform;
        }));
    }

    private void Update()
    {
        // control call navigation and spatial limitations each fixed frame
        CellNavigationController();
        CellNavigationLimiter();
    }

    // cell generation
    public void GenerateCells(List<ViRMA_APIController.Cell> cells)
    {
        // loop through all cells data from server
        foreach (var newCell in cells)
        {
            // create a primitive cube and set its scale to match image aspect ratio
            GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cube);
            float aspectRatio = 1.5f;
            node.transform.localScale = new Vector3(aspectRatio, 1, 1);

            // assign coordinates to cell from server using a pre-defined space multiplier
            Vector3 nodePosition = new Vector3(newCell.Coordinates.x, newCell.Coordinates.y, newCell.Coordinates.z) * (defaultCellSpacingRatio + 1);        
            node.transform.position = nodePosition;
            node.transform.parent = transform;

            // use filename to assign image .dds as texture from local system folder
            string projectRoot = ViRMA_APIController.imagesDirectory;
            string imageNameDDS = newCell.ImageName.Substring(0, newCell.ImageName.Length - 4) + ".dds";
            byte[] imageBytes = File.ReadAllBytes(projectRoot + imageNameDDS);
            Texture2D imageTexture = ConvertImage(imageBytes);
            node.GetComponent<Renderer>().material.mainTexture = imageTexture;
            //node.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, -1));    

            // for debugging
            // Debug.Log("X: " + newCell.Coordinates.x + " Y: " + newCell.Coordinates.y + " Z: " + newCell.Coordinates.z);
        }
    }  
    public static Texture2D ConvertImage(byte[] ddsBytes)
    {
        byte ddsSizeCheck = ddsBytes[4];
        if (ddsSizeCheck != 124)
        {
            throw new Exception("Invalid DDS DXTn texture size! (not 124)");
        }
        int height = ddsBytes[13] * 256 + ddsBytes[12];
        int width = ddsBytes[17] * 256 + ddsBytes[16];

        int ddsHeaderSize = 128;
        byte[] dxtBytes = new byte[ddsBytes.Length - ddsHeaderSize];
        Buffer.BlockCopy(ddsBytes, ddsHeaderSize, dxtBytes, 0, ddsBytes.Length - ddsHeaderSize);
        Texture2D texture = new Texture2D(width, height, TextureFormat.DXT1, false);

        texture.LoadRawTextureData(dxtBytes);
        texture.Apply();
        return (texture);
    }
    public void GenerateAxes(List<ViRMA_APIController.Cell> cells)
    {
        float maxX = 0;
        float maxY = 0;
        float maxZ = 0;
        foreach (var newCell in cells)
        {
            if (newCell.Coordinates.x > maxX)
            {
                maxX = newCell.Coordinates.x;
            }
            if (newCell.Coordinates.y > maxY)
            {
                maxY = newCell.Coordinates.y;
            }
            if (newCell.Coordinates.z > maxZ)
            {
                maxZ = newCell.Coordinates.z;
            }
        }
 
        GameObject oAxisObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        oAxisObj.GetComponent<Renderer>().material = Resources.Load("Materials/BasicTransparent") as Material;
        oAxisObj.GetComponent<Renderer>().material.color = new Color32(0, 0, 0, 255);
        oAxisObj.name = "Axis_Origin";
        oAxisObj.transform.position = Vector3.zero;
        //oAxisObj.transform.localScale = Vector3.one * 0.5f;
        oAxisObj.transform.parent = transform;

        for (int i = 0; i <= maxX; i++)
        {
            GameObject xAxisObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            xAxisObj.GetComponent<Renderer>().material = Resources.Load("Materials/BasicTransparent") as Material;
            xAxisObj.GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 130);
            xAxisObj.name = "Axis_X_" + i;
            xAxisObj.transform.position = new Vector3(i, 0, 0) * (defaultCellSpacingRatio + 1);
            //xAxisObj.transform.localScale = Vector3.one * 0.5f;
            xAxisObj.transform.parent = transform;
        }

        for (int i = 0; i <= maxY; i++)
        {
            GameObject yAxisObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            yAxisObj.GetComponent<Renderer>().material = Resources.Load("Materials/BasicTransparent") as Material;
            yAxisObj.GetComponent<Renderer>().material.color = new Color32(0, 255, 0, 130);
            yAxisObj.name = "Axis_Y_" + i;
            yAxisObj.transform.position = new Vector3(0, i, 0) * (defaultCellSpacingRatio + 1);
            //yAxisObj.transform.localScale = Vector3.one * 0.5f;
            yAxisObj.transform.parent = transform;
        }

        for (int i = 0; i <= maxZ; i++)
        {
            GameObject zAxisObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zAxisObj.GetComponent<Renderer>().material = Resources.Load("Materials/BasicTransparent") as Material;
            zAxisObj.GetComponent<Renderer>().material.color = new Color32(0, 0, 255, 130);
            zAxisObj.name = "Axis_Z_" + i;
            zAxisObj.transform.position = new Vector3(0, 0, i) * (defaultCellSpacingRatio + 1);
            // zAxisObj.transform.localScale = Vector3.one * 0.5f;
            zAxisObj.transform.parent = transform;
        }
    }
    private void CenterParentOnCells()
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        Vector3 newPosition = Vector3.one;
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


    // node navigation (position, rotation, scale)
    private void CellNavigationController()
    {
        if (CellNavigationToggle[SteamVR_Input_Sources.LeftHand].state && CellNavigationToggle[SteamVR_Input_Sources.RightHand].state)
        {
            // both triggers held down
            ToggleCellScaling();
        }
        else if (CellNavigationToggle[SteamVR_Input_Sources.LeftHand].state || CellNavigationToggle[SteamVR_Input_Sources.RightHand].state)
        {
            // one trigger held down
            if (CellNavigationToggle[SteamVR_Input_Sources.RightHand].state)
            {
                // right trigger held down
                ToggleCellPositioning();
            }

            if (CellNavigationToggle[SteamVR_Input_Sources.LeftHand].state)
            {
                // left trigger held down
                ToggleCellRotation();
            }
        }
        else
        {
            // no triggers held down
            if (previousDistanceBetweenHands != 0)
            {
                previousDistanceBetweenHands = 0;
            }
        }    
    }
    private void ToggleCellPositioning()
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        if (Player.instance.rightHand.GetTrackedObjectVelocity().magnitude > 0.5f)
        {
            /*      
            Vector3 localVelocity = transform.InverseTransformDirection(Player.instance.rightHand.GetTrackedObjectVelocity());
            localVelocity.x = 0;
            localVelocity.y = 0;
            localVelocity.z = 0;
            rigidBody.velocity = transform.TransformDirection(localVelocity) * 2f;
            */

            // scale throwing velocity with the size of the parent
            float parentMagnitude = transform.lossyScale.magnitude;
            float thrustAdjuster = parentMagnitude * 5f;
            Vector3 controllerVelocity = Player.instance.rightHand.GetTrackedObjectVelocity();
            rigidBody.velocity = controllerVelocity * thrustAdjuster;
        }
    }
    private void ToggleCellRotation()
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        Vector3 localAngularVelocity = transform.InverseTransformDirection(Player.instance.leftHand.GetTrackedObjectAngularVelocity());
        localAngularVelocity.x = 0;
        //localAngularVelocity.y = 0;
        localAngularVelocity.z = 0;
        rigidBody.angularVelocity = transform.TransformDirection(localAngularVelocity) * 0.1f;
    }
    private void ToggleCellScaling()
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        Vector3 leftHandPosition = Player.instance.leftHand.transform.position;
        Vector3 rightHandPosition = Player.instance.rightHand.transform.position;
        float thisFrameDistance = Mathf.Round(Vector3.Distance(leftHandPosition, rightHandPosition) * 100.0f) * 0.01f;

        if (previousDistanceBetweenHands == 0)
        {
            previousDistanceBetweenHands = thisFrameDistance;
        }
        else
        {
            if (thisFrameDistance > previousDistanceBetweenHands)
            {
                Vector3 targetScale = Vector3.one * maxParentScale;            
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 2f * Time.deltaTime);
            }
            if (thisFrameDistance < previousDistanceBetweenHands)
            {
                Vector3 targetScale = Vector3.one * minParentScale;
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 2f * Time.deltaTime);
            }
            previousDistanceBetweenHands = thisFrameDistance;
        }

        // calculate bounding box again
        CalculateBoundingBox();
    }
    private void CellNavigationLimiter()
    {
        if (Player.instance)
        {
            Vector3 currentVelocity = rigidBody.velocity;

            // x and z
            float boundary = Mathf.Max(Mathf.Max(maximumScrollBounds.size.x, maximumScrollBounds.size.y), maximumScrollBounds.size.z);
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


    // general methods 
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

        // toggle bounding box visibility for testing
        ToggleBoundingBox();
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


    // testing methods
    private List<ViRMA_APIController.CellParamHandler> GenerateDummyData()
    {
        // string url = "cell?xAxis={'AxisType': 'Tagset', 'TagsetId': 7}&yAxis={'AxisType': 'Tagset', 'TagsetId': 3}"; 
        // string url = "cell?xAxis={'AxisType': 'Tagset', 'TagsetId': 7}&yAxis={'AxisType': 'Tagset', 'TagsetId': 7}&zAxis={'AxisType': 'Tagset', 'TagsetId': 7}";
        // localhost:44317/api/cell/?xAxis={"AxisDirection":"X","AxisType":"Tagset","TagsetId":3,"HierarchyNodeId":0}&yAxis={"AxisDirection":"Y","AxisType":"Tagset","TagsetId":7,"HierarchyNodeId":0}&zAxis={"AxisDirection":"Z","AxisType":"Hierarchy","TagsetId":0,"HierarchyNodeId":77}

        List<ViRMA_APIController.CellParamHandler> paramHandlers = new List<ViRMA_APIController.CellParamHandler>();
        ViRMA_APIController.CellParamHandler paramHandler1 = new ViRMA_APIController.CellParamHandler
        {
            Call = "cell",
            Axis = "x",
            Id = 77,
            Type = "Hierarchy"
        };
        paramHandlers.Add(paramHandler1);
        ViRMA_APIController.CellParamHandler paramHandler2 = new ViRMA_APIController.CellParamHandler
        {
            Call = "cell",
            Axis = "y",
            Id = 7,
            Type = "Tagset"
        };
        paramHandlers.Add(paramHandler2);
        ViRMA_APIController.CellParamHandler paramHandler3 = new ViRMA_APIController.CellParamHandler
        {
            Call = "cell",
            Axis = "z",
            Id = 3,
            Type = "Tagset"
        };
        paramHandlers.Add(paramHandler3);
        return paramHandlers;
    }
    private void ToggleBoundingBox()
    {
        if (!boundingBox)
        {
            boundingBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(boundingBox.GetComponent<Collider>());
            boundingBox.GetComponent<Renderer>().material = Resources.Load("Materials/BasicTransparent") as Material;
            boundingBox.GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 130);
            boundingBox.name = "BoundingBox";
            boundingBox.transform.position = maximumScrollBounds.center;
            boundingBox.transform.localScale = maximumScrollBounds.size;
            boundingBox.transform.SetParent(transform);
        }     
    }
    private void CellRotationTest() {

        // need to be global
        Quaternion initialCubesRotation = Quaternion.identity;
        Quaternion initialControllerRotation = Quaternion.identity;
        bool set = false;

        if (CellNavigationToggle[SteamVR_Input_Sources.LeftHand].state && CellNavigationToggle[SteamVR_Input_Sources.RightHand].state)
        {
            Debug.Log("Two controllers!");
        }
        else if (CellNavigationToggle[SteamVR_Input_Sources.LeftHand].state || CellNavigationToggle[SteamVR_Input_Sources.RightHand].state)
        {
            Debug.Log("One controller!");
            if (CellNavigationToggle[SteamVR_Input_Sources.RightHand].state)
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
    private void GenerateTestCells(Vector3 blueprint)
    {
        UnityEngine.Object[] imagesAsTextures = Resources.LoadAll("Test Images", typeof(Material));

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
                    GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cell.transform.localScale = new Vector3(defaultParentSize, defaultParentSize, defaultParentSize);
                    float spacingMultipler = defaultParentSize + (defaultParentSize * defaultCellSpacingRatio);
                    Vector3 nodePosition = new Vector3(x, y, z) * spacingMultipler;
                    cell.transform.position = nodePosition;
                    cell.transform.parent = transform;

                    // apply textures 
                    int imageId = UnityEngine.Random.Range(0, imagesAsTextures.Length);
                    cell.GetComponent<Renderer>().material = imagesAsTextures[imageId] as Material;
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
