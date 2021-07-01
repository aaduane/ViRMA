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

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
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
    }

    public void submitTagForTraversal()
    {
        globals.dimExplorer.submittedTagForTraversal = tagData;
    }

}
