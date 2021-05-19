using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Valve.VR.InteractionSystem;

public class ViRMA_AxisPoint : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    GameObject axisLabel;

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
        axisLabel.transform.SetParent(transform);
        axisLabel.transform.localScale = axisLabel.transform.localScale * 0.5f;
        axisLabel.transform.localPosition = Vector3.zero;
        axisLabel.transform.rotation = globals.vizController.cellsandAxesWrapper.transform.rotation;      

        if (x)
        {
            axisLabel.name = "AxisXPointLabel";
            Vector3 xPos = axisLabel.transform.localPosition;
            xPos.y -= 1;
            axisLabel.transform.localPosition = xPos;
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
            zPos.y -= 1;
            axisLabel.transform.localPosition = zPos;
        }
    }

    private void Update()
    {
        axisLabel.GetComponent<TextMeshPro>().text = axisPointLabel;

        transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);

        globals.vizController.gameObject.GetComponent<Rigidbody>().centerOfMass = Vector3.zero;

        if (x)
        {     
            axisLabel.transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position, Vector3.left);
        }
        if (y)
        {
            axisLabel.transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position, Vector3.up);
        }
        if (z)
        {
            axisLabel.transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position, Vector3.left);
        }


        if (globals.vizController.targetCellAxesHover)
        {
            GameObject targetCell = globals.vizController.targetCellAxesHover;
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
