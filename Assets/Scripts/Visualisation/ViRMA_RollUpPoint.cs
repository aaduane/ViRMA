using TMPro;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_RollUpPoint : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    public Rigidbody axisRollUpRigidbody;
    public GameObject axisLabelObj;
    public TextMeshPro axisLabelText;
    public int axisId;
    public string axisLabel;

    [HideInInspector] public bool x;
    [HideInInspector] public bool y;
    [HideInInspector] public bool z;

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
            axisLabelObj.name = axisLabel + "_" + axisId;
            Vector3 xPos = axisLabelObj.transform.localPosition;
            xPos.z -= 1;
            axisLabelObj.transform.localPosition = xPos;
            axisLabelObj.transform.localEulerAngles = new Vector3(90, 0, -90);
        }
        if (y)
        {
            axisLabelObj.name = axisLabel + "_" + axisId;
            Vector3 yPos = axisLabelObj.transform.localPosition;
            yPos.x -= 1;
            axisLabelObj.transform.localPosition = yPos;
            axisLabelObj.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.MidlineRight;
        }
        if (z)
        {
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
        // set axis label text when it is ready and surround it in a collider
        if (axisLabelText.text != axisLabel)
        {
            // remove bracket id if it exists
            int bracketIndex = axisLabel.IndexOf("(");
            if (bracketIndex > -1)
            {
                axisLabel = axisLabel.Substring(0, bracketIndex);
            }

            axisLabelText.text = axisLabel;
            axisLabelText.color = ViRMA_Colors.axisTextRed;

            float offsetSize = (axisLabelText.preferredWidth * 0.5f);
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

            transform.localRotation = Quaternion.Euler(0, -45, 0);
        }
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
}
