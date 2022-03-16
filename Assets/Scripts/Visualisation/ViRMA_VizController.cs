using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_VizController : MonoBehaviour
{
    /* --- public --- */

    // cells and axes objects
    [HideInInspector] public List<Cell> cellData;
    [HideInInspector] public List<GameObject> cellObjs, axisXPointObjs, axisYPointObjs, axisZPointObjs;
    [HideInInspector] public LineRenderer axisXLine, axisYLine, axisZLine;
    [HideInInspector] public AxesLabels activeAxesLabels;
    public GameObject focusedCell;
    public GameObject focusedAxisPoint;

    /*--- private --- */

    // general
    private ViRMA_GlobalsAndActions globals;
    public Rigidbody rigidBody;
    private float previousDistanceBetweenHands;
    private Bounds cellsAndAxesBounds;
    public Query activeQuery;
    public bool vizFullyLoaded;

    [HideInInspector] public bool activeBrowsingState;
    [HideInInspector] public Vector3 activeVizPosition;
    [HideInInspector] public Quaternion activeVizRotation;

    // cell properties
    public GameObject cellsandAxesWrapper;
    private float maxParentScale = 0.025f;
    private float minParentScale = 0.015f;
    private float defaultParentSize;
    private float defaultCellSpacingRatio = 1.5f;
    private string cellMaterial = "Materials/BasicTransparent";

    private void Awake()
    {
        // define ViRMA globals script
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        activeQuery = new Query();

        // setup cells and axes wrapper
        cellsandAxesWrapper = new GameObject("CellsAndAxesWrapper");

        // setup rigidbody
        gameObject.AddComponent<Rigidbody>();
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.useGravity = false;
        rigidBody.drag = 0.1f;
        rigidBody.angularDrag = 0.5f;
    }
    
    private void Update()
    {
        if (vizFullyLoaded)
        {
            // enable viz movement with SteamVR controllers
            CellNavigationController();

            // prevent viz from moving too far away if moving
            CellNavigationLimiter();
        }

        // draw axes line renderers 
        DrawAxesLines();
    }

    // cell and axes generation
    public IEnumerator SubmitVizQuery(Query submittedQuery)
    {
        // unload unsed textures
        Resources.UnloadUnusedAssets();

        // set loading flags to true and fade controllers
        vizFullyLoaded = false;
        globals.queryController.vizQueryLoading = true;
        globals.ToggleControllerFade(Player.instance.leftHand, true);
        globals.ToggleControllerFade(Player.instance.rightHand, true);

        // get cell data from server (WAIT FOR)
        yield return StartCoroutine(ViRMA_APIController.GetCells(submittedQuery, (cells) => {
            cellData = cells;
        }));

        // generate axes with axis labels (WAIT FOR)
        yield return StartCoroutine(GenerateAxesFromLabels(submittedQuery));

        // generate textures and texture arrays from local image storage
        GenerateTexturesAndTextureArrays(cellData);

        // generate cells and their posiitons, centered on a parent
        GenerateCells(cellData);

        // set center point of wrapper around cells and axes
        CenterParentOnCellsAndAxes();

        // calculate bounding box to set cells positional limits
        CalculateCellsAndAxesBounds();

        // DEBUG: show cells/axes bounds and bounds center
        //ToggleDebuggingBounds(); 

        // add cells and axes to final parent to set default starting scale and position
        SetupDefaultScaleAndPosition();

        // update main menu data to match new viz
        globals.mainMenu.FetchProjectedFilterMetadata();
        globals.mainMenu.FetchDirectFilterMetadata();

        // set loading flags to true and unfade controllers
        vizFullyLoaded = true;
        globals.queryController.vizQueryLoading = false;
        globals.ToggleControllerFade(Player.instance.leftHand, false);
        globals.ToggleControllerFade(Player.instance.rightHand, false);
    }
    private void GenerateTexturesAndTextureArrays(List<Cell> cellData)
    {
        if (cellData.Count > 0)
        {
            //float before = Time.realtimeSinceStartup; // testing

            // make a list of all the unique image textures present in the current query
            List<KeyValuePair<string, Texture2D>> uniqueTextures = new List<KeyValuePair<string, Texture2D>>();
            foreach (var newCell in cellData)
            {
                if (!newCell.Filtered)
                {
                    int index = uniqueTextures.FindIndex(a => a.Key == newCell.ImageName);
                    if (index == -1)
                    {
                        //byte[] imageBytes = File.ReadAllBytes(ViRMA_APIController.imagesDirectory + newCell.ImageName);
                        byte[] imageBytes = new byte[0];
                        try
                        {
                            imageBytes = File.ReadAllBytes(ViRMA_APIController.imagesDirectory + newCell.ImageName);
                            newCell.ImageTexture = ViRMA_APIController.ConvertImageFromDDS(imageBytes); // dds stuff
                            //newCell.ImageTexture = ViRMA_APIController.ConvertImageFromJPEG(imageBytes); // jpg stuff
                            KeyValuePair<string, Texture2D> uniqueTexture = new KeyValuePair<string, Texture2D>(newCell.ImageName, newCell.ImageTexture);
                            uniqueTextures.Add(uniqueTexture);
                        }
                        catch(FileNotFoundException e)
                        {
                            Debug.LogError(e.Message);
                            newCell.Filtered = true;
                        }
                    }
                    else
                    {
                        newCell.ImageTexture = uniqueTextures[index].Value;
                    }
                }
            }

            // calculate the number of texture arrays needed based on the size of the first texture in the list
            int textureWidth = uniqueTextures[0].Value.width; // e.g. 1024 or 684
            int textureHeight = uniqueTextures[0].Value.height; // e.g. 765 or 485
            int maxTextureArraySize = SystemInfo.maxTextureSize; // e.g. 16384 (most GPUs)
            int maxTexturesPerArray = maxTextureArraySize / textureHeight; // e.g. 22 or 33
            int totalTextureArrays = uniqueTextures.Count / maxTexturesPerArray + 1;

            for (int i = 0; i < totalTextureArrays; i++)
            {
                //Debug.Log("----------------- " + i + " -----------------"); // debugging

                if (i != totalTextureArrays - 1)
                {
                    Material newtextureArrayMaterial = new Material(Resources.Load(cellMaterial) as Material);
                    Texture2D newTextureArray = new Texture2D(textureWidth, textureHeight * maxTexturesPerArray, TextureFormat.DXT1, false); // dds stuff
                    //Texture2D newTextureArray = new Texture2D(textureWidth, textureHeight * maxTexturesPerArray, TextureFormat.RGB24, false); // jpg stuff
                    for (int j = 0; j < maxTexturesPerArray; j++)
                    {
                        int uniqueTextureIndex = j + maxTexturesPerArray * i;
                        Texture2D targetTexture = uniqueTextures[uniqueTextureIndex].Value;
                        if (targetTexture.width != textureWidth || targetTexture.height != textureHeight)
                        {
                            Debug.LogError("Texture " + uniqueTextures[uniqueTextureIndex].Key + " is not " + textureWidth + " x " + textureHeight + " and so will not fit properly in the texture array!");
                        }
                        Graphics.CopyTexture(targetTexture, 0, 0, 0, 0, targetTexture.width, targetTexture.height, newTextureArray, 0, 0, 0, targetTexture.height * j);

                        // Debug.Log(j + " | " + uniqueTextureIndex); // debugging

                        foreach (var cellDataObj in this.cellData)
                        {
                            if (cellDataObj.ImageName == uniqueTextures[uniqueTextureIndex].Key)
                            {
                                cellDataObj.TextureArrayId = j;
                                cellDataObj.TextureArrayMaterial = newtextureArrayMaterial;
                                cellDataObj.TextureArraySize = maxTexturesPerArray;
                            }
                        }
                    }
                    // newTextureArray.Compress(false);
                    newtextureArrayMaterial.mainTexture = newTextureArray;
                }
                else
                {
                    Material newtextureArrayMaterial = new Material(Resources.Load(cellMaterial) as Material);
                    int lastTextureArraySize = uniqueTextures.Count - (maxTexturesPerArray * (totalTextureArrays - 1));
                    Texture2D newTextureArray = new Texture2D(textureWidth, textureHeight * lastTextureArraySize, TextureFormat.DXT1, false); // dds stuff
                    //Texture2D newTextureArray = new Texture2D(textureWidth, textureHeight * lastTextureArraySize, TextureFormat.RGB24, false); // jpg stuff
                    for (int j = 0; j < lastTextureArraySize; j++)
                    {
                        int uniqueTextureIndex = j + maxTexturesPerArray * i;
                        Texture2D targetTexture = uniqueTextures[uniqueTextureIndex].Value;
                        if (targetTexture.width != textureWidth || targetTexture.height != textureHeight)
                        {
                            Debug.LogError("Texture " + uniqueTextures[uniqueTextureIndex].Key + " is not " + textureWidth + " x " + textureHeight + " and so will not fit properly in the texture array!");
                        }
                        Graphics.CopyTexture(targetTexture, 0, 0, 0, 0, targetTexture.width, targetTexture.height, newTextureArray, 0, 0, 0, targetTexture.height * j);

                        // Debug.Log(j + " | " + uniqueTextureIndex); // debugging

                        foreach (var cellDataObj in this.cellData)
                        {
                            if (cellDataObj.ImageName == uniqueTextures[uniqueTextureIndex].Key)
                            {
                                cellDataObj.TextureArrayId = j;
                                cellDataObj.TextureArrayMaterial = newtextureArrayMaterial;
                                cellDataObj.TextureArraySize = lastTextureArraySize;
                            }
                        }
                    }
                    // newTextureArray.Compress(false);
                    newtextureArrayMaterial.mainTexture = newTextureArray;
                }
            }

            //float after = Time.realtimeSinceStartup; // testing
            // Debug.Log("TEXTURE PARSE TIME ~ ~ ~ ~ ~ " + (after - before).ToString("n3") + " seconds"); // testing
        }
    }
    private void GenerateCells(List<Cell> cellData)
    {
        if (cellData.Count > 0)
        {
            // grab cell prefab from resoucres
            GameObject cellPrefab = Resources.Load("Prefabs/CellPrefab") as GameObject;

            // loop through all cells data from server
            foreach (var newCellData in cellData)
            {
                // create a primitive cube and set its scale to match image aspect ratio
                GameObject cell = Instantiate(cellPrefab);
                cell.AddComponent<ViRMA_Cell>().thisCellData = newCellData;

                // adjust aspect ratio
                float aspectRatio = 1.5f;
                cell.transform.localScale = new Vector3(aspectRatio, 1, aspectRatio);

                // assign coordinates to cell from server using a pre-defined space multiplier
                Vector3 nodePosition = new Vector3(newCellData.Coordinates.x, newCellData.Coordinates.y, newCellData.Coordinates.z) * (defaultCellSpacingRatio + 1);
                cell.transform.position = nodePosition;
                cell.transform.parent = cellsandAxesWrapper.transform;

                // name cell object and add it to a list of objects for reference
                cell.name = "Cell(" + newCellData.Coordinates.x + "," + newCellData.Coordinates.y + "," + newCellData.Coordinates.z + ")";
                cellObjs.Add(cell);
            }
        }   
    }
    private IEnumerator GenerateAxesFromLabels(Query submittedQuery)
    {
        activeAxesLabels = new AxesLabels();

        // get X label data from server
        if (submittedQuery.X != null)
        {
            string parentLabel = "";
            yield return StartCoroutine(ViRMA_APIController.GetHierarchyTag(submittedQuery.X.Id, (Tag parentTag) => {
                parentLabel = parentTag.Label;
            }));
            yield return StartCoroutine(ViRMA_APIController.GetHierarchyChildren(submittedQuery.X.Id, (List<Tag> childTags) => {
                activeAxesLabels.SetAxisLabsls("X", submittedQuery.X.Id, submittedQuery.X.Type, parentLabel, childTags);
            }));
        }

        // get Y label data from server
        if (submittedQuery.Y != null)
        {
            string parentLabel = "";
            yield return StartCoroutine(ViRMA_APIController.GetHierarchyTag(submittedQuery.Y.Id, (Tag parentTag) => {
                parentLabel = parentTag.Label;
            }));
            yield return StartCoroutine(ViRMA_APIController.GetHierarchyChildren(submittedQuery.Y.Id, (List<Tag> childTags) => {
                activeAxesLabels.SetAxisLabsls("Y", submittedQuery.Y.Id, submittedQuery.Y.Type, parentLabel, childTags);
            }));
        }

        // get Z label data from server
        if (submittedQuery.Z != null)
        {
            string parentLabel = "";
            yield return StartCoroutine(ViRMA_APIController.GetHierarchyTag(submittedQuery.Z.Id, (Tag parentTag) => {
                parentLabel = parentTag.Label;
            }));
            yield return StartCoroutine(ViRMA_APIController.GetHierarchyChildren(submittedQuery.Z.Id, (List<Tag> childTags) => {
                activeAxesLabels.SetAxisLabsls("Z", submittedQuery.Z.Id, submittedQuery.Z.Type, parentLabel, childTags);
            }));
        }    

        // // global style for propety blocks
        Material transparentMaterial = Resources.Load("Materials/BasicTransparent") as Material;
        MaterialPropertyBlock materialProperties = new MaterialPropertyBlock();
        float axisLineWidth = 0.005f;

        // origin point
        GameObject AxisOriginPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        AxisOriginPoint.GetComponent<Renderer>().material = transparentMaterial;
        materialProperties.SetColor("_Color", new Color32(0, 0, 0, 255));
        AxisOriginPoint.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
        AxisOriginPoint.name = "AxisOriginPoint";
        AxisOriginPoint.transform.position = Vector3.zero;
        AxisOriginPoint.transform.localScale = Vector3.one * 0.5f;
        AxisOriginPoint.transform.parent = cellsandAxesWrapper.transform;

        // add origin block to all axis object lists
        axisXPointObjs.Add(AxisOriginPoint);
        axisYPointObjs.Add(AxisOriginPoint);
        axisZPointObjs.Add(AxisOriginPoint);

        // x axis points
        if (activeAxesLabels.X != null)
        {
            materialProperties.SetColor("_Color", ViRMA_Colors.axisFadeRed);
            for (int i = 0; i < activeAxesLabels.X.Labels.Count; i++)
            {
                // create gameobject to represent axis point
                GameObject axisXPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
                axisXPoint.GetComponent<Renderer>().material = transparentMaterial;
                axisXPoint.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
                axisXPoint.name = "AxisXPoint_" + i;
                axisXPoint.transform.position = new Vector3(i + 1, 0, 0) * (defaultCellSpacingRatio + 1);
                axisXPoint.transform.localScale = Vector3.one * 0.5f;
                axisXPoint.transform.parent = cellsandAxesWrapper.transform;
                axisXPoint.AddComponent<ViRMA_AxisPoint>().x = true;

                // apply metadata to axis point
                ViRMA_AxisPoint axisPoint = axisXPoint.GetComponent<ViRMA_AxisPoint>();
                axisPoint.axisId = activeAxesLabels.X.Id;
                axisPoint.axisLabel = activeAxesLabels.X.Label;
                axisPoint.axisType = activeAxesLabels.X.Type;
                axisPoint.axisPointLabel = activeAxesLabels.X.Labels[i].Label;
                axisPoint.axisPointId = activeAxesLabels.X.Labels[i].Id;

                // add gameobject to list
                axisXPointObjs.Add(axisXPoint);

                // x axis roll up axis point
                if (i == activeAxesLabels.X.Labels.Count - 1)
                {             
                    GameObject axisXPointRollUp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    axisXPointRollUp.GetComponent<Renderer>().material = transparentMaterial;
                    axisXPointRollUp.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
                    axisXPointRollUp.name = "AxisXPoint_RollUp";
                    axisXPointRollUp.transform.position = new Vector3(i + 2, 0, 0) * (defaultCellSpacingRatio + 1);
                    axisXPointRollUp.transform.parent = cellsandAxesWrapper.transform;
                    axisXPointRollUp.AddComponent<ViRMA_RollUpPoint>().x = true;
                    axisXPointRollUp.GetComponent<ViRMA_RollUpPoint>().axisId = activeAxesLabels.X.Id;
                    axisXPointRollUp.GetComponent<ViRMA_RollUpPoint>().axisLabel = activeAxesLabels.X.Label;
                    axisXPointRollUp.GetComponent<ViRMA_RollUpPoint>().axisType = activeAxesLabels.X.Type;
                }
            }

            // x axis line
            if (axisXPointObjs.Count > 2)
            {
                GameObject AxisXLineObj = new GameObject("AxisXLine");
                axisXLine = AxisXLineObj.AddComponent<LineRenderer>();
                axisXLine.GetComponent<Renderer>().material = transparentMaterial;
                axisXLine.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
                axisXLine.positionCount = 2;
                axisXLine.startWidth = axisLineWidth;
                axisXLine.endWidth = axisLineWidth;
            }
        }

        // y axis points
        if (activeAxesLabels.Y != null)
        {
            materialProperties.SetColor("_Color", ViRMA_Colors.axisFadeGreen);
            for (int i = 0; i < activeAxesLabels.Y.Labels.Count; i++)
            {
                // create gameobject to represent axis point
                GameObject axisYPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
                axisYPoint.GetComponent<Renderer>().material = transparentMaterial;
                axisYPoint.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
                axisYPoint.name = "AxisYPoint_" + i;
                axisYPoint.transform.position = new Vector3(0, i + 1, 0) * (defaultCellSpacingRatio + 1);
                axisYPoint.transform.localScale = Vector3.one * 0.5f;
                axisYPoint.transform.parent = cellsandAxesWrapper.transform;
                axisYPoint.AddComponent<ViRMA_AxisPoint>().y = true;

                // apply metadata to axis point
                ViRMA_AxisPoint axisPoint = axisYPoint.GetComponent<ViRMA_AxisPoint>();
                axisPoint.axisId = activeAxesLabels.Y.Id;
                axisPoint.axisLabel = activeAxesLabels.Y.Label;
                axisPoint.axisType = activeAxesLabels.Y.Type;
                axisPoint.axisPointLabel = activeAxesLabels.Y.Labels[i].Label;
                axisPoint.axisPointId = activeAxesLabels.Y.Labels[i].Id;

                // add gameobject to list
                axisYPointObjs.Add(axisYPoint);

                // y axis roll up axis point
                if (i == activeAxesLabels.Y.Labels.Count - 1)
                {
                    GameObject axisYPointRollUp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    axisYPointRollUp.GetComponent<Renderer>().material = transparentMaterial;
                    axisYPointRollUp.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
                    axisYPointRollUp.name = "AxisYPoint_RollUp";
                    axisYPointRollUp.transform.position = new Vector3(0, i + 2, 0) * (defaultCellSpacingRatio + 1);
                    axisYPointRollUp.transform.parent = cellsandAxesWrapper.transform;
                    axisYPointRollUp.AddComponent<ViRMA_RollUpPoint>().y = true;
                    axisYPointRollUp.GetComponent<ViRMA_RollUpPoint>().axisId = activeAxesLabels.Y.Id;
                    axisYPointRollUp.GetComponent<ViRMA_RollUpPoint>().axisLabel = activeAxesLabels.Y.Label;
                    axisYPointRollUp.GetComponent<ViRMA_RollUpPoint>().axisType = activeAxesLabels.Y.Type;
                }
            }

            // y axis line
            if (axisYPointObjs.Count > 2)
            {
                GameObject AxisYLineObj = new GameObject("AxisYLine");
                axisYLine = AxisYLineObj.AddComponent<LineRenderer>();
                axisYLine.GetComponent<Renderer>().material = transparentMaterial;
                axisYLine.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
                axisYLine.positionCount = 2;
                axisYLine.startWidth = axisLineWidth;
                axisYLine.endWidth = axisLineWidth;
            }
        }

        // z axis points
        if (activeAxesLabels.Z != null)
        {
            materialProperties.SetColor("_Color", ViRMA_Colors.axisFadeBlue);
            for (int i = 0; i < activeAxesLabels.Z.Labels.Count; i++)
            {
                // create gameobject to represent axis point
                GameObject axisZPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
                axisZPoint.GetComponent<Renderer>().material = transparentMaterial;
                axisZPoint.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
                axisZPoint.name = "AxisZPoint_" + i;
                axisZPoint.transform.position = new Vector3(0, 0, i + 1) * (defaultCellSpacingRatio + 1);
                axisZPoint.transform.localScale = Vector3.one * 0.5f;
                axisZPoint.transform.parent = cellsandAxesWrapper.transform;
                axisZPoint.AddComponent<ViRMA_AxisPoint>().z = true;

                // apply metadata to axis point
                ViRMA_AxisPoint axisPoint = axisZPoint.GetComponent<ViRMA_AxisPoint>();
                axisPoint.axisId = activeAxesLabels.Z.Id;
                axisPoint.axisLabel = activeAxesLabels.Z.Label;
                axisPoint.axisType = activeAxesLabels.Z.Type;
                axisPoint.axisPointLabel = activeAxesLabels.Z.Labels[i].Label;
                axisPoint.axisPointId = activeAxesLabels.Z.Labels[i].Id;

                // add gameobject to list
                axisZPointObjs.Add(axisZPoint);

                // z axis roll up axis point
                if (i == activeAxesLabels.Z.Labels.Count - 1)
                {
                    GameObject axisZPointRollUp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    axisZPointRollUp.GetComponent<Renderer>().material = transparentMaterial;
                    axisZPointRollUp.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
                    axisZPointRollUp.name = "AxisYPoint_RollUp";
                    axisZPointRollUp.transform.position = new Vector3(0, 0, i + 2) * (defaultCellSpacingRatio + 1);
                    axisZPointRollUp.transform.parent = cellsandAxesWrapper.transform;
                    axisZPointRollUp.AddComponent<ViRMA_RollUpPoint>().z = true;
                    axisZPointRollUp.GetComponent<ViRMA_RollUpPoint>().axisId = activeAxesLabels.Z.Id;
                    axisZPointRollUp.GetComponent<ViRMA_RollUpPoint>().axisLabel = activeAxesLabels.Z.Label;
                    axisZPointRollUp.GetComponent<ViRMA_RollUpPoint>().axisType = activeAxesLabels.Z.Type;
                }
            }

            // z axis line
            if (axisZPointObjs.Count > 2)
            {
                GameObject AxisZLineObj = new GameObject("AxisZLine");
                axisZLine = AxisZLineObj.AddComponent<LineRenderer>();
                axisZLine.GetComponent<Renderer>().material = transparentMaterial;
                axisZLine.GetComponent<Renderer>().SetPropertyBlock(materialProperties);
                axisZLine.positionCount = 2;
                axisZLine.startWidth = axisLineWidth;
                axisZLine.endWidth = axisLineWidth;
            }
        }           
    }
    private void CalculateCellsAndAxesBounds()
    {
        Vector3 currentPosition = transform.position;

        transform.position = Vector3.zero;

        // calculate bounding box
        Renderer[] meshes = cellsandAxesWrapper.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(cellsandAxesWrapper.transform.position, Vector3.zero);
        foreach (Renderer mesh in meshes)
        {
            bounds.Encapsulate(mesh.bounds);
        }
        cellsAndAxesBounds = bounds;

        transform.position = currentPosition;
    }
    private void SetupDefaultScaleAndPosition()
    {
        // set wrapper position and parent cells/axes to wrapper and set default starting scale
        transform.position = cellsAndAxesBounds.center;
        cellsandAxesWrapper.transform.parent = transform;
        defaultParentSize = (maxParentScale + minParentScale) / 2f;
        transform.localScale = Vector3.one * defaultParentSize;

        // get the bounds of the newly resized cells/axes
        Renderer[] meshes = GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer mesh in meshes)
        {
            bounds.Encapsulate(mesh.bounds);
        }

        // if there is an active browsing state, maintain location of viz
        if (activeBrowsingState)
        {
            transform.position = activeVizPosition;
            transform.rotation = activeVizRotation;
        }
        else
        {
            // calculate distance to place cells/axes in front of player based on longest axis
            float distance = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);
            distance = distance < 1 ? 1.0f : distance;
            Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
            flattenedVector.y = 0;
            flattenedVector.Normalize();
            Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * distance;
            transform.position = spawnPos;
            transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position); // flip viz 180 degrees from 'LookAt'
        }

        // set new layer to prevent physical interactions with other objects on that layer
        foreach (Transform child in cellsandAxesWrapper.transform)
        {
            child.gameObject.layer = 9;
        }

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
    private void DrawAxesLines()
    {
        // x axis
        if (axisXLine)
        {
            if (axisXPointObjs.Count > 1)
            {
                axisXLine.SetPosition(0, axisXPointObjs[1].transform.position);
                axisXLine.SetPosition(1, axisXPointObjs[axisXPointObjs.Count - 1].transform.position);
            }
            if (axisXLine.transform.parent == null)
            {
                axisXLine.transform.parent = cellsandAxesWrapper.transform;
            }
        }

        // y axis
        if (axisYLine)
        {
            if (axisYPointObjs.Count > 1)
            {
                axisYLine.SetPosition(0, axisYPointObjs[1].transform.position);
                axisYLine.SetPosition(1, axisYPointObjs[axisYPointObjs.Count - 1].transform.position);
            }
            if (axisYLine.transform.parent == null)
            {
                axisYLine.transform.parent = cellsandAxesWrapper.transform;
            }
        }

        // z axis
        if (axisZLine)
        {
            if (axisZPointObjs.Count > 1)
            {
                axisZLine.SetPosition(0, axisZPointObjs[1].transform.position);
                axisZLine.SetPosition(1, axisZPointObjs[axisZPointObjs.Count - 1].transform.position);
            }
            if (axisZLine.transform.parent == null)
            {
                axisZLine.transform.parent = cellsandAxesWrapper.transform;
            }
        }
    }
    public void ClearViz()
    {
        cellObjs.Clear();
        axisXPointObjs.Clear();
        axisYPointObjs.Clear();
        axisZPointObjs.Clear();

        if (vizFullyLoaded)
        {
            // maintain active position after viz is loaded for the first time
            activeVizPosition = transform.position;
            activeVizRotation = transform.rotation;
            activeBrowsingState = true;
        }
        
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;

        foreach (Transform child in cellsandAxesWrapper.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void HideViz(bool toHide)
    {
        if (toHide)
        {
            if (vizFullyLoaded)
            {
                activeVizPosition = transform.position;
                activeVizRotation = transform.rotation;

                transform.position = new Vector3(0, 9999, 0);
                transform.rotation = Quaternion.identity;

                vizFullyLoaded = false;
            }                   
        }
        else
        {
            if (vizFullyLoaded == false)
            {
                transform.position = activeVizPosition;
                transform.rotation = activeVizRotation;

                vizFullyLoaded = true;
            }           
        }      
    }


    // node navigation (position, rotation, scale)
    private void CellNavigationController()
    {
        if (globals.vizNavActions.IsActive())
        {
            if (rigidBody.isKinematic)
            {
                rigidBody.isKinematic = false;
            }

            if (globals.vizNav_Position.GetState(SteamVR_Input_Sources.Any) && globals.vizNav_Rotation.GetState(SteamVR_Input_Sources.Any))
            {
                // ToggleCellScaling();
            }
            else if (globals.vizNav_Position.GetState(SteamVR_Input_Sources.Any) || globals.vizNav_Rotation.GetState(SteamVR_Input_Sources.Any))
            {
                if (globals.vizNav_Position.GetState(SteamVR_Input_Sources.Any))
                {
                    ToggleCellPositioning();
                }
                if (globals.vizNav_Rotation.GetState(SteamVR_Input_Sources.Any))
                {
                    ToggleCellRotation();
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
        else
        {
            if (!rigidBody.isKinematic)
            {
                rigidBody.isKinematic = true;
            }
        }
        
    }
    private void ToggleCellPositioning()
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        /*
        if (Player.instance.rightHand.GetTrackedObjectVelocity().magnitude > 0.5f)
        { 
            //Vector3 localVelocity = transform.InverseTransformDirection(Player.instance.rightHand.GetTrackedObjectVelocity());
            //localVelocity.x = 0;
            //localVelocity.y = 0;
            //localVelocity.z = 0;
            //rigidBody.velocity = transform.TransformDirection(localVelocity) * 2f;

            // scale throwing velocity with the size of the parent
            float parentMagnitude = transform.lossyScale.magnitude;
            float thrustAdjuster = parentMagnitude * 5f;
            Vector3 controllerVelocity = Player.instance.rightHand.GetTrackedObjectVelocity();
            rigidBody.velocity = controllerVelocity * thrustAdjuster;
        }
        */

        // adjust throwing velocity and drag
        Vector3 controllerVelocity = Player.instance.rightHand.GetTrackedObjectVelocity();
        if (!float.IsNaN(controllerVelocity.x) && !float.IsNaN(controllerVelocity.y) && !float.IsNaN(controllerVelocity.z))
        {
            rigidBody.velocity = controllerVelocity * 0.75f;
        }
        rigidBody.drag = 2.5f;
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

        rigidBody.angularDrag = 1f;
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
                defaultParentSize = transform.localScale.x;
            }
            if (thisFrameDistance < previousDistanceBetweenHands)
            {
                Vector3 targetScale = Vector3.one * minParentScale;
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 2f * Time.deltaTime);
                defaultParentSize = transform.localScale.x;
            }
            previousDistanceBetweenHands = thisFrameDistance;
        }

        // calculate bounding box again
        CalculateCellsAndAxesBounds();
    } 
    private void CellNavigationLimiter()
    {
        if (Player.instance && !rigidBody.isKinematic)
        {
            Vector3 currentVelocity = rigidBody.velocity;

            // x and z
            float boundary = Mathf.Max(Mathf.Max(cellsAndAxesBounds.size.x, cellsAndAxesBounds.size.y), cellsAndAxesBounds.size.z);
            boundary = boundary < 1 ? 1.0f : boundary;
            if (Vector3.Distance(transform.position, Player.instance.hmdTransform.transform.position) > boundary)
            {
                Vector3 normalisedDirection = (transform.position - Player.instance.hmdTransform.transform.position).normalized;
                Vector3 v = rigidBody.velocity;
                float d = Vector3.Dot(v, normalisedDirection);
                if (d > 0f) v -= normalisedDirection * d;
                rigidBody.velocity = v;
            }

            // y max
            float verticalBoundary = cellsAndAxesBounds.extents.y + 0.50f;
            float maxDistanceY = Player.instance.eyeHeight + verticalBoundary;
            if (transform.position.y >= maxDistanceY && currentVelocity.y > 0)
            {
                currentVelocity.y = 0;
                rigidBody.velocity = currentVelocity;
            }

            // y min
            float minDistanceY = Player.instance.eyeHeight - verticalBoundary;
            if (transform.position.y <= minDistanceY && currentVelocity.y < 0)
            {
                currentVelocity.y = 0;
                rigidBody.velocity = currentVelocity;
            }

        }
    }


    // node interaction (drill dowm, roll up, timeline for cell)
    public void DrillDownRollUp(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (focusedAxisPoint != null)
        {
            if (focusedAxisPoint.GetComponent<ViRMA_AxisPoint>())
            {
                ViRMA_AxisPoint axisPoint = focusedAxisPoint.GetComponent<ViRMA_AxisPoint>();
                if (axisPoint.axisType == "node")
                {
                    StartCoroutine(ViRMA_APIController.GetHierarchyChildren(axisPoint.axisPointId, (response) => {
                        List<Tag> children = response;
                        if (children.Count > 0)
                        {
                            string axisQueryType = "";
                            if (axisPoint.x)
                            {
                                axisQueryType = "X";
                            }
                            else if (axisPoint.y)
                            {
                                axisQueryType = "Y";
                            }
                            else if (axisPoint.z)
                            {
                                axisQueryType = "Z";
                            }
                            globals.queryController.buildingQuery.SetAxis(axisQueryType, axisPoint.axisPointId, "node");
                            // Debug.Log(children.Count + " children in " + axisPoint.axisPointLabel);
                        }
                        else
                        {
                            //Debug.Log("0 children in " + axisPoint.axisPointLabel);
                        }
                    }));
                }
            }
            else if (focusedAxisPoint.GetComponent<ViRMA_RollUpPoint>())
            {
                ViRMA_RollUpPoint axisPoint = focusedAxisPoint.GetComponent<ViRMA_RollUpPoint>();
                StartCoroutine(ViRMA_APIController.GetHierarchyParent(focusedAxisPoint.GetComponent<ViRMA_RollUpPoint>().axisId, (response) => {
                    Tag parent = response;
                    if (parent != null)
                    {
                        string axisQueryType = "";
                        if (axisPoint.x)
                        {
                            axisQueryType = "X";
                        }
                        else if (axisPoint.y)
                        {
                            axisQueryType = "Y";
                        }
                        else if (axisPoint.z)
                        {
                            axisQueryType = "Z";
                        }
                        globals.queryController.buildingQuery.SetAxis(axisQueryType, parent.Id, "node");
                        // Debug.Log("Parent: " + parent.Name);
                    }
                    else
                    {
                        //Debug.Log("No parent!");
                    }
                }));
            }
        }
    }
    public void SubmitCellForTimeline(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (focusedCell != null)
        {
            //globals.timeline.LoadTimelineData(focusedCell);
            globals.timeline.LoadCellContentTimelineData(focusedCell);
        }      
    }


    // testing and debugging
    private void ToggleDebuggingBounds()
    {
        // show bounds in-game for debugging
        GameObject debugBounds = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugBounds.name = "DebugBounds"; 
        Destroy(debugBounds.GetComponent<Collider>());
        debugBounds.GetComponent<Renderer>().material = Resources.Load("Materials/BasicTransparent") as Material;
        debugBounds.GetComponent<Renderer>().material.color = new Color32(100, 100, 100, 130);
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
    private void OrganiseHierarchy()
    {
        // add cells to hierarchy parent
        GameObject cellsParent = new GameObject("Cells");
        cellsParent.transform.parent = cellsandAxesWrapper.transform;
        cellsParent.transform.localScale = Vector3.one;
        foreach (var cell in cellObjs)
        {
            cell.transform.parent = cellsParent.transform;
        }

        // add axes to hierarchy parent
        GameObject axesParent = new GameObject("Axes");
        axesParent.transform.parent = cellsandAxesWrapper.transform;
        axesParent.transform.localScale = Vector3.one;
        foreach (GameObject point in axisXPointObjs)
        {
            point.transform.parent = axesParent.transform;
        }
        axisXLine.gameObject.transform.parent = axesParent.transform;
        foreach (GameObject point in axisYPointObjs)
        {
            point.transform.parent = axesParent.transform;
        }
        axisYLine.gameObject.transform.parent = axesParent.transform;
        foreach (GameObject point in axisZPointObjs)
        {
            point.transform.parent = axesParent.transform;
        }
        axisZLine.gameObject.transform.parent = axesParent.transform;
    }
}
