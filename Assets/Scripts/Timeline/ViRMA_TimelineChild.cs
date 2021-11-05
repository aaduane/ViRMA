using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR.InteractionSystem;
using TMPro;

public class ViRMA_TimelineChild : MonoBehaviour
{
    // globals
    private ViRMA_GlobalsAndActions globals;
    private Renderer childRend;
    private GameObject tooltip;

    // target timeline child paramters
    public int id;
    public string fileName;
    public List<string> tags;
    public DateTime timestamp;

    // border stuff
    private GameObject border;
    public bool hasBorder;
    public bool contextMenuActiveOnChild;

    private float initializationTime;
    private bool delayComplete;
    private bool metadataLoaded;

    // prev/next btn stuff
    public bool isNextBtn;
    public bool isPrevBtn;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        childRend = GetComponent<Renderer>();
    }
    private void Start()
    {
        initializationTime = Time.realtimeSinceStartup;
    }
    private void Update()
    {
        if (delayComplete == false)
        {
            float timeSinceInitialization = Time.realtimeSinceStartup - initializationTime;
            if (timeSinceInitialization > 0.1f)
            {
                delayComplete = true;
            }
        }       

        if (tags != null && metadataLoaded == false)
        {
            GetTimestamp();
            metadataLoaded = true;
        }

        if (globals.timeline.targetContextTimelineChild)
        {
            if (globals.timeline.targetContextTimelineChild == gameObject)
            {
                ToggleBorder(true);
            }
        }
    }

    // triggers for UI drumsticks
    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            if (delayComplete)
            {
                globals.timeline.hoveredChild = gameObject;

                if (isPrevBtn || isNextBtn)
                {
                    ToggleBorder(true);             
                }   
                else
                {
                    LoadTImelineContextMenu();
                }
            }
        }
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            if (globals.timeline.hoveredChild == gameObject)
            {
                globals.timeline.hoveredChild = null;

                if (contextMenuActiveOnChild == false)
                {
                    //ToggleBorder(false);
                }          
            }
        }
    }

    public void LoadTimelineChild(int targetId, string targetFilename)
    {
        id = targetId;
        fileName = targetFilename;
        name = id + "_" + fileName;

        GetTimelineChildTexture();

        // get associated metadata for timeline child (for concurrent fetch)
        GetTimelineChildMetadata();
    }
    public void GetTimelineChildTexture()
    {
        if (fileName.Length > 0)
        {
            byte[] imageBytes = new byte[0];
            try
            {
                imageBytes = File.ReadAllBytes(ViRMA_APIController.imagesDirectory + fileName);
                Texture2D imageTexture = ViRMA_APIController.ConvertImageFromDDS(imageBytes);
                Material timelineChildMaterial = new Material(Resources.Load("Materials/UnlitCell") as Material);
                timelineChildMaterial.mainTexture = imageTexture;
                childRend.material = timelineChildMaterial;
                childRend.material.SetTextureScale("_MainTex", new Vector2(-1, 1));
            }
            catch (FileNotFoundException e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
    public void LoadTImelineContextMenu()
    {
        globals.timeline.timelineRb.velocity = Vector3.zero;
        globals.timeline.timelineRb.angularVelocity = Vector3.zero;

        GameObject contextMenu = new GameObject("TimelineContextMenu");
        contextMenu.AddComponent<ViRMA_TimelineContextMenu>();
        contextMenu.GetComponent<ViRMA_TimelineContextMenu>().targetTimelineChild = gameObject;

        contextMenu.transform.parent = transform.parent;
        contextMenu.transform.localPosition = transform.localPosition;
        contextMenu.transform.localRotation = transform.localRotation;

        contextMenu.AddComponent<Rigidbody>().useGravity = false;
        contextMenu.AddComponent<BoxCollider>().isTrigger = true;

        float hitBoxThickness = 0.15f;
        contextMenu.GetComponent<BoxCollider>().size = new Vector3(transform.lossyScale.x, transform.lossyScale.y, hitBoxThickness);
        contextMenu.GetComponent<BoxCollider>().center = new Vector3(0, 0, (hitBoxThickness / 2) * -1);

        contextMenuActiveOnChild = true;
    }
    public void ToggleBorder(bool toToggle)
    {
        if (toToggle)
        {
            if (isNextBtn || isPrevBtn)
            {
                Color currentColor = GetComponent<Renderer>().material.GetColor("_Color");
                GetComponent<Renderer>().material.SetColor("_Color", ViRMA_Colors.BrightenColor(currentColor));
            }
            else
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
                    border.GetComponent<Renderer>().material.color = ViRMA_Colors.axisDarkBlue;
                    hasBorder = true;
                }
            }              
        }
        else
        {
            if (isNextBtn || isPrevBtn)
            {
                Color currentColor = GetComponent<Renderer>().material.GetColor("_Color");
                GetComponent<Renderer>().material.SetColor("_Color", ViRMA_Colors.DarkenColor(currentColor));
            }
            else
            {
                if (border)
                {
                    Destroy(border);
                }
            }
        
            hasBorder = false;
        }
    }
    public void GetTimelineChildMetadata()
    {
        if (id != 0)
        {
            StartCoroutine(ViRMA_APIController.GetTimelineMetadata(id, (metadata) => {
                tags = metadata;
                var testing = String.Join(" | ", tags.ToArray());
                Debug.Log(id + " : " + testing);
            }));
        }
    }
    public void GetTimestamp()
    {
        DateTime date = new DateTime();
        DateTime time = new DateTime();

        for (int i = 0; i < tags.Count; i++)
        {
            string targetTag = tags[i];

            if (DateTime.TryParseExact(targetTag, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime outDate))
            {
                date = outDate;
            }

            if (DateTime.TryParseExact(targetTag, "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime outTime)) 
            {
                time = outTime;
            }
        }

        timestamp = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);

        LoadTooltip();
    }
    private void LoadTooltip()
    {
        tooltip = new GameObject();
        TextMeshPro textMesh = tooltip.AddComponent<TextMeshPro>();
        tooltip.name = timestamp.ToString("HH:mm dd/MM/yyyy");
        textMesh.text = timestamp.ToString("HH:mm dd/MM/yyyy");

        tooltip.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);

        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
        textMesh.fontSize = 0.75f;
        textMesh.outlineWidth = 0.2f;

        tooltip.transform.parent = transform;
        tooltip.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1); // adjust for parent's scale
        tooltip.transform.localPosition = new Vector3(0.29f, -0.45f, -1f);
        tooltip.transform.localRotation = Quaternion.identity;
    }

}
