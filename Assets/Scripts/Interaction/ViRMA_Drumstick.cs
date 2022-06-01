using System.Collections;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_Drumstick : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Hand hand;

    private Renderer drumstickRend;
    private MaterialPropertyBlock drumstickRendPropBlock;
    private Color32 highlight = new Color32(0, 0, 0, 120);
    private Color32 normal = new Color32(0, 0, 0, 200);
    private bool toHiglight;

    private float scale = 0.05f;

    Coroutine highlightTimeout;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        drumstickRend = GetComponent<Renderer>();
        drumstickRendPropBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        if (hand)
        {
            if (hand.gameObject.transform.Find("HoverPoint"))
            {
                GameObject steamVRHoverPoint = hand.gameObject.transform.Find("HoverPoint").gameObject;
                steamVRHoverPoint.transform.position = transform.position;
            }
        }
        //GetComponent<Renderer>().material.renderQueue = 9999;
    }

    private void Update()
    {
        if (toHiglight)
        {
            drumstickRend.GetPropertyBlock(drumstickRendPropBlock);
            drumstickRendPropBlock.SetColor("_Color", highlight);
            drumstickRend.SetPropertyBlock(drumstickRendPropBlock);
            //transform.localScale = Vector3.one * 0.0375f;
        }
        else
        {
            drumstickRend.GetPropertyBlock(drumstickRendPropBlock);
            drumstickRendPropBlock.SetColor("_Color", normal);
            drumstickRend.SetPropertyBlock(drumstickRendPropBlock);
            //transform.localScale = Vector3.one * 0.05f;
        }
    }

    private void OnTriggerEnter(Collider triggeredCol)
    {
        //Debug.Log("TRIGGER ENTER! " + triggeredCol.transform.parent.name);
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        //Debug.Log("TRIGGER EXIT! " + triggeredCol.transform.parent.name);
    }
    private void OnTriggerStay(Collider triggeredCol)
    {
        //Debug.Log("TRIGGER STAYING! " + triggeredCol.transform.parent.name);

        if (triggeredCol.transform.root == globals.dimExplorer.transform || triggeredCol.transform.root == globals.vizController.transform || triggeredCol.transform.root == globals.timeline.transform)
        {
            StartHighlight();
        }    
    }

    private IEnumerator HighlightTimeout()
    {
        yield return 0;
        toHiglight = false;
    }

    public void StartHighlight()
    {
        toHiglight = true;

        if (highlightTimeout == null)
        {
            highlightTimeout = StartCoroutine(HighlightTimeout());
        }
        else
        {
            StopCoroutine(highlightTimeout);
            highlightTimeout = StartCoroutine(HighlightTimeout());
        }
    }

}
