using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR.InteractionSystem;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class ViRMA_TimelineChild : MonoBehaviour
{
    // globals
    private ViRMA_GlobalsAndActions globals;
    private Renderer childRend;
    private GameObject timelineChildLabel;

    // target timeline child paramters
    public int id;
    public string fileName;

    // timeline child data
    public List<Tag> tagsData;      
    public DateTime timestampUTC;   
    public DateTime timestampLOC;
    private bool metadataLoaded;

    // border stuff
    private GameObject border;
    public bool hasBorder;
    public bool contextMenuActiveOnChild;

    // prev/next btn stuff
    public bool isNextBtn;
    public bool isPrevBtn;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        childRend = GetComponent<Renderer>();
    }
    private void Update()
    {
        // if this is the child source of the timeline, then highlight it with a border
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
            globals.timeline.hoveredChild = gameObject;

            if (isPrevBtn || isNextBtn)
            {
                ToggleBorder(true);
            }
            else
            {
                if (metadataLoaded)
                {
                    LoadTimelineContextMenu();
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
                    ToggleBorder(false);
                }          
            }
        }
    }

    public void LoadTimelineChild(int targetId, string targetFilename)
    {
        id = targetId;
        fileName = targetFilename;
        name = id + "_" + fileName;

        // get the textur
        GetTimelineChildTexture();

        // get associated metadata for timeline child (for async fetch)
        GetTimelineChildMetadata();
    }
    public void GetTimelineChildTexture()
    {
        if (fileName.Length > 0)
        {
            if (ViRMA_APIController.useLocalMedia)
            {
                try
                {
                    byte[] imageBytes = new byte[0];

                    Material timelineChildMaterial = new Material(Resources.Load("Materials/UnlitCell") as Material);

                    string imageNameFormatted;
                    if (ViRMA_APIController.localMediaType == "DDS")
                    {
                        imageNameFormatted = fileName.Substring(0, fileName.Length - 3) + "dds";
                        imageBytes = File.ReadAllBytes(ViRMA_APIController.localMediaDirectory + imageNameFormatted);
                        timelineChildMaterial.mainTexture = ViRMA_APIController.FormatDDSTexture(imageBytes);
                        childRend.material = timelineChildMaterial;
                    }
                    else if (ViRMA_APIController.localMediaType == "JPG")
                    {
                        imageNameFormatted = fileName;
                        imageBytes = File.ReadAllBytes(ViRMA_APIController.localMediaDirectory + imageNameFormatted);
                        timelineChildMaterial.mainTexture = ViRMA_APIController.FormatJPGTexture(imageBytes);
                        childRend.material = timelineChildMaterial;
                        childRend.material.SetTextureScale("_MainTex", new Vector2(1, -1));
                    }
                    else
                    {
                        Debug.LogError(gameObject.name +  " | Invalid media file extension!");
                    }
                }
                catch (FileNotFoundException e)
                {
                    Debug.LogError(e.Message);
                }
            }
            else
            {
                StartCoroutine(DownloadTimelineChildTexture());
            }
        }
    }
    private IEnumerator DownloadTimelineChildTexture()
    {
        UnityWebRequest texture = UnityWebRequestTexture.GetTexture(ViRMA_APIController.remoteMediaDirectory + fileName);
        yield return texture.SendWebRequest();
        if (texture.result == UnityWebRequest.Result.Success)
        {
            Texture2D imageTexture = DownloadHandlerTexture.GetContent(texture);
            Material timelineChildMaterial = new Material(Resources.Load("Materials/UnlitCell") as Material);
            timelineChildMaterial.mainTexture = imageTexture;
            childRend.material = timelineChildMaterial;
            childRend.material.SetTextureScale("_MainTex", new Vector2(-1, -1));
        }
        else
        {
            Debug.LogError(texture.error + " | " + fileName);
        }
    }
    public void LoadTimelineContextMenu()
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
            StartCoroutine(ViRMA_APIController.GetMediaObjectTagData(id, (results) => {
                tagsData = results;
                GenerateTimestamp();
            }));
        }
    }
    public void GenerateTimestamp()
    {  
        foreach (Tag tagData in tagsData)
        {
            if (tagData.Label == "Timestamp LOC")
            {
                if (DateTime.TryParseExact(tagData.Children[0].Label, "dd/MM/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTimestamp))
                {
                    timestampLOC = parsedTimestamp;
                }
            }
            if (tagData.Label == "Timestamp UTC")
            {
                if (DateTime.TryParseExact(tagData.Children[0].Label, "dd/MM/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTimestamp))
                {
                    timestampUTC = parsedTimestamp;
                }
            }
        }
        LoadTimelineChild();
    }
    private void LoadTimelineChild()
    {
        timelineChildLabel = new GameObject();
        TextMeshPro textMesh = timelineChildLabel.AddComponent<TextMeshPro>();
        
        timelineChildLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);

        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textMesh.verticalAlignment = VerticalAlignmentOptions.Bottom;
        textMesh.fontSize = 0.75f;
        textMesh.outlineWidth = 0.2f;

        timelineChildLabel.transform.SetParent(transform);
        timelineChildLabel.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1); // adjust for parent's scale
        timelineChildLabel.transform.localRotation = Quaternion.identity;
        timelineChildLabel.transform.localPosition = new Vector3(0, 0, -1f);

        if (ViRMA_APIController.database == "LSC")
        {
            timelineChildLabel.name = timestampLOC.ToString("ddd HH:mm:ss dd/MM/yyyy");
            textMesh.text = timestampLOC.ToString("ddd HH:mm:ss dd/MM/yyyy");
        }

        if (ViRMA_APIController.database == "VBS")
        {
            int firstSlash = fileName.IndexOf("/");
            string remainingSlash = fileName.Substring(firstSlash + 1);
            int secondSlash = remainingSlash.IndexOf("/");
            string videoId = fileName.Substring(firstSlash + 1, secondSlash);

            int underScore = fileName.IndexOf("_");
            string keyframeCount = fileName.Substring(underScore + 1);
            keyframeCount = keyframeCount.Substring(0, keyframeCount.Length - 4);

            timelineChildLabel.name = videoId;
            textMesh.text = videoId + " | " + keyframeCount;        
        }

        metadataLoaded = true;
    }
    public void LoadTimelineChildTooltip()
    {
        Debug.Log("ID: " + id);

        foreach (Tag tagsetData in tagsData)
        {
            Debug.Log(" * * * * * * * * " + tagsetData.Label + " * * * * * * * * ");
            foreach (Tag tagData in tagsetData.Children)
            {
                Debug.Log(tagData.Label);
            }
        }

    }

}
