using TMPro;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorerBtn : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private ViRMA_DimExplorerGroup parentDimExGrp;
    public bool searchedForTag;
    public bool contextMenuActiveOnBtn;

    public Tag tagData;

    // assigned inside prefab
    public GameObject background;
    public GameObject textMesh;
    public BoxCollider col;
    public Renderer bgRend;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    private void Start()
    {
        parentDimExGrp = transform.parent.GetComponent<ViRMA_DimExplorerGroup>();
    }

    private void Update()
    {
        DimExBtnStateContoller();
    }

    // triggers for UI drumsticks
    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.dimExplorer.tagBtnHoveredByUser = gameObject;
        }
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            if (globals.dimExplorer.tagBtnHoveredByUser == gameObject)
            {
                globals.dimExplorer.tagBtnHoveredByUser = null;
            }          
        }
    }

    public void LoadDimExButton(Tag tag)
    {
        tagData = tag;

        gameObject.name = tagData.Name;

        textMesh.GetComponent<TextMeshPro>().text = tagData.Name;

        textMesh.GetComponent<TextMeshPro>().ForceMeshUpdate();

        float textWidth = textMesh.GetComponent<TextMeshPro>().textBounds.size.x * 0.011f;
        float textHeight = textMesh.GetComponent<TextMeshPro>().textBounds.size.y * 0.02f;

        Vector3 adjustScale = background.transform.localScale;
        adjustScale.x = textWidth;
        adjustScale.y = textHeight;
        background.transform.localScale = adjustScale;

        col.size = adjustScale;
    }
    public void LoadContextMenu()
    {
        contextMenuActiveOnBtn = true;
        GameObject contextMenuBtnPrefab = Resources.Load("Prefabs/ContextMenuBtn") as GameObject;

        GameObject contextMenu = new GameObject("DimExContextMenu");
        contextMenu.AddComponent<ViRMA_DimExplorerContextMenu>();

        contextMenu.transform.parent = transform;
        contextMenu.transform.localPosition = Vector3.zero;
        contextMenu.transform.localRotation = Quaternion.identity;

        contextMenu.AddComponent<Rigidbody>().useGravity = false;

        contextMenu.AddComponent<BoxCollider>().isTrigger = true;
        contextMenu.GetComponent<BoxCollider>().size = new Vector3(col.size.x * 2f, col.size.y * 3f, col.size.z * 10f);
        contextMenu.GetComponent<BoxCollider>().center = new Vector3(col.center.x, col.center.y, (col.size.z * 10f / 2f) * -1);

        GameObject directFilterBtn = Instantiate(contextMenuBtnPrefab, contextMenu.transform);
        directFilterBtn.transform.localPosition = new Vector3(0, -0.05f, -0.025f);
        directFilterBtn.transform.localScale = directFilterBtn.transform.localScale * 0.5f;
        directFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().tagQueryData = tagData;
        directFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().LoadContextMenuBtn("filter");

        GameObject xFilterBtn = Instantiate(contextMenuBtnPrefab, contextMenu.transform);
        xFilterBtn.transform.localPosition = new Vector3(-0.12f, 0.05f, -0.025f);
        xFilterBtn.transform.localScale = xFilterBtn.transform.localScale * 0.5f;
        xFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().tagQueryData = tagData;
        xFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().LoadContextMenuBtn("X");

        GameObject yFilterBtn = Instantiate(contextMenuBtnPrefab, contextMenu.transform);
        yFilterBtn.transform.localPosition = new Vector3(0, 0.05f, -0.025f);
        yFilterBtn.transform.localScale = yFilterBtn.transform.localScale * 0.5f;
        yFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().tagQueryData = tagData;
        yFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().LoadContextMenuBtn("Y");

        GameObject zFilterBtn = Instantiate(contextMenuBtnPrefab, contextMenu.transform);
        zFilterBtn.transform.localPosition = new Vector3(0.12f, 0.05f, -0.025f);
        zFilterBtn.transform.localScale = zFilterBtn.transform.localScale * 0.5f;
        zFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().tagQueryData = tagData;
        zFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().LoadContextMenuBtn("Z");
    }

    // button state controls
    private void DimExBtnStateContoller()
    {
        // controls appearance of button in various states
        if (globals.dimExplorer.tagBtnHoveredByUser == gameObject || searchedForTag)
        {
            SetFocusedState();
        }
        else if (globals.dimExplorer.activeVerticalRigidbody == parentDimExGrp.dimExRigidbody)
        {
            SetHighlightState();
        }
        else
        {
            SetDefaultState();
        }

        // if collider is disabled, fade the buttons, unless it's context menu is active
        if (col.enabled == false && contextMenuActiveOnBtn == false)
        {
            SetFadedState();
        }
    }
    public void SetDefaultState()
    {
        bgRend.material.color = globals.flatDarkBlue;
    }
    public void SetHighlightState()
    {
        bgRend.material.color = globals.flatLightBlue;
    }
    public void SetFocusedState()
    {
        bgRend.material.color = globals.flatGreen;
    }
    public void SetFadedState()
    {
        Color colourToFade = bgRend.material.color;
        colourToFade.a = 0.5f;
        bgRend.material.color = colourToFade;
    }

}
