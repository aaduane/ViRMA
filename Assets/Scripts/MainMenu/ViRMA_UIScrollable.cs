using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_UIScrollable : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private ScrollRect scrollRect;  
    private Transform scrollContent;
    private RectTransform rectTransform;
    private Rect rect;
    private bool allowScrolling;
    private Canvas canvas;

    private BoxCollider[] scrollingCols;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        scrollRect = GetComponent<ScrollRect>();
        scrollContent = scrollRect.content.transform;
        rectTransform = GetComponent<RectTransform>();
        rect = rectTransform.rect;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        SetMenuColliderSize();
    }

    private void Update()
    {
        ScrollableColliderController();

        EnableJoystickTouchScrolling();
    }

    private void OnTriggerStay(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            allowScrolling = true;
        }
    }

    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            allowScrolling = false;
        }
    }

    private void ScrollableColliderController()
    {
        for (int i = 0; i < scrollContent.childCount; i++)
        {
            Rect elementRect = scrollContent.GetChild(i).GetComponent<RectTransform>().rect;

            float halfContainerHeight = (rect.height * canvas.transform.localScale.x) / 2;
            float halfButtonHeight = (elementRect.height * canvas.transform.localScale.x * scrollContent.GetChild(i).localScale.x) / 2;
            float maxDist = (halfContainerHeight + halfButtonHeight);

            float distance = Vector3.Distance(transform.position, scrollContent.GetChild(i).position);
            if (distance > maxDist)
            {
                scrollContent.GetChild(i).GetComponent<BoxCollider>().enabled = false;
            }
            else
            {
                scrollContent.GetChild(i).GetComponent<BoxCollider>().enabled = true;
            }

            if (name == "LocationWrapper")
            {
                Transform child = scrollContent.GetChild(i);
                Transform grandParent = child.transform.parent.parent;

                Vector3 positionRelativeToCenter = grandParent.InverseTransformPoint(child.position);
                float maxScrollingDist = grandParent.GetComponent<RectTransform>().sizeDelta.y / 2;
                float yDist = positionRelativeToCenter.y;

                if (yDist > maxScrollingDist || yDist < (maxScrollingDist * -1))
                {
                    scrollContent.GetChild(i).GetComponent<BoxCollider>().enabled = false;
                }
                else
                {
                    scrollContent.GetChild(i).GetComponent<BoxCollider>().enabled = true;
                }
            }
            
        }
    }
    private void EnableJoystickTouchScrolling()
    {
        if (allowScrolling)
        {
            float joyStickDirection = globals.menuInteraction_Scroll.GetAxis(SteamVR_Input_Sources.Any).y;
            if (joyStickDirection != 0)
            {
                float multiplier = joyStickDirection * 0.45f;
                scrollContent.position -= multiplier * Time.deltaTime * transform.up;
            }
        }
    }
    private void SetMenuColliderSize()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        BoxCollider menuCol = GetComponent<BoxCollider>();
        menuCol.size = new Vector3(width, height, 25);
    }

  
}
