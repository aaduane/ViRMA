using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_VizController : MonoBehaviour
{
    /* --- public --- */

    // actions
    public SteamVR_ActionSet CellNavigationControls;
    public SteamVR_Action_Boolean CellNavigationToggle;

    // cells and axes objects
    [HideInInspector] public List<Cell> CellData;
    [HideInInspector] public List<GameObject> CellsGameObjs, AxisXPointsObjs, AxisYPointsObjs, AxisZPointsObjs;
    [HideInInspector] public LineRenderer AxisXLine, AxisYLine, AxisZLine;

    /*--- private --- */

    // general
    private Rigidbody rigidBody;
    private float previousDistanceBetweenHands;
    private Bounds cellsAndAxesBounds;

    // cell properties
    private GameObject cellsandAxesWrapper;
    private float maxParentScale = 1.0f;
    private float minParentScale = 0.1f;
    private float defaultParentSize = 0.2f;
    private float defaultCellSpacingRatio = 1.5f;

    private void Awake()
    {
        // setup cells and axes wrapper
        cellsandAxesWrapper = new GameObject("CellsAndAxesWrapper");

        // setup rigidbody
        gameObject.AddComponent<Rigidbody>();
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.useGravity = false;
        rigidBody.drag = 0.1f;
        rigidBody.angularDrag = 0.5f;       
    }

    private IEnumerator Start()
    {
        Query dummyQuery = new Query();

        dummyQuery.SetAxis("X", 3, "Tagset");
        dummyQuery.SetAxis("Y", 7, "Tagset");
        dummyQuery.SetAxis("Z", 77, "Hierarchy");

        //dummyQuery.SetAxis("X", 7, "Tagset");
        //dummyQuery.SetAxis("Y", 7, "Tagset");
        //dummyQuery.SetAxis("Z", 7, "Tagset");

        //dummyQuery.AddFilter(115, "Hierarchy");
        //dummyQuery.AddFilter(116, "Hierarchy");

        yield return StartCoroutine(ViRMA_APIController.GetCells(dummyQuery, (cells) => {
            CellData = cells;
        }));

        //TestTextureArrays(CellData);

        GenerateTextures(CellData);

        // generate cell axes
        GenerateAxes(CellData);

        // generate cells and their posiitons, centered on a parent
        GenerateCells(CellData);

        // set center point of wrapper around cells and axes
        CenterParentOnCellsAndAxes();

        // calculate bounding box to set cells positional limits
        CalculateCellsAndAxesBounds();

        // show cells/axes bounds and bounds center for debugging
        // ToggleDebuggingBounds(); 

        // add cells and axes to final parent to set default starting scale and position
        SetupDefaultScaleAndPosition();

        // so it does not affect bounds, organise hierarachy after everything is rendered
        OrganiseHierarchy();

        // activate navigation action controls
        CellNavigationControls.Activate();
    }

    private void Update()
    {
        // control call navigation and spatial limitations
        CellNavigationController();
        CellNavigationLimiter();

        // draw axes line renderers 
        DrawAxesLines();
    }


    // cell and axes generation
    private void GenerateCells(List<Cell> cellData)
    {
        // loop through all cells data from server
        foreach (var newCellData in cellData)
        {
            // create a primitive cube and set its scale to match image aspect ratio
            GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cell.AddComponent<ViRMA_Cell>().ThisCellData = newCellData;

            // adjust aspect ratio
            float aspectRatio = 1.5f;
            cell.transform.localScale = new Vector3(aspectRatio, 1, 1);

            // assign coordinates to cell from server using a pre-defined space multiplier
            Vector3 nodePosition = new Vector3(newCellData.Coordinates.x, newCellData.Coordinates.y, newCellData.Coordinates.z) * (defaultCellSpacingRatio + 1);
            cell.transform.position = nodePosition;
            cell.transform.parent = cellsandAxesWrapper.transform;

            // name cell object and add it to a list of objects for reference
            cell.name = "Cell(" + newCellData.Coordinates.x + "," + newCellData.Coordinates.y + "," + newCellData.Coordinates.z + ")";
            CellsGameObjs.Add(cell);
        }

    }  
    private void GenerateAxes(List<Cell> cells)
    {
        // get max cell axis values
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

        // reuse same material and just change colour property
        Material transparentMaterial = Resources.Load("Materials/BasicTransparent") as Material;
        MaterialPropertyBlock materialProperties = new MaterialPropertyBlock();
        Color32 transparentRed = new Color32(255, 0, 0, 130);
        Color32 transparentGreen = new Color32(0, 255, 0, 130);
        Color32 transparentBlue = new Color32(0, 0, 255, 130);

        // origin
        GameObject AxisOriginPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        AxisOriginPoint.GetComponent<Renderer>().material = transparentMaterial;
        materialProperties.SetColor("_Color", new Color32(0, 0, 0, 255));
        AxisOriginPoint.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
        AxisOriginPoint.name = "AxisOriginPoint";
        AxisOriginPoint.transform.position = Vector3.zero;
        AxisOriginPoint.transform.localScale = Vector3.one * 0.5f;
        AxisOriginPoint.transform.parent = cellsandAxesWrapper.transform;
        AxisXPointsObjs.Add(AxisOriginPoint);
        AxisYPointsObjs.Add(AxisOriginPoint);
        AxisZPointsObjs.Add(AxisOriginPoint);

        // x axis
        GameObject AxisXLineObj = new GameObject("AxisXLine");
        AxisXLine = AxisXLineObj.AddComponent<LineRenderer>();
        AxisXLine.GetComponent<Renderer>().material = transparentMaterial;
        materialProperties.SetColor("_Color", transparentRed);
        AxisXLine.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
        AxisXLine.positionCount = 2;
        AxisXLine.startWidth = 0.05f;
        for (int i = 0; i <= maxX; i++)
        {
            GameObject AxisXPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            AxisXPoint.GetComponent<Renderer>().material = transparentMaterial;
            AxisXPoint.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
            AxisXPoint.name = "AxisXPoint" + i;
            AxisXPoint.transform.position = new Vector3(i, 0, 0) * (defaultCellSpacingRatio + 1);
            AxisXPoint.transform.localScale = Vector3.one * 0.5f;
            AxisXPoint.transform.parent = cellsandAxesWrapper.transform;
            AxisXPointsObjs.Add(AxisXPoint);
        }

        // y axis
        GameObject AxisYLineObj = new GameObject("AxisYLine");
        AxisYLine = AxisYLineObj.AddComponent<LineRenderer>();
        AxisYLine.GetComponent<Renderer>().material = transparentMaterial;
        materialProperties.SetColor("_Color", transparentGreen);
        AxisYLine.GetComponent<Renderer>().SetPropertyBlock(materialProperties);

        AxisYLine.positionCount = 2;
        AxisYLine.startWidth = 0.05f;
        for (int i = 0; i <= maxY; i++)
        {
            GameObject AxisYPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            AxisYPoint.GetComponent<Renderer>().material = transparentMaterial;
            AxisYPoint.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
            AxisYPoint.name = "AxisYPoint" + i;
            AxisYPoint.transform.position = new Vector3(0, i, 0) * (defaultCellSpacingRatio + 1);
            AxisYPoint.transform.localScale = Vector3.one * 0.5f;
            AxisYPoint.transform.parent = cellsandAxesWrapper.transform;
            AxisYPointsObjs.Add(AxisYPoint);
        }

        // z axis
        GameObject AxisZLineObj = new GameObject("AxisZLine");
        AxisZLine = AxisZLineObj.AddComponent<LineRenderer>();
        AxisZLine.GetComponent<Renderer>().material = transparentMaterial;
        materialProperties.SetColor("_Color", transparentBlue);
        AxisZLine.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
        AxisZLine.positionCount = 2;
        AxisZLine.startWidth = 0.05f;
        for (int i = 0; i <= maxZ; i++)
        {
            GameObject AxisZPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            AxisZPoint.GetComponent<Renderer>().material = transparentMaterial;
            AxisZPoint.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
            AxisZPoint.name = "AxisZPoint" + i;
            AxisZPoint.transform.position = new Vector3(0, 0, i) * (defaultCellSpacingRatio + 1);
            AxisZPoint.transform.localScale = Vector3.one * 0.5f;
            AxisZPoint.transform.parent = cellsandAxesWrapper.transform;
            AxisZPointsObjs.Add(AxisZPoint);
        }
    }
    private void DrawAxesLines()
    {
        // x axis
        if (AxisXLine)
        {
            if (AxisXPointsObjs.Count > 1)
            {
                AxisXLine.SetPosition(0, AxisXPointsObjs[0].transform.position);
                AxisXLine.SetPosition(1, AxisXPointsObjs[AxisXPointsObjs.Count - 1].transform.position);
            }
        }

        // y axis
        if (AxisYLine)
        {
            if (AxisYPointsObjs.Count > 1)
            {
                AxisYLine.SetPosition(0, AxisYPointsObjs[0].transform.position);
                AxisYLine.SetPosition(1, AxisYPointsObjs[AxisYPointsObjs.Count - 1].transform.position);
            }
        }

        // z axis
        if (AxisZLine)
        {
            if (AxisZPointsObjs.Count > 1)
            {
                AxisZLine.SetPosition(0, AxisZPointsObjs[0].transform.position);
                AxisZLine.SetPosition(1, AxisZPointsObjs[AxisZPointsObjs.Count - 1].transform.position);
            }
        }
    }
    private void OrganiseHierarchy()
    {
        // add cells to hierarchy parent
        GameObject cellsParent = new GameObject("Cells");
        cellsParent.transform.parent = cellsandAxesWrapper.transform;
        foreach (var cell in CellsGameObjs)
        {
            cell.transform.parent = cellsParent.transform;
        }

        // add axes to hierarchy parent
        GameObject axesParent = new GameObject("Axes");
        axesParent.transform.parent = cellsandAxesWrapper.transform;
        foreach (GameObject point in AxisXPointsObjs)
        {
            point.transform.parent = axesParent.transform;
        }
        AxisXLine.gameObject.transform.parent = axesParent.transform;
        foreach (GameObject point in AxisYPointsObjs)
        {
            point.transform.parent = axesParent.transform;
        }
        AxisYLine.gameObject.transform.parent = axesParent.transform;
        foreach (GameObject point in AxisZPointsObjs)
        {
            point.transform.parent = axesParent.transform;
        }
        AxisZLine.gameObject.transform.parent = axesParent.transform;
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
        CalculateCellsAndAxesBounds();
    }
    private void CellNavigationLimiter()
    {
        if (Player.instance)
        {
            Vector3 currentVelocity = rigidBody.velocity;

            // x and z
            float boundary = Mathf.Max(Mathf.Max(cellsAndAxesBounds.size.x, cellsAndAxesBounds.size.y), cellsAndAxesBounds.size.z);
            if (Vector3.Distance(transform.position, Player.instance.hmdTransform.transform.position) > boundary)
            {
                Vector3 normalisedDirection = (transform.position - Player.instance.hmdTransform.transform.position).normalized;
                Vector3 v = rigidBody.velocity;
                float d = Vector3.Dot(v, normalisedDirection);
                if (d > 0f) v -= normalisedDirection * d;
                rigidBody.velocity = v;
            }

            // y max
            float maxDistanceY = Player.instance.eyeHeight + cellsAndAxesBounds.extents.y;
            if (transform.position.y >= maxDistanceY && currentVelocity.y > 0)
            {
                currentVelocity.y = 0;
                rigidBody.velocity = currentVelocity;
            }

            // y min
            float minDistanceY = Player.instance.eyeHeight - cellsAndAxesBounds.extents.y;
            if (transform.position.y <= minDistanceY && currentVelocity.y < 0)
            {
                currentVelocity.y = 0;
                rigidBody.velocity = currentVelocity;
            }

        }
    }


    // general  
    private void CalculateCellsAndAxesBounds()
    {
        // calculate bounding box
        Renderer[] meshes = cellsandAxesWrapper.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(cellsandAxesWrapper.transform.position, Vector3.zero);
        foreach (Renderer mesh in meshes)
        {
            bounds.Encapsulate(mesh.bounds);
        }
        cellsAndAxesBounds = bounds;
    }
    private void SetupDefaultScaleAndPosition()
    {
        // set wrapper position and parent cells/axes to wrapper and set default starting scale
        transform.position = cellsAndAxesBounds.center;
        cellsandAxesWrapper.transform.parent = transform;
        transform.localScale = Vector3.one * defaultParentSize;

        // get the bounds of the newly resized cells/axes
        Renderer[] meshes = GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer mesh in meshes)
        {
            bounds.Encapsulate(mesh.bounds);
        }

        // calculate distance to place cells/axes in front of player based on longest axis
        float distance = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);
        Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
        flattenedVector.y = 0;
        flattenedVector.Normalize();
        Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * distance;
        transform.position = spawnPos;
        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);

        // recalculate bounds to dertmine positional limits 
        CalculateCellsAndAxesBounds();
    }
    private void CenterParentOnCellsAndAxes()
    {
        Transform[] children = cellsandAxesWrapper.transform.GetComponentsInChildren<Transform>();
        Vector3 newPosition = Vector3.one;
        foreach (var child in children)
        {
            newPosition += child.position;
            child.parent = null;
        }
        newPosition /= children.Length;
        cellsandAxesWrapper.transform.position = newPosition;
        foreach (var child in children)
        {
            child.parent = cellsandAxesWrapper.transform;
        }
    }


    // debugging
    private void ToggleDebuggingBounds()
    {
        // show bounds in-game for debugging
        GameObject debugBounds = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugBounds.name = "DebugBounds"; 
        Destroy(debugBounds.GetComponent<Collider>());
        debugBounds.GetComponent<Renderer>().material = Resources.Load("Materials/BasicTransparent") as Material;
        debugBounds.GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 130);
        debugBounds.transform.position = cellsAndAxesBounds.center;
        debugBounds.transform.localScale = cellsAndAxesBounds.size;
        debugBounds.transform.SetParent(cellsandAxesWrapper.transform);
        debugBounds.transform.SetAsFirstSibling();

        // show center of bounds in-game for debugging
        GameObject debugBoundsCenter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugBoundsCenter.name = "DebugBoundsCenter";       
        Destroy(debugBoundsCenter.GetComponent<Collider>());
        debugBoundsCenter.GetComponent<Renderer>().material = Resources.Load("Materials/BasicTransparent") as Material;
        debugBoundsCenter.GetComponent<Renderer>().material.color = new Color32(0, 0, 0, 255);
        debugBoundsCenter.transform.position = cellsAndAxesBounds.center;
        debugBoundsCenter.transform.rotation = cellsandAxesWrapper.transform.rotation;
        debugBoundsCenter.transform.parent = cellsandAxesWrapper.transform;
        debugBoundsCenter.transform.SetAsFirstSibling();
    }
    private static Texture2D ConvertImage(byte[] ddsBytes)
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
    private void TestTextureArrays(List<Cell> cellData)
    {
        // CopyTexture(Texture src, int srcElement, int srcMip, int srcX, int srcY, int srcWidth, int srcHeight, Texture dst, int dstElement, int dstMip, int dstX, int dstY);
        // 684 x 484

        /*
        int destinationWidth = 684 * 2;
        int destinationHeight = 484 * 2;
        Texture2D destinationTexture = new Texture2D(destinationWidth, destinationHeight, TextureFormat.DXT1, false);

        Texture2D texture_1 = cellData[0].ImageTexture;
        Texture2D texture_2 = cellData[1].ImageTexture;
        Texture2D texture_3 = cellData[5].ImageTexture;
        Texture2D texture_4 = cellData[8].ImageTexture;

        Graphics.CopyTexture(texture_1, 0, 0, 0, 0, texture_1.width, texture_1.height, destinationTexture, 0, 0, 0, 0);
        Graphics.CopyTexture(texture_2, 0, 0, 0, 0, texture_2.width, texture_2.height, destinationTexture, 0, 0, 684, 0);
        Graphics.CopyTexture(texture_3, 0, 0, 0, 0, texture_3.width, texture_3.height, destinationTexture, 0, 0, 0, 484);
        Graphics.CopyTexture(texture_4, 0, 0, 0, 0, texture_4.width, texture_4.height, destinationTexture, 0, 0, 684, 484);

        GameObject testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testCube.transform.localScale = new Vector3(1.5f, 1, 1.5f);
        testCube.GetComponent<Renderer>().material.mainTexture = destinationTexture;
        testCube.AddComponent<TextureUVTest>();
        */

        /*
        int destinationWidth = 684;
        int destinationHeight = 484 * 4;
        Texture2D destinationTexture = new Texture2D(destinationWidth, destinationHeight, TextureFormat.DXT1, false);

        Texture2D texture_1 = cellData[0].ImageTexture;
        Texture2D texture_2 = cellData[1].ImageTexture;
        Texture2D texture_3 = cellData[5].ImageTexture;
        Texture2D texture_4 = cellData[8].ImageTexture;

        Graphics.CopyTexture(texture_1, 0, 0, 0, 0, texture_1.width, texture_1.height, destinationTexture, 0, 0, 0, 0);
        Graphics.CopyTexture(texture_2, 0, 0, 0, 0, texture_2.width, texture_2.height, destinationTexture, 0, 0, 0, 484);
        Graphics.CopyTexture(texture_3, 0, 0, 0, 0, texture_3.width, texture_3.height, destinationTexture, 0, 0, 0, 484 * 2);
        Graphics.CopyTexture(texture_4, 0, 0, 0, 0, texture_4.width, texture_4.height, destinationTexture, 0, 0, 0, 484 * 3);

        GameObject testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testCube.transform.localScale = new Vector3(1.5f, 1, 1.5f);
        testCube.GetComponent<Renderer>().material.mainTexture = destinationTexture;
        testCube.AddComponent<TextureUVTest>();
        */

        

        byte[] imageBytes1 = File.ReadAllBytes("C:/Users/aaron/Downloads/1.jpg");
        Texture2D texture_1 = new Texture2D(1, 1);
        texture_1.LoadImage(imageBytes1);

        byte[] imageBytes2 = File.ReadAllBytes("C:/Users/aaron/Downloads/2.jpg");
        Texture2D texture_2 = new Texture2D(1, 1);
        texture_2.LoadImage(imageBytes2);

        byte[] imageBytes3 = File.ReadAllBytes("C:/Users/aaron/Downloads/4.jpg");
        Texture2D texture_3 = new Texture2D(1, 1);
        texture_3.LoadImage(imageBytes3); 

        byte[] imageBytes4 = File.ReadAllBytes("C:/Users/aaron/Downloads/4.jpg");
        Texture2D texture_4 = new Texture2D(1, 1);
        texture_4.LoadImage(imageBytes4);

        bool isEqual = false;
        int equalChecker = 0;
        for (int i = 0; i < imageBytes3.Length; i++)
        {
            if (imageBytes3[i] == imageBytes4[i])
            {
                equalChecker++;
            }
            if (equalChecker == imageBytes3.Length)
            {
                isEqual = true;
            }
        }
        Debug.Log(isEqual);


        int destinationWidth = 1024;
        int destinationHeight = 765 * 4;
        Texture2D destinationTexture = new Texture2D(destinationWidth, destinationHeight, TextureFormat.RGB24, false);
        //Texture2DArray testArray = new Texture2DArray(destinationWidth, destinationHeight, 4, TextureFormat.RGB24, false); // testing

        Graphics.CopyTexture(texture_1, 0, 0, 0, 0, texture_1.width, texture_1.height, destinationTexture, 0, 0, 0, 0);
        Graphics.CopyTexture(texture_2, 0, 0, 0, 0, texture_2.width, texture_2.height, destinationTexture, 0, 0, 0, 765);
        Graphics.CopyTexture(texture_3, 0, 0, 0, 0, texture_3.width, texture_3.height, destinationTexture, 0, 0, 0, 765 * 2);
        Graphics.CopyTexture(texture_4, 0, 0, 0, 0, texture_4.width, texture_4.height, destinationTexture, 0, 0, 0, 765 * 3);

        GameObject testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testCube.transform.localScale = new Vector3(1.5f, 1, 1.5f);
        testCube.GetComponent<Renderer>().material.mainTexture = destinationTexture;
        testCube.AddComponent<TextureUVTest>();
    }




    private void GenerateTextures(List<Cell> cellData)
    {
        List<KeyValuePair<string, Texture2D>> uniqueTextures = new List<KeyValuePair<string, Texture2D>>();
        foreach (var newCell in cellData)
        {   
            if (!newCell.Filtered)
            {
                int index = uniqueTextures.FindIndex(a => a.Key == newCell.ImageName);
                if (index == -1)
                {
                    byte[] imageBytes = File.ReadAllBytes(ViRMA_APIController.imagesDirectory + newCell.ImageName);
                    newCell.ImageTexture = ConvertImage(imageBytes);
                    KeyValuePair<string, Texture2D> uniqueTexture = new KeyValuePair<string, Texture2D>(newCell.ImageName, newCell.ImageTexture);
                    uniqueTextures.Add(uniqueTexture);
                }
                else
                {
                    newCell.ImageTexture = uniqueTextures[index].Value;
                }
            }      
        }

        int textureWidth = uniqueTextures[0].Value.width; // 684
        int textureHeight = uniqueTextures[0].Value.height; // 485
        int maxTextureArraySize = SystemInfo.maxTextureSize; // 16k+
        int maxTexturesPerArray = maxTextureArraySize / textureHeight; // 33
        int totalTextureArrays = uniqueTextures.Count / maxTexturesPerArray + 1; // 3       

        for (int i = 0; i < totalTextureArrays; i++)
        {
            //Debug.Log("----------------- " + i + " -----------------"); // debugging

            if (i != totalTextureArrays - 1)
            {
                Material newtextureArrayMaterial = new Material(Resources.Load("Materials/CellMaterial") as Material);

                Texture2D newTextureArray = new Texture2D(textureWidth, textureHeight * maxTexturesPerArray, TextureFormat.DXT1, false);
                for (int j = 0; j < maxTexturesPerArray; j++)
                {
                    int uniqueTextureIndex = j + maxTexturesPerArray * i;
                    Texture2D targetTexture = uniqueTextures[uniqueTextureIndex].Value;
                    Graphics.CopyTexture(targetTexture, 0, 0, 0, 0, targetTexture.width, targetTexture.height, newTextureArray, 0, 0, 0, targetTexture.height * j);

                    //Debug.Log(j + " | " + uniqueTextureIndex); // debugging


                    foreach (var cellDataObj in CellData)
                    {
                        if (cellDataObj.ImageName == uniqueTextures[uniqueTextureIndex].Key)
                        {
                            cellDataObj.TextureArrayId = j;
                            cellDataObj.TextureArrayMaterial = newtextureArrayMaterial;
                            cellDataObj.TextureArraySize = maxTexturesPerArray;
                        }
                    }
                }
                newtextureArrayMaterial.mainTexture = newTextureArray;
            }
            else
            {
                Material newtextureArrayMaterial = new Material(Resources.Load("Materials/CellMaterial") as Material);

                int lastTextureArraySize = uniqueTextures.Count - (maxTexturesPerArray * (totalTextureArrays - 1)); // 15
                Texture2D newTextureArray = new Texture2D(textureWidth, textureHeight * lastTextureArraySize, TextureFormat.DXT1, false);
                for (int j = 0; j < lastTextureArraySize; j++)
                {
                    int uniqueTextureIndex = j + maxTexturesPerArray * i;
                    Texture2D targetTexture = uniqueTextures[uniqueTextureIndex].Value;
                    Graphics.CopyTexture(targetTexture, 0, 0, 0, 0, targetTexture.width, targetTexture.height, newTextureArray, 0, 0, 0, targetTexture.height * j);

                    // Debug.Log(j + " | " + uniqueTextureIndex); // debugging

                    foreach (var cellDataObj in CellData)
                    {
                        if (cellDataObj.ImageName == uniqueTextures[uniqueTextureIndex].Key)
                        {
                            cellDataObj.TextureArrayId = j;
                            cellDataObj.TextureArrayMaterial = newtextureArrayMaterial;
                            cellDataObj.TextureArraySize = lastTextureArraySize;
                        }
                    }
                }               
                newtextureArrayMaterial.mainTexture = newTextureArray;
            }
        }

    }
}
