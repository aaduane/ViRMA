using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorerBtn : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    public Tag tagData;
    public GameObject background;
    public GameObject textMesh;
    public BoxCollider col;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        col = GetComponent<BoxCollider>();

        SetDefaultState();
    }

    // triggers for UI drumsticks
    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.dimExplorer.submittedTagForTraversal = tagData;

            SetFocusState();
        }
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.dimExplorer.submittedTagForTraversal = null;
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

    public void SubmitTagForTraversal()
    {
        globals.dimExplorer.submittedTagForTraversal = tagData;
    }

    public void SetDefaultState()
    {
        background.GetComponent<Renderer>().material.color = globals.flatDarkBlue;
    }
    public void SetHighlightState()
    {
        background.GetComponent<Renderer>().material.color = globals.flatLightBlue;
    }

}
