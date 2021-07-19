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
    public GameObject innerBackground;
    public TextMeshPro textMesh;
    public BoxCollider col;
    public Renderer bgRend;
    public MaterialPropertyBlock matPropBlock;    

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        matPropBlock = new MaterialPropertyBlock();
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

        textMesh.text = tagData.Name;

        textMesh.ForceMeshUpdate();

        float textWidth = textMesh.textBounds.size.x * 0.011f;
        float textHeight = textMesh.textBounds.size.y * 0.02f;

        Vector3 adjustScale = background.transform.localScale;
        adjustScale.x = textWidth;
        adjustScale.y = textHeight;
        background.transform.localScale = adjustScale;

        col.size = adjustScale;

        bgRend.GetPropertyBlock(matPropBlock);
        if (searchedForTag)
        {
            SetFocusedState();
        }
        else
        {
            SetDefaultState();
        }
        bgRend.SetPropertyBlock(matPropBlock);
    }
    public void LoadContextMenu()
    {
        contextMenuActiveOnBtn = true;      

        GameObject contextMenu = new GameObject("DimExContextMenu");
        contextMenu.AddComponent<ViRMA_DimExplorerContextMenu>().tagData = tagData;

        contextMenu.transform.parent = transform;
        contextMenu.transform.localPosition = Vector3.zero;
        contextMenu.transform.localRotation = Quaternion.identity;

        contextMenu.AddComponent<Rigidbody>().useGravity = false;

        contextMenu.AddComponent<BoxCollider>().isTrigger = true;
        contextMenu.GetComponent<BoxCollider>().size = new Vector3(col.size.x * 3f, col.size.y * 4f, col.size.z * 10f);
        contextMenu.GetComponent<BoxCollider>().center = new Vector3(col.center.x, col.center.y, (col.size.z * 10f / 2f) * -1);     
    }

    // button state controls
    private void DimExBtnStateContoller()
    {
        if (globals.dimExplorerActions.IsActive())
        {
            // controls appearance of button in various states
            bgRend.GetPropertyBlock(matPropBlock);
            if (globals.dimExplorer.tagBtnHoveredByUser == gameObject || searchedForTag || contextMenuActiveOnBtn)
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
            bgRend.SetPropertyBlock(matPropBlock);

            // if collider on button is disabled, fade the buttons, unless it's context menu is active
            if (col.enabled == false && contextMenuActiveOnBtn == false)
            {
                SetFadedState();
            }

            // clear border for focused states
            if (innerBackground)
            {
                Destroy(innerBackground);
                innerBackground = null;
            }
        }  
    }
    public void SetDefaultState()
    {
        matPropBlock.SetColor("_Color", globals.lightBlack);
        textMesh.color = Color.white;
    }
    public void SetHighlightState()
    {
        matPropBlock.SetColor("_Color", globals.BrightenColor(globals.lightBlack));
        textMesh.color = Color.white;
    }
    public void SetFocusedState()
    {
        if (innerBackground == null)
        {
            matPropBlock.SetColor("_Color", globals.lightBlack);
            textMesh.color = globals.lightBlack;

            innerBackground = Instantiate(background, background.transform.parent);

            float borderThickness = innerBackground.transform.localScale.y * 0.1f;
            innerBackground.transform.localScale = new Vector3(innerBackground.transform.localScale.x - borderThickness, innerBackground.transform.localScale.y - borderThickness, innerBackground.transform.localScale.z - borderThickness);
            innerBackground.transform.localPosition = new Vector3(innerBackground.transform.localPosition.x, innerBackground.transform.localPosition.y, innerBackground.transform.localPosition.z - 0.003f);            
        }
    }
    public void SetFadedState()
    {
        Color colourToFade = matPropBlock.GetColor("_Color");
        colourToFade.a = 0.5f;
        matPropBlock.SetColor("_Color", colourToFade);       
    }

}
