using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_TimelineChild : MonoBehaviour
{
    // globals
    private ViRMA_GlobalsAndActions globals;
    private string timelineChildMaterial = "Materials/BasicTransparent";

    // child paramters
    public int id;
    public string fileName;
    public DateTime localTime;
    public DateTime universalTime;
    public List<string> semanticTags;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    public void GetTimelineChildTexture()
    {
        byte[] imageBytes = new byte[0];
        try
        {
            imageBytes = File.ReadAllBytes(ViRMA_APIController.imagesDirectory + fileName);
            Texture2D imageTexture = ViRMA_APIController.ConvertImageFromDDS(imageBytes);

            Material timelineChildMaterial = new Material(Resources.Load("Materials/BasicTransparent") as Material);
            timelineChildMaterial.mainTexture = imageTexture;
            GetComponent<Renderer>().material = timelineChildMaterial;
        }
        catch (FileNotFoundException e)
        {
            Debug.LogError(e.Message);
        }
    }

}
