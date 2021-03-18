using UnityEngine;

public class ViRMA_Cell : MonoBehaviour
{
    public Cell ThisCellData;

    private void Start()
    {
        if (ThisCellData.Filtered)
        {
            Destroy(gameObject);
        }
        else
        {
            GetComponent<MeshRenderer>().material = ThisCellData.TextureArrayMaterial;
            SetTextureFromArray(ThisCellData.TextureArrayId);
        }      
    }

    public void SetTextureFromArray(int target)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector2[] UVs = new Vector2[mesh.vertices.Length];

        int totalImages = ThisCellData.TextureArraySize;

        if (target >= totalImages)
        {
            Debug.LogError("Warning! Target texture index above tyexture array count.");
        }
        float increment = 1.0f / totalImages;
        float modifier = increment * target;

        Vector2 bottomLeft = new Vector2(0, modifier);
        Vector2 bottomRight = new Vector2(1, modifier);
        Vector2 topLeft = new Vector2(0, modifier + increment);
        Vector2 topRight = new Vector2(1, modifier + increment);


        // front
        //UVs[0] = bottomLeft;        // bottom-left
        //UVs[1] = bottomRight;       // bottom-right
        //UVs[2] = topLeft;           // top-left
        //UVs[3] = topRight;          // top-right

        UVs[0] = topLeft;              // bottom-left
        UVs[1] = topRight;             // bottom-right
        UVs[2] = bottomLeft;           // top-left
        UVs[3] = bottomRight;          // top-right

        // top
        UVs[4] = topLeft;           // top-left
        UVs[5] = topRight;          // top-right
        UVs[8] = bottomLeft;        // bottom-left
        UVs[9] = bottomRight;       // bottom-right   

        // back
        UVs[6] = bottomRight;        // bottom-right
        UVs[7] = bottomLeft;         // bottom-left
        UVs[10] = topRight;          // top-right
        UVs[11] = topLeft;           // top-left      

        // bottom
        UVs[12] = bottomLeft;        // bottom-left
        UVs[13] = topLeft;           // top-left
        UVs[14] = topRight;          // top-right
        UVs[15] = bottomRight;       // bottom-right

        // left
        UVs[16] = bottomLeft;        // bottom-left
        UVs[17] = topLeft;           // top-left
        UVs[18] = topRight;          // top-right
        UVs[19] = bottomRight;       // bottom-right

        // right
        UVs[20] = bottomLeft;        // bottom-left
        UVs[21] = topLeft;           // top-left
        UVs[22] = topRight;          // top-right
        UVs[23] = bottomRight;       // bottom-right

        mesh.uv = UVs;
    }

}
