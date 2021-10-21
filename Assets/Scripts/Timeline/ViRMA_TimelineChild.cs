using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_TimelineChild : MonoBehaviour
{
    // globals
    private ViRMA_GlobalsAndActions globals;
    private Renderer childRend;
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
        childRend = GetComponent<Renderer>();
    }

    // triggers for UI drumsticks
    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.timeline.hoveredChild = gameObject;
        }
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            if (globals.dimExplorer.tagBtnHoveredByUser == gameObject)
            {
                globals.timeline.hoveredChild = null;
            }
        }
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
            childRend.material = timelineChildMaterial;
            childRend.material.SetTextureScale("_MainTex", new Vector2(-1, 1));
        }
        catch (FileNotFoundException e)
        {
            Debug.LogError(e.Message);
        }
    }
    public void LoadContextMenu()
    {
        Debug.Log(gameObject.name);


    }

}
