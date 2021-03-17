using System.Collections.Generic;
using UnityEngine;

public class TextureUVTest : MonoBehaviour
{
    /*
    void Start()
    {

        Mesh mesh = GetComponent<MeshFilter>().mesh;         
        Vector2[] UVs = new Vector2[mesh.vertices.Length];

        float totalImages = 4;
        float rows = totalImages / 2;
        float increment = 1 / rows;


        float i = 3; // target image index


        float x = 0;
        float y = 0;     
        if (i == 0)
        {
            // first
            x = 0;
            y = 0;
        }
        else if (i == totalImages - 1)
        {
            // last
            x = increment;
            y = increment;
        }
        else if (i % 2 == 0)
        {
            // even 
            x = 0;
            y = increment;
        }
        else
        {
            x = increment;
            y = 0;
        }



        Vector2 bottomLeft = new Vector2(x, y);
        Vector2 bottomRight = new Vector2(x + increment, y);
        Vector2 topLeft = new Vector2(x, y + increment);
        Vector2 topRight = new Vector2(x + increment, y + increment);

        //Vector2 bottomLeft = new Vector2(0.0f, 0.0f);
        //Vector2 bottomRight = new Vector2(0.5f, 0.0f);
        //Vector2 topLeft = new Vector2(0.0f, 0.5f);
        //Vector2 topRight = new Vector2(0.5f, 0.5f);

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
    */

    void Start()
    {

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector2[] UVs = new Vector2[mesh.vertices.Length];

        int target = 1;
        int totalImages = 4;

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
        UVs[0] = bottomLeft;        // bottom-left
        UVs[1] = bottomRight;       // bottom-right
        UVs[2] = topLeft;           // top-left
        UVs[3] = topRight;          // top-right

        //UVs[0] = topLeft;              // bottom-left
        //UVs[1] = topRight;             // bottom-right
        //UVs[2] = bottomLeft;           // top-left
        //UVs[3] = bottomRight;          // top-right

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
