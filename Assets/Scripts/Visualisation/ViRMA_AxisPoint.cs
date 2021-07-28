using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Valve.VR.InteractionSystem;

public class ViRMA_AxisPoint : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    GameObject axisLabel;
    TextMeshPro axisLabelText;

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
            axisLabel.name = "AxisXPointLabel";
            Vector3 xPos = axisLabel.transform.localPosition;
            xPos.z -= 1;
            axisLabel.transform.localPosition = xPos;
            axisLabel.transform.localEulerAngles = new Vector3(90, 0, -90);
        }
        if (y)
        {
            axisLabel.name = "AxisYPointLabel";         
            Vector3 yPos = axisLabel.transform.localPosition;
            yPos.x -= 1;
            axisLabel.transform.localPosition = yPos;
            axisLabel.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.MidlineRight;
        }
        if (z)
        {
            axisLabel.name = "AxisZPointLabel";
            Vector3 zPos = axisLabel.transform.localPosition;
            zPos.x -= 1;
            axisLabel.transform.localPosition = zPos;
            axisLabel.transform.localEulerAngles = new Vector3(90, 0, 0);
            axisLabel.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.MidlineRight;
        }

    }

    private void Update()
    {
        // set axis label text when it is ready
        if (axisLabelText.text != axisPointLabel)
        {
            axisLabelText.text = axisPointLabel;
        }

        //MoveAxesToFocusedCell(); // no longer works properly with changes (needs to be edited to use again)
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
