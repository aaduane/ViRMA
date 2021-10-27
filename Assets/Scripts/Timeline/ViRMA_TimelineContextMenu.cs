using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_TimelineContextMenu : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    // target timeline child paramters
    public GameObject targetChild;
    public int id;
    public string fileName;   

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    private void Start()
    {
        GameObject contextMenuBtnPrefab = Resources.Load("Prefabs/TimelineContextMenuBtn") as GameObject;

        GameObject contextBtn = Instantiate(contextMenuBtnPrefab, transform);
        contextBtn.transform.localPosition = new Vector3(-0.12f, 0, -0.025f);
        contextBtn.transform.localScale = contextBtn.transform.localScale * 0.75f;
        contextBtn.GetComponent<ViRMA_TimeLineContextMenuBtn>().id = id;
        contextBtn.GetComponent<ViRMA_TimeLineContextMenuBtn>().fileName = fileName;
        contextBtn.GetComponent<ViRMA_TimeLineContextMenuBtn>().LoadTimelineContextMenuBtn("Context");

        GameObject submitBtn = Instantiate(contextMenuBtnPrefab, transform);
        submitBtn.transform.localPosition = new Vector3(0.12f, 0, -0.025f);
        submitBtn.transform.localScale = submitBtn.transform.localScale * 0.75f;
        submitBtn.GetComponent<ViRMA_TimeLineContextMenuBtn>().id = id;
        submitBtn.GetComponent<ViRMA_TimeLineContextMenuBtn>().fileName = fileName;
        submitBtn.GetComponent<ViRMA_TimeLineContextMenuBtn>().LoadTimelineContextMenuBtn("Submit");
    }

    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            targetChild.GetComponent<ViRMA_TimelineChild>().ToggleBorder(false);
            targetChild.GetComponent<ViRMA_TimelineChild>().contextMenuActiveOnChild = false;
            Destroy(gameObject);
        }
    }

}
