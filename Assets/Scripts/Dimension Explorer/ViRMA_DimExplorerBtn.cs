using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ViRMA_DimExplorerBtn : MonoBehaviour
{
    public Tag tagData;
    public GameObject background;
    public GameObject textMesh;

    private void Awake()
    {
        textMesh.GetComponent<TextMeshPro>().text = "Hey this is a really long sentence that I have here! ";     
    }

    private void Start()
    {
        textMesh.GetComponent<TextMeshPro>().ForceMeshUpdate();

        float textWidth = textMesh.GetComponent<TextMeshPro>().textBounds.size.x * 0.0105f;
        float textHeight = textMesh.GetComponent<TextMeshPro>().textBounds.size.y * 0.02f;

        Vector3 adjustScale = background.transform.localScale;
        adjustScale.x = textWidth;
        adjustScale.y = textHeight;
        background.transform.localScale = adjustScale;

    }

}
