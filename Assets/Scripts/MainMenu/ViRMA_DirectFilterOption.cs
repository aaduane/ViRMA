using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;
using TMPro;

public class ViRMA_DirectFilterOption : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private BoxCollider col;
    private Rigidbody rigidBody;

    public Tag directFilterData;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        col = gameObject.AddComponent<BoxCollider>();
    }

    private void Start()
    {
        SetColliderSize();

        SetupAppearance();
    }

    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.mainMenu.hoveredDirectFilter = gameObject;
        }
    }
    private void OnTriggerStay(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.mainMenu.hoveredDirectFilter = gameObject;
        }
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            if (globals.mainMenu.hoveredDirectFilter == gameObject)
            {
                globals.mainMenu.hoveredDirectFilter = null;
            }
        }
    }
    private void SetColliderSize()
    {
        GameObject bg = transform.GetChild(0).gameObject;
        float width = bg.GetComponent<RectTransform>().sizeDelta.x;
        float height = bg.GetComponent<RectTransform>().sizeDelta.y;
        col.size = new Vector3(width, height * 0.5f, 1);
    }
    private void SetupAppearance()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "Background")
            {
                child.GetComponent<Image>().color = ViRMA_Colors.lightGrey;
                child.GetComponentInChildren<TMP_Text>().color = Color.black;
            }
            else if (child.name == "X_btn")
            {
                child.GetComponent<Image>().color = ViRMA_Colors.axisRed;
                child.GetComponentInChildren<Text>().color = Color.white;
            }
            else if (child.name == "Y_btn")
            {
                child.GetComponent<Image>().color = ViRMA_Colors.axisGreen;
                child.GetComponentInChildren<Text>().color = Color.white;
            }
            else if (child.name == "Z_btn")
            {
                child.GetComponent<Image>().color = ViRMA_Colors.axisBlue;
                child.GetComponentInChildren<Text>().color = Color.white;
            }
            else if (child.name == "R_btn")
            {
                child.GetComponent<Image>().color = ViRMA_Colors.lightBlack;
            }
        }
    }

}
