using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class ViRMA_Cell : MonoBehaviour
{
    public Cell CellData;

    public IEnumerator CellSetup(Cell newCellData)
    {
        CellData = newCellData; 

        if (newCellData.Filtered)
        {
            Destroy(gameObject);
        }
        else
        {
            // use filename to assign image .dds as texture from local system folder
            string projectRoot = ViRMA_APIController.imagesDirectory;
            string imageNameDDS = CellData.ImageName.Substring(0, CellData.ImageName.Length - 4) + ".dds";

            // read bytes from local storage and convert it to Unity texture
            byte[] imageBytes = new byte[] { };
            Thread thread = new Thread(() => {
                imageBytes = File.ReadAllBytes(projectRoot + imageNameDDS);        
            });
            thread.Start();
            while (thread.IsAlive)
            {
                yield return null;
            }
            Texture2D imageTexture = ConvertImage(imageBytes);

            // apply new texture and make sure lights/shadows are off for performance
            GetComponent<Renderer>().material.mainTexture = imageTexture;
            GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            GetComponent<MeshRenderer>().receiveShadows = false;


            //GetComponent<MeshRenderer>().material = Resources.Load("Materials/UnlitTexture") as Material;
            //GetComponent<MeshRenderer>().material.color = new Color32(255, 0, 0, 255);
            
            //MaterialPropertyBlock materialSettings = new MaterialPropertyBlock();
            //materialSettings.SetColor("_Color", Color.red);
            //GetComponent<MeshRenderer>().SetPropertyBlock(materialSettings);

            //node.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, -1));
        }
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

    public void SetTexture()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector2[] UVs = new Vector2[mesh.vertices.Length];

        int target = 20;
        int totalImages = 33;

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
