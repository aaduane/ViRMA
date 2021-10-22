using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorerContextMenu : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Tag tagData;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    private void Start()
    {
        GameObject contextMenuBtnPrefab = Resources.Load("Prefabs/DimExContextMenuBtn") as GameObject;

        GameObject directFilterBtn = Instantiate(contextMenuBtnPrefab, transform);
        directFilterBtn.transform.localPosition = new Vector3(0, -0.05f, -0.025f);
        directFilterBtn.transform.localScale = directFilterBtn.transform.localScale * 0.5f;
        directFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().tagQueryData = tagData;
        directFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().LoadDimExContextMenuBtn("filter");

        GameObject xFilterBtn = Instantiate(contextMenuBtnPrefab, transform);
        xFilterBtn.transform.localPosition = new Vector3(-0.12f, 0.05f, -0.025f);
        xFilterBtn.transform.localScale = xFilterBtn.transform.localScale * 0.5f;
        xFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().tagQueryData = tagData;
        xFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().LoadDimExContextMenuBtn("X");

        GameObject yFilterBtn = Instantiate(contextMenuBtnPrefab, transform);
        yFilterBtn.transform.localPosition = new Vector3(0, 0.05f, -0.025f);
        yFilterBtn.transform.localScale = yFilterBtn.transform.localScale * 0.5f;
        yFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().tagQueryData = tagData;
        yFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().LoadDimExContextMenuBtn("Y");

        GameObject zFilterBtn = Instantiate(contextMenuBtnPrefab, transform);
        zFilterBtn.transform.localPosition = new Vector3(0.12f, 0.05f, -0.025f);
        zFilterBtn.transform.localScale = zFilterBtn.transform.localScale * 0.5f;
        zFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().tagQueryData = tagData;
        zFilterBtn.GetComponent<ViRMA_DimExplorerContextMenuBtn>().LoadDimExContextMenuBtn("Z");
    }

    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.dimExplorer.ToggleDimExFade(false);

            transform.parent.GetComponent<ViRMA_DimExplorerBtn>().contextMenuActiveOnBtn = false;

            Destroy(gameObject);
        }
    }

}
