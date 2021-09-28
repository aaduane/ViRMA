using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Valve.VR.InteractionSystem;

public class ViRMA_AxisPoint : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public GameObject axisLabel;
    public TextMeshPro axisLabelText;
    public Rigidbody axisPointRigidbody;
    public bool axisPointFaded;

    public MeshRenderer axisPointRend;
    public MaterialPropertyBlock axisPointRendPropBlock;

    [HideInInspector] public bool x;
    [HideInInspector] public bool y;
    [HideInInspector] public bool z;

    public string axisType;
    public string axisName;
    public int axisId;
    public string axisPointLabel;
    public int axisPointLabelId;

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
        axisLabel = Instantiate(Resources.Load("Prefabs/AxisLabel")) as GameObject;
        axisLabelText = axisLabel.GetComponent<TextMeshPro>();
        axisLabel.transform.SetParent(transform);
        axisLabel.transform.localScale = axisLabel.transform.localScale * 0.5f;
        axisLabel.transform.localPosition = Vector3.zero;
        axisLabel.transform.localRotation = Quaternion.identity;

        if (x)
        {
            axisLabel.name = axisPointLabel + "_" + axisPointLabelId;
            Vector3 xPos = axisLabel.transform.localPosition;
            xPos.z -= 1;
            axisLabel.transform.localPosition = xPos;
            axisLabel.transform.localEulerAngles = new Vector3(90, 0, -90);
        }
        if (y)
        {
            axisLabel.name = axisPointLabel + "_" + axisPointLabelId;
            Vector3 yPos = axisLabel.transform.localPosition;
            yPos.x -= 1;
            axisLabel.transform.localPosition = yPos;
            axisLabel.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.MidlineRight;
        }
        if (z)
        {
            axisLabel.name = axisPointLabel + "_" + axisPointLabelId;
            Vector3 zPos = axisLabel.transform.localPosition;
            zPos.x -= 1;
            axisLabel.transform.localPosition = zPos;
            axisLabel.transform.localEulerAngles = new Vector3(90, 0, 0);
            axisLabel.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.MidlineRight;
        }
    }

    private void Update()
    {
        LoadAxisPointLabel();

        AxisPointStateController();

        //MoveAxesToFocusedCell(); // no longer works properly with changes (needs to be edited to use again)
    }

    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.vizController.focusedAxisPoint = gameObject;

            globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, true);
        }
    }

    private void OnTriggerStay(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.vizController.focusedAxisPoint = gameObject;

            globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, true);
        }
    }

    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
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
                        axisLabelText.color = new Color(axisLabelText.color.r, axisLabelText.color.g, axisLabelText.color.b, alpha);
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
                axisLabelText.color = new Color(axisLabelText.color.r, axisLabelText.color.g, axisLabelText.color.b, alpha);
                axisPointFaded = false;
            }
        }
    }
    private void LoadAxisPointLabel()
    {
        // set axis label text when it is ready
        if (axisLabelText.text != axisPointLabel)
        {
            axisLabelText.text = axisPointLabel;

            float offsetSize = (axisLabelText.preferredWidth * 0.5f) + 2;
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
