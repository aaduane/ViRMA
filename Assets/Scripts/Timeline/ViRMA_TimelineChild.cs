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

    // target timeline child paramters
    public int id;
    public string fileName;

    // border stuff
    private GameObject border;
    public bool hasBorder;
    public bool contextMenuActiveOnChild;

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

            ToggleBorder(true);
        }
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            if (globals.timeline.hoveredChild == gameObject)
            {
                globals.timeline.hoveredChild = null;

                ToggleBorder(false);
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
    public void LoadTImelineContextMenu()
    {
        GameObject contextMenu = new GameObject("TimelineContextMenu");
        contextMenu.AddComponent<ViRMA_TimelineContextMenu>();
        contextMenu.GetComponent<ViRMA_TimelineContextMenu>().targetChild = gameObject;
        contextMenu.GetComponent<ViRMA_TimelineContextMenu>().id = id;
        contextMenu.GetComponent<ViRMA_TimelineContextMenu>().fileName = fileName;

        contextMenu.transform.parent = transform.parent;
        contextMenu.transform.localPosition = transform.localPosition;
        contextMenu.transform.localRotation = transform.localRotation;

        contextMenu.AddComponent<Rigidbody>().useGravity = false;
        contextMenu.AddComponent<BoxCollider>().isTrigger = true;

        float hitBoxThickness = 0.15f;
        contextMenu.GetComponent<BoxCollider>().size = new Vector3(transform.lossyScale.x, transform.lossyScale.y, hitBoxThickness);
        contextMenu.GetComponent<BoxCollider>().center = new Vector3(0, 0, (hitBoxThickness / 2) * -1);

        contextMenuActiveOnChild = true;
        ToggleBorder(false);
    }
    public void ToggleBorder(bool toToggle)
    {
        if (toToggle)
        {
            if (hasBorder == false && contextMenuActiveOnChild == false)
            {
                border = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(border.GetComponent<Rigidbody>());
                Destroy(border.GetComponent<BoxCollider>());
                border.name = "TimelineChildBorder";
                border.transform.parent = transform.parent;
                border.transform.localPosition = transform.localPosition;
                border.transform.localRotation = transform.localRotation;
                float borderThickness = transform.localScale.x * 0.1f;
                border.transform.localScale = new Vector3(transform.localScale.x + borderThickness, transform.localScale.y + borderThickness, transform.localScale.z * 0.5f);
                border.GetComponent<Renderer>().material.color = ViRMA_Colors.axisTextBlue;           
                hasBorder = true;
            }      
        }
        else
        {
            if (border)
            {
                Destroy(border);
            }
            hasBorder = false;
        }
    }

}
