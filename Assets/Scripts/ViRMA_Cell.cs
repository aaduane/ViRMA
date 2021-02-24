using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ViRMA_Cell : MonoBehaviour
{
    public ViRMA_APIController.Cell CellData;

    public void CellSetup(ViRMA_APIController.Cell newCellData)
    {
        CellData = newCellData; 

        // use filename to assign image .dds as texture from local system folder
        string projectRoot = ViRMA_APIController.imagesDirectory;
        string imageNameDDS = CellData.ImageName.Substring(0, CellData.ImageName.Length - 4) + ".dds";
        byte[] imageBytes = File.ReadAllBytes(projectRoot + imageNameDDS);
        Texture2D imageTexture = ConvertImage(imageBytes);
        GetComponent<Renderer>().material.mainTexture = imageTexture;
        //node.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, -1));
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

}
