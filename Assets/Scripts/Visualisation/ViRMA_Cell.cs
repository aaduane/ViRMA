using TMPro;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_Cell : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    public Cell thisCellData;

    private Renderer thisCellRend;
    private Mesh thisCellMesh;
    private GameObject axesLabels;

    public MaterialPropertyBlock cellRendPropBlock;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        thisCellRend = GetComponent<Renderer>();
        thisCellMesh = GetComponent<MeshFilter>().mesh;

        cellRendPropBlock = new MaterialPropertyBlock();
    }
    private void Start()
    {
        if (thisCellData.Filtered)
        {
            // destroy the cell gameobject if the result has been filtered
            Destroy(gameObject);
        }
        else
        {
            // set material with texture array containing many images
            thisCellRend.material = thisCellData.TextureArrayMaterial;

            // use id to only show relevant image on the mesh from the material texture array
            SetTextureFromArray(thisCellData.TextureArrayId);
        }
    }
    private void Update()
    {
        CellStateController();
    }

    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.vizController.focusedCell = gameObject;

            globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, true);
        }
    }

    private void OnTriggerStay(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.vizController.focusedCell = gameObject;

            globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, true);
        }
    }

    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            if (globals.vizController.focusedCell == gameObject)
            {
                globals.vizController.focusedCell = null;

                globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, false);
            }
        }
    }

    // update
    private void CellStateController()
    {
        if (globals.vizController.focusedCell == null)
        {
            ToggleFade(false);
            ToggleAxesLabels(false);
        }
        else
        {
            if (globals.vizController.focusedCell == gameObject)
            {
                ToggleFade(false);
                ToggleAxesLabels(true);
            }
            else
            {
                ToggleFade(true);
                ToggleAxesLabels(false);
            }
        }
    }

    // general
    public void SetTextureFromArray(int textureIndexInArray)
    {
        Vector2[] UVs = new Vector2[thisCellMesh.vertices.Length];
        int totalTexturesInArray = thisCellData.TextureArraySize;
        float textureOffset = 1.0f / totalTexturesInArray;
        float textureLocationInArray = textureIndexInArray * textureOffset;
        if (textureIndexInArray >= totalTexturesInArray)
        {
            Debug.LogError("Warning! Target texture index above texture array count.");
        }

        Vector2 bottomLeftOfTexture = new Vector2(0, textureLocationInArray);
        Vector2 bottomRightOfTexture = new Vector2(1, textureLocationInArray);
        Vector2 topLeftOfTexture = new Vector2(0, textureLocationInArray + textureOffset);
        Vector2 topRightOfTexture = new Vector2(1, textureLocationInArray + textureOffset);

        /* - - - - - - - DDS Images - - - - - - - -*/

        
        // front of cube
        UVs[2] = bottomLeftOfTexture;
        UVs[3] = bottomRightOfTexture;
        UVs[0] = topLeftOfTexture;
        UVs[1] = topRightOfTexture;

        // top of cube
        UVs[8] = topLeftOfTexture;
        UVs[9] = topRightOfTexture;
        UVs[4] = bottomLeftOfTexture;
        UVs[5] = bottomRightOfTexture;

        // back of cube
        UVs[10] = bottomRightOfTexture;
        UVs[11] = bottomLeftOfTexture;
        UVs[6] = topRightOfTexture;
        UVs[7] = topLeftOfTexture;

        // bottom of cube
        UVs[14] = bottomLeftOfTexture;
        UVs[15] = topLeftOfTexture;
        UVs[12] = topRightOfTexture;
        UVs[13] = bottomRightOfTexture;

        // left of cube
        UVs[18] = bottomLeftOfTexture;
        UVs[19] = topLeftOfTexture;
        UVs[16] = topRightOfTexture;
        UVs[17] = bottomRightOfTexture;

        // right of cube
        UVs[22] = bottomLeftOfTexture;
        UVs[23] = topLeftOfTexture;
        UVs[20] = topRightOfTexture;
        UVs[21] = bottomRightOfTexture;
        

        /* - - - - - - - JPG Images - - - - - - - -*/

        /*
        // front of cube
        UVs[0] = bottomLeftOfTexture;        
        UVs[1] = bottomRightOfTexture;       
        UVs[2] = topLeftOfTexture;           
        UVs[3] = topRightOfTexture;          

        // top of cube
        UVs[4] = topLeftOfTexture;
        UVs[5] = topRightOfTexture;
        UVs[8] = bottomLeftOfTexture;
        UVs[9] = bottomRightOfTexture;

        // back of cube
        UVs[6] = bottomRightOfTexture;
        UVs[7] = bottomLeftOfTexture;
        UVs[10] = topRightOfTexture;
        UVs[11] = topLeftOfTexture;

        // bottom of cube
        UVs[12] = bottomLeftOfTexture;
        UVs[13] = topLeftOfTexture;
        UVs[14] = topRightOfTexture;
        UVs[15] = bottomRightOfTexture;

        // left of cube
        UVs[16] = bottomLeftOfTexture;
        UVs[17] = topLeftOfTexture;
        UVs[18] = topRightOfTexture;
        UVs[19] = bottomRightOfTexture;

        // right of cube
        UVs[20] = bottomLeftOfTexture;
        UVs[21] = topLeftOfTexture;
        UVs[22] = topRightOfTexture;
        UVs[23] = bottomRightOfTexture;
        */

        thisCellMesh.uv = UVs;
    }
    public void ToggleFade(bool toFade)
    {
        float alpha = 0;
        if (toFade)
        {
            alpha = 0.25f;
        }
        else
        {
            alpha = 1.0f;
        }
        Color32 newColorWithFade = new Color(1.0f, 1.0f, 1.0f, alpha);
        thisCellRend.GetPropertyBlock(cellRendPropBlock);
        cellRendPropBlock.SetColor("_Color", newColorWithFade);
        thisCellRend.SetPropertyBlock(cellRendPropBlock);
    }
    public void ToggleAxesLabels(bool showHide)
    {
        if (showHide)
        {
            if (axesLabels == null)
            {
                // x 
                int xAxisPointIndex = (int)thisCellData.Coordinates.x;
                GameObject xAxisPointObj = globals.vizController.axisXPointObjs[xAxisPointIndex];
                string xAxisPointLabel = xAxisPointObj.GetComponent<ViRMA_AxisPoint>().axisPointLabel;

                // y
                int yAxisPointIndex = (int)thisCellData.Coordinates.y;
                GameObject yAxisPointObj = globals.vizController.axisYPointObjs[yAxisPointIndex];
                string yAxisPointLabel = yAxisPointObj.GetComponent<ViRMA_AxisPoint>().axisPointLabel;            

                // z 
                int zAxisPointIndex = (int)thisCellData.Coordinates.z;
                GameObject zAxisPointObj = globals.vizController.axisZPointObjs[zAxisPointIndex];
                string zAxisPointLabel = zAxisPointObj.GetComponent<ViRMA_AxisPoint>().axisPointLabel;

                axesLabels = Instantiate(Resources.Load("Prefabs/AxesLabels")) as GameObject;
                axesLabels.transform.SetParent(transform.parent.transform);
                axesLabels.transform.localScale = Vector3.one * 0.3f;
                axesLabels.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 3f, transform.localPosition.z);
                axesLabels.transform.LookAt(2 * axesLabels.transform.position - Player.instance.hmdTransform.position);

                axesLabels.transform.GetChild(0).GetComponent<TextMeshPro>().text = xAxisPointLabel;
                axesLabels.transform.GetChild(0).GetComponent<TextMeshPro>().color = ViRMA_Colors.axisRed;

                axesLabels.transform.GetChild(1).GetComponent<TextMeshPro>().text = yAxisPointLabel;
                axesLabels.transform.GetChild(1).GetComponent<TextMeshPro>().color = ViRMA_Colors.axisGreen;

                axesLabels.transform.GetChild(2).GetComponent<TextMeshPro>().text = zAxisPointLabel;
                axesLabels.transform.GetChild(2).GetComponent<TextMeshPro>().color = ViRMA_Colors.axisBlue;
            }
        }
        else
        {
            if (axesLabels != null)
            {
                Destroy(axesLabels);
                axesLabels = null;
            }

        }
    }


    /*
    private void OnHandHoverBegin(Hand hand)
    {
        Debug.Log("Old A");

        globals.ToggleControllerFade(hand, true);

        //globals.vizController.focusedCell = gameObject;
        ToggleAxesLabels(true);

        if (globals.vizController.cellObjs.Count > 0)
        {
            foreach (GameObject cell in globals.vizController.cellObjs)
            {
                if (cell != gameObject)
                {
                    cell.GetComponent<ViRMA_Cell>().ToggleFade(true);
                }
            }
        }
    }
    private void OnHandHoverEnd(Hand hand)
    {
        Debug.Log("Old B");

        globals.ToggleControllerFade(hand, false);

        //globals.vizController.focusedCell = globals.vizController.axisXPointObjs[0];
        ToggleAxesLabels(false);

        if (globals.vizController.cellObjs.Count > 0)
        {
            foreach (GameObject cell in globals.vizController.cellObjs)
            {
                if (cell != gameObject)
                {
                    cell.GetComponent<ViRMA_Cell>().ToggleFade(false);
                }
            }
        }
    }
    
    public void OnHoverStart(Hand hand)
    {
        Debug.Log("New A");

        globals.ToggleControllerFade(hand, true);

        globals.vizController.focusedCell = gameObject;

        if (globals.vizController.cellObjs.Count > 0)
        {
            foreach (GameObject cell in globals.vizController.cellObjs)
            {
                if (cell != gameObject)
                {
                    cell.GetComponent<ViRMA_Cell>().ToggleFade(true);
                }
            }
        }
    }
    public void OnHoverEnd(Hand hand)
    {
        Debug.Log("New B");

        globals.ToggleControllerFade(hand, false);

        globals.vizController.focusedCell = globals.vizController.axisXPointObjs[0];

        if (globals.vizController.cellObjs.Count > 0)
        {
            foreach (GameObject cell in globals.vizController.cellObjs)
            {
                if (cell != gameObject)
                {
                    cell.GetComponent<ViRMA_Cell>().ToggleFade(false);
                }
            }
        }
    }
    */


}
