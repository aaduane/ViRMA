using TMPro;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorerContextMenuBtn : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    // assigned inside prefab
    public GameObject textMesh;
    public BoxCollider col;
    public Renderer outerBgRend;
    public Renderer innerBgRend;
    public Color activeColor;

    // query info
    public Tag tagQueryData;
    public string axisQueryType;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    public void LoadContextMenuBtn(string axisType)
    {
        axisQueryType = axisType;

        if (axisType == "filter")
        {
            textMesh.GetComponent<TextMeshPro>().text = "Apply as Filter";
            activeColor = Color.black;          
        }
        else
        {
            textMesh.GetComponent<TextMeshPro>().text = "Project to " + axisType + " Axis";
            if (axisType == "X")
            {
                activeColor = Color.red;
            }
            if (axisType == "Y")
            {
                activeColor = Color.green;
            }
            if (axisType == "Z")
            {
                activeColor = Color.blue;
            }
        }

        textMesh.GetComponent<TextMeshPro>().color = activeColor;
        outerBgRend.material.color = activeColor;
    }

    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            innerBgRend.material.color = new Color(0.75f, 0.75f, 0.75f, 1f);
            textMesh.GetComponent<TextMeshPro>().color = Color.white;

            globals.dimExplorer.filterBtnHoveredByUser = gameObject;
        }
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            innerBgRend.material.color = Color.white;
            textMesh.GetComponent<TextMeshPro>().color = activeColor;

            if (globals.dimExplorer.filterBtnHoveredByUser == gameObject)
            {
                globals.dimExplorer.filterBtnHoveredByUser = null;
            }       
        }
    }

    
}
