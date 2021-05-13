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

    public bool positionSet = false;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
    }

    private void Start()
    {
        axisLabel = Instantiate(Resources.Load("Prefabs/AxisLabel")) as GameObject;
        axisLabel.transform.SetParent(transform);
        axisLabel.transform.localPosition = Vector3.zero;
        axisLabel.transform.rotation = Quaternion.identity;
        axisLabel.transform.localScale = Vector3.one * 0.3f;       
    }

    private void Update()
    {
        axisLabel.GetComponent<TextMeshPro>().text = axisPointLabel;

        if (globals.vizController.targetCellAxesHover)
        {
            GameObject targetCell = globals.vizController.targetCellAxesHover;

            Vector3 targetPosition = targetCell.transform.localPosition;
            if (x)
            {
                targetPosition.x = transform.localPosition.x;
                axisLabel.transform.localPosition = new Vector3(0, 2, 0);
            }
            if (y)
            {
                targetPosition.y = transform.localPosition.y;
                axisLabel.transform.localPosition = new Vector3(0, 0, 0);
            }
            if (z)
            {
                targetPosition.z = transform.localPosition.z;
                axisLabel.transform.localPosition = new Vector3(0, -2, 0);
            }
            transform.localPosition = targetPosition;
        }


        if (globals.vizController.targetCellAxesHover)
        {
            if (transform.localPosition == globals.vizController.targetCellAxesHover.transform.localPosition)
            {
                if (x)
                {
                    axisLabel.transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
                    globals.vizController.targetCellAxisLabelRotation = axisLabel.transform.localRotation;
                }
                else
                {
                    axisLabel.transform.localRotation = globals.vizController.targetCellAxisLabelRotation;
                }
            }
            else
            {
                axisLabel.transform.localRotation = globals.vizController.targetCellAxisLabelRotation;
            }
        }        

    }

}
