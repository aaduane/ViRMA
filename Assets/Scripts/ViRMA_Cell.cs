using UnityEngine;

public class ViRMA_Cell : MonoBehaviour
{
    public Cell thisCellData;
    private Mesh thisCellMesh;

    private void Awake()
    {
        thisCellMesh = GetComponent<MeshFilter>().mesh;
    }
    private void Start()
    {
        if (thisCellData.Filtered)
        {
            Destroy(gameObject);
        }
        else
        {
            GetComponent<MeshRenderer>().material = thisCellData.TextureArrayMaterial;
            SetTextureFromArray(thisCellData.TextureArrayId);
        }      
    }
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
}
