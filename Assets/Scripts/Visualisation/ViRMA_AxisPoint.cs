using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Valve.VR.InteractionSystem;

public class ViRMA_AxisPoint : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public GameObject axisPointLabelObj;
    public TextMeshPro axisPointLabelText;
    public Rigidbody axisPointRigidbody;
    public bool axisPointFaded;

    public MeshRenderer axisPointRend;
    public MaterialPropertyBlock axisPointRendPropBlock;

    [HideInInspector] public bool x;
    [HideInInspector] public bool y;
    [HideInInspector] public bool z;

    public bool childrenSet;
    public bool labelSet;

    public string axisType; 
    public string axisLabel; 
    public int axisId;
    public string axisPointLabel;
    public int axisPointId;

    public int axisPointChildrenCount;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        axisPointRigidbody = gameObject.AddComponent<Rigidbody>();
        axisPointRigidbody.isKinematic = true;
        axisPointRigidbody.useGravity = false;

        axisPointRend = GetComponent<MeshRenderer>();
        axisPointRendPropBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        axisPointLabelObj = Instantiate(Resources.Load("Prefabs/AxisLabel")) as GameObject;
        axisPointLabelText = axisPointLabelObj.GetComponent<TextMeshPro>();
        axisPointLabelObj.transform.SetParent(transform);
        axisPointLabelObj.transform.localScale = axisPointLabelObj.transform.localScale * 0.5f;
        axisPointLabelObj.transform.localPosition = Vector3.zero;
        axisPointLabelObj.transform.localRotation = Quaternion.identity;

        if (x)
        {
            axisPointLabelObj.name = axisPointLabel + "_" + axisPointId;
            Vector3 xPos = axisPointLabelObj.transform.localPosition;
            xPos.z -= 1;
            axisPointLabelObj.transform.localPosition = xPos;
            axisPointLabelObj.transform.localEulerAngles = new Vector3(90, 0, -90);
        }
        if (y)
        {
            axisPointLabelObj.name = axisPointLabel + "_" + axisPointId;
            Vector3 yPos = axisPointLabelObj.transform.localPosition;
            yPos.x -= 1;
            axisPointLabelObj.transform.localPosition = yPos;
            axisPointLabelObj.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.MidlineRight;
        }
        if (z)
        {
            axisPointLabelObj.name = axisPointLabel + "_" + axisPointId;
            Vector3 zPos = axisPointLabelObj.transform.localPosition;
            zPos.x -= 1;
            axisPointLabelObj.transform.localPosition = zPos;
            axisPointLabelObj.transform.localEulerAngles = new Vector3(90, 0, 0);
            axisPointLabelObj.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.MidlineRight;
        }
    }

    private void Update()
    {
        LoadAxisPointLabelAndCollider();

        AxisPointStateController();

        //MoveAxesToFocusedCell(); // no longer works properly with changes (needs to be edited to use again)
    }

    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            CheckForChildren();

            if (childrenSet && axisPointChildrenCount > 0)
            {
                if (x)
                {
                    axisPointLabelText.text = axisPointLabel + " (" + axisPointChildrenCount + ") <b>↓</b>";
                }
                else
                {
                    axisPointLabelText.text = "<b>↓</b>" + " (" + axisPointChildrenCount + ") " + axisPointLabel;
                }
            }

            globals.vizController.focusedAxisPoint = gameObject;

            globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, true);
        }
    }

    private void OnTriggerStay(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            if (childrenSet && axisPointChildrenCount > 0)
            {
                if (x)
                {
                    axisPointLabelText.text = axisPointLabel + " (" + axisPointChildrenCount + ") <b>↓</b>";
                }
                else
                {
                    axisPointLabelText.text = "<b>↓</b>" + " (" + axisPointChildrenCount + ") " + axisPointLabel;
                }
            }

            globals.vizController.focusedAxisPoint = gameObject;

            globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, true);
        }
    }

    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            axisPointLabelText.text = axisPointLabel;

            if (globals.vizController.focusedAxisPoint == gameObject)
            {
                globals.vizController.focusedAxisPoint = null;

                globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, false);
            }
        }
    }

    private void AxisPointStateController()
    {
        if (globals.vizController.focusedAxisPoint)
        {
            if (globals.vizController.focusedAxisPoint == gameObject)
            {
                transform.localScale = Vector3.one * 0.65f;
                ToggleFade(false);
            }
            else
            {
                transform.localScale = Vector3.one * 0.5f;
                ToggleFade(true);
            }
        }
        else
        {
            transform.localScale = Vector3.one * 0.5f;
            ToggleFade(false);
        }
    }
    private void CheckForChildren()
    {
        if (childrenSet == false)
        {
            if (axisType == "node")
            {
                StartCoroutine(ViRMA_APIController.GetHierarchyChildren(axisPointId, (response) => {
                    List<Tag> children = response;
                    axisPointChildrenCount = children.Count;
                    childrenSet = true;
                }));
            }
        }    
    }
    public void ToggleFade(bool toFade)
    {
        float alpha = 1;
        if (toFade)
        {
            if (!axisPointFaded)
            {
                int fadeChecker = 0;
                if (globals.vizController.focusedAxisPoint.GetComponent<ViRMA_AxisPoint>())
                {
                    if (x && globals.vizController.focusedAxisPoint.GetComponent<ViRMA_AxisPoint>().x)
                    {
                        fadeChecker++;
                    }
                    else if (y && globals.vizController.focusedAxisPoint.GetComponent<ViRMA_AxisPoint>().y)
                    {
                        fadeChecker++;
                    }
                    else if (z && globals.vizController.focusedAxisPoint.GetComponent<ViRMA_AxisPoint>().z)
                    {
                        fadeChecker++;
                    }
                    if (fadeChecker > 0)
                    {
                        alpha = 0.35f;
                        axisPointLabelText.color = new Color(axisPointLabelText.color.r, axisPointLabelText.color.g, axisPointLabelText.color.b, alpha);
                        axisPointFaded = true;
                    }
                }          
            }
        }
        else
        {
            if (axisPointFaded)
            {
                alpha = 1.0f;
                axisPointLabelText.color = new Color(axisPointLabelText.color.r, axisPointLabelText.color.g, axisPointLabelText.color.b, alpha);
                axisPointFaded = false;
            }
        }
    }
    private void LoadAxisPointLabelAndCollider()
    {
        // set axis label text when it is ready and surround it in a collider
        if (axisLabel != "" && labelSet == false)
        {
            axisPointLabelText.text = axisPointLabel;

            float offsetSize = (axisPointLabelText.preferredWidth * 0.5f) + 2;
            float offsetPos = ((offsetSize / 2) * -1) + 1;
            BoxCollider axisPointCol = GetComponent<BoxCollider>();

            if (x)
            {
                axisPointCol.center = new Vector3(0, 0, offsetPos);
                axisPointCol.size = new Vector3(2.5f, 1, offsetSize);
            }
            else if (y)
            {
                axisPointCol.center = new Vector3(offsetPos, 0, 0);
                axisPointCol.size = new Vector3(offsetSize, 2.5f, 1);
            }
            else if (z)
            {
                axisPointCol.center = new Vector3(offsetPos, 0, 0);
                axisPointCol.size = new Vector3(offsetSize, 1, 2.5f);
            }
            labelSet = true;
        }
    }
    private void MoveAxesToFocusedCell()
    {
        //transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
        //globals.vizController.gameObject.GetComponent<Rigidbody>().centerOfMass = Vector3.zero;

        if (globals.vizController.focusedCell)
        {
            GameObject targetCell = globals.vizController.focusedCell;
            Vector3 targetPosition = targetCell.transform.localPosition;
            if (x)
            {
                targetPosition.x = transform.localPosition.x;
            }
            if (y)
            {
                targetPosition.y = transform.localPosition.y;
            }
            if (z)
            {
                targetPosition.z = transform.localPosition.z;
            }
            transform.localPosition = targetPosition;
        }
    }

}
