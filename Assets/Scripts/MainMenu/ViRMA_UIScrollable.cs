using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_UIScrollable : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private ScrollRect scrollRect;
    private bool allowScrolling;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        scrollRect = GetComponent<ScrollRect>();       
    }

    private void Start()
    {
        SetMenuColliderSize();
    }

    private void Update()
    {
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

    private void EnableJoystickTouchScrolling()
    {
        if (allowScrolling)
        {
            float joyStickDirection = globals.menuInteraction_Scroll.GetAxis(SteamVR_Input_Sources.Any).y;
            if (joyStickDirection != 0)
            {
                //float multiplier = joyStickDirection * 5f;
                //scrollRect.verticalNormalizedPosition = (scrollRect.verticalNormalizedPosition + multiplier * Time.deltaTime);

                float multiplier = joyStickDirection * 0.05f;
                scrollRect.verticalNormalizedPosition = scrollRect.verticalNormalizedPosition + multiplier;
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
