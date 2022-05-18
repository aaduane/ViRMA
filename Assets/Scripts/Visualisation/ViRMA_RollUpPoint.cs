using System.Collections;
using TMPro;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_RollUpPoint : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Rigidbody axisRollUpRigidbody;
    public GameObject axisLabelObj;
    public TextMeshPro axisLabelText;

    [HideInInspector] public bool x;
    [HideInInspector] public bool y;
    [HideInInspector] public bool z;

    public bool parentSet;
    public bool labelSet;

    public int axisId;
    public string axisLabel;
    public string axisType;

    public int parentAxisId;
    public string parentAxisLabel;
    public int parentChildrenCount;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        gameObject.AddComponent<BoxCollider>();
        Destroy(GetComponent<SphereCollider>());

        axisRollUpRigidbody = gameObject.AddComponent<Rigidbody>();
        axisRollUpRigidbody.isKinematic = true;
        axisRollUpRigidbody.useGravity = false;
    }

    private void Start()
    {
        axisLabelObj = Instantiate(Resources.Load("Prefabs/AxisLabel")) as GameObject;
        axisLabelText = axisLabelObj.GetComponent<TextMeshPro>();
        axisLabelObj.transform.SetParent(transform);
        axisLabelObj.transform.localScale = axisLabelObj.transform.localScale * 0.7f;
        axisLabelObj.transform.localPosition = Vector3.zero;
        axisLabelObj.transform.localRotation = Quaternion.identity;

        if (x)
        {
            axisLabelText.color = ViRMA_Colors.axisDarkRed;
            axisLabelObj.name = axisLabel + "_" + axisId;
            Vector3 xPos = axisLabelObj.transform.localPosition;
            xPos.z -= 1;
            axisLabelObj.transform.localPosition = xPos;
            axisLabelObj.transform.localEulerAngles = new Vector3(90, 0, -90);
        }
        if (y)
        {
            axisLabelText.color = ViRMA_Colors.axisDarkGreen;
            axisLabelObj.name = axisLabel + "_" + axisId;
            Vector3 yPos = axisLabelObj.transform.localPosition;
            yPos.x -= 1;
            axisLabelObj.transform.localPosition = yPos;
            axisLabelObj.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.MidlineRight;
        }
        if (z)
        {
            axisLabelText.color = ViRMA_Colors.axisDarkBlue;
            axisLabelObj.name = axisLabel + "_" + axisId;
            Vector3 zPos = axisLabelObj.transform.localPosition;
            zPos.x -= 1;
            axisLabelObj.transform.localPosition = zPos;
            axisLabelObj.transform.localEulerAngles = new Vector3(90, 0, 0);
            axisLabelObj.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.MidlineRight;
        }
    }

    private void Update()
    {
        LoadRollUpLabelAndCollider();
    }

    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            StartCoroutine(CheckForParentThenChildren());

            if (parentSet)
            {
                if (x)
                {
                    axisLabelText.text = parentAxisLabel + " (" + parentChildrenCount + ") <b>↑</b>";
                }
                else
                {
                    axisLabelText.text = "<b>↑</b> " + " (" + parentChildrenCount + ") " + parentAxisLabel;
                }
            }       

            transform.localScale = Vector3.one * 1.1f;

            globals.vizController.focusedAxisPoint = gameObject;

            globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, true);
        }
    }

    private void OnTriggerStay(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            if (parentSet)
            {
                if (x)
                {
                    axisLabelText.text = parentAxisLabel + " (" + parentChildrenCount + ") <b>↑</b>";
                }
                else
                {
                    axisLabelText.text = "<b>↑</b> " + " (" + parentChildrenCount + ") " + parentAxisLabel;
                }
            }

            transform.localScale = Vector3.one * 1.1f;

            globals.vizController.focusedAxisPoint = gameObject;

            globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, true);
        }
    }

    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            axisLabelText.text = axisLabel;

            transform.localScale = Vector3.one;

            globals.vizController.focusedAxisPoint = null;

            globals.ToggleControllerFade(triggeredCol.GetComponent<ViRMA_Drumstick>().hand, false);
        }
    }

    private void LoadRollUpLabelAndCollider()
    {
        // set axis label text when it is ready and surround it in a collider
        if (axisLabel != "" && labelSet == false)
        {
            axisLabelText.text = axisLabel;

            float offsetSize = (axisLabelText.preferredWidth * 0.35f) + 1.5f;
            float offsetPos = ((offsetSize / 2) * -1) + 0.75f;

            BoxCollider axisPointCol = GetComponent<BoxCollider>();
            if (x)
            {
                transform.localRotation = Quaternion.Euler(0, -45, 0);
                axisPointCol.center = new Vector3(0, 0, offsetPos);
                axisPointCol.size = new Vector3(2.5f, 1, offsetSize);
            }
            else if (y)
            {
                transform.localRotation = Quaternion.Euler(0, 0, -45);
                axisPointCol.center = new Vector3(offsetPos, 0, 0);
                axisPointCol.size = new Vector3(offsetSize, 2.5f, 1);
            }
            else if (z)
            {
                transform.localRotation = Quaternion.Euler(0, 45, 0);
                axisPointCol.center = new Vector3(offsetPos, 0, 0);
                axisPointCol.size = new Vector3(offsetSize, 1, 2.5f);
            }
            labelSet = true;
        }
    }
    private IEnumerator CheckForParentThenChildren()
    {
        if (parentSet == false)
        {
            if (axisType == "node")
            {
                Tag parent = new Tag();

                yield return StartCoroutine(ViRMA_APIController.GetHierarchyParent(axisId, (response) => {            
                    if (response != null)
                    {
                        parent = response;
                        parentAxisId = parent.Id;
                        parentAxisLabel = parent.Label;
                        //Debug.Log("Parent: " + parent.Label);
                    }
                    else
                    {
                        //Debug.Log("No parent!");
                    }
                }));

                StartCoroutine(ViRMA_APIController.GetHierarchyChildren(parent.Id, (response) => {
                    parent.Children = response;
                    parentChildrenCount = parent.Children.Count;
                    parentSet = true;
                }));

            }         
        }    
    }
}
