using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_TimelineContextMenu : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    // target timeline child paramters
    public GameObject targetTimelineChild;   

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    private void Update()
    {
        if (globals.timeline.timelineRb.velocity != Vector3.zero)
        {
            DestroyContextMenu();
        }
    }

    private void Start()
    {
        GameObject contextMenuBtnPrefab = Resources.Load("Prefabs/TimelineContextMenuBtn") as GameObject;

        GameObject contextBtn = Instantiate(contextMenuBtnPrefab, transform);
        contextBtn.transform.localPosition = new Vector3(-0.12f, 0, -0.025f);
        contextBtn.transform.localScale = contextBtn.transform.localScale * 0.75f;
        contextBtn.GetComponent<ViRMA_TimeLineContextMenuBtn>().LoadTimelineContextMenuBtn("Context", targetTimelineChild);

        GameObject submitBtn = Instantiate(contextMenuBtnPrefab, transform);
        submitBtn.transform.localPosition = new Vector3(0.12f, 0, -0.025f);
        submitBtn.transform.localScale = submitBtn.transform.localScale * 0.75f;
        submitBtn.GetComponent<ViRMA_TimeLineContextMenuBtn>().LoadTimelineContextMenuBtn("Submit", targetTimelineChild);
    }

    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            DestroyContextMenu();
        }
    }

    private void DestroyContextMenu()
    {
        targetTimelineChild.GetComponent<ViRMA_TimelineChild>().ToggleBorder(false);
        targetTimelineChild.GetComponent<ViRMA_TimelineChild>().contextMenuActiveOnChild = false;
        transform.parent = null;
        Destroy(gameObject);
    }

}
