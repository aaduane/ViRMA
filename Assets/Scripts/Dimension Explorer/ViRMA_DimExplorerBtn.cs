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

    // already assigned in prefab
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
