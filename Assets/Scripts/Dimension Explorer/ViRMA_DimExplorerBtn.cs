using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorerBtn : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private ViRMA_DimExplorerGroup parentDimExGrp;

    public Tag tagData;

    public GameObject background;
    public GameObject textMesh;
    public BoxCollider col;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        col = GetComponent<BoxCollider>();
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
            globals.dimExplorer.submittedBtnForTraversal = gameObject;
        }
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            if (globals.dimExplorer.submittedBtnForTraversal == gameObject)
            {
                globals.dimExplorer.submittedBtnForTraversal = null;
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

    // button state controls
    private void DimExBtnStateContoller()
    {
        if (globals.dimExplorer.submittedBtnForTraversal == gameObject)
        {
            SetFocusedState();
        }
        else if (parentDimExGrp.groupIsHighlighted)
        {
            SetHighlightState();
        }
        else
        {
            SetDefaultState();
        }
    }
    public void SetDefaultState()
    {
        background.GetComponent<Renderer>().material.color = globals.flatDarkBlue;
    }
    public void SetHighlightState()
    {
        background.GetComponent<Renderer>().material.color = globals.flatLightBlue;
    }
    public void SetFocusedState()
    {
        background.GetComponent<Renderer>().material.color = globals.flatGreen;
    }

}
