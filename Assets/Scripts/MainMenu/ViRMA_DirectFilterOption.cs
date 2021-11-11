using UnityEngine;
using Valve.VR.InteractionSystem;
using TMPro;

public class ViRMA_DirectFilterOption : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private BoxCollider col;
    private Rigidbody rigidBody;
    public TextMeshProUGUI labelText;

    public Tag directFilterData;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        col = gameObject.AddComponent<BoxCollider>();
        labelText = transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        SetColliderSize();

        SetupDirectFilterOptions();
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
    private void SetupDirectFilterOptions()
    {
        ViRMA_UiElement[] directFilterOptions = GetComponentsInChildren<ViRMA_UiElement>();
        foreach (ViRMA_UiElement child in directFilterOptions)
        {
            child.btn.onClick.AddListener(() => SubmitDirectFilterOption(child.name));

            if (child.name == "X_btn")
            {
                child.GenerateBtnDefaults(ViRMA_Colors.axisRed, Color.white);
            }
            else if (child.name == "Y_btn")
            {
               child.GenerateBtnDefaults(ViRMA_Colors.axisGreen, Color.white);
            }
            else if (child.name == "Z_btn")
            {
                child.GenerateBtnDefaults(ViRMA_Colors.axisBlue, Color.white);
            }
            else if (child.name == "R_btn")
            {
                child.GenerateBtnDefaults(ViRMA_Colors.darkGrey, Color.white);
            }
        }
    }
    private void SubmitDirectFilterOption(string optionType)
    {
        if (directFilterData != null)
        {
            if (optionType == "X_btn")
            {
                // Debug.Log("Project " + directFilterData.Label + " to X axis!");
                if (globals.queryController.activeXAxisId != -1)
                {
                    globals.queryController.buildingQuery.AddFilter(globals.queryController.activeXAxisId, globals.queryController.activeXAxisType);
                }               
                globals.queryController.buildingQuery.SetAxis("X", directFilterData.Id, "node");
            }
            else if (optionType == "Y_btn")
            {
                // Debug.Log("Project " + directFilterData.Label + " to Y axis!");
                if (globals.queryController.activeYAxisId != -1)
                {
                    globals.queryController.buildingQuery.AddFilter(globals.queryController.activeYAxisId, globals.queryController.activeYAxisType);
                }       
                globals.queryController.buildingQuery.SetAxis("Y", directFilterData.Id, "node");
            }
            else if (optionType == "Z_btn")
            {
                // Debug.Log("Project " + directFilterData.Label + " to Z axis!");
                if (globals.queryController.activeZAxisId != -1)
                {
                    globals.queryController.buildingQuery.AddFilter(globals.queryController.activeZAxisId, globals.queryController.activeZAxisType);
                }         
                globals.queryController.buildingQuery.SetAxis("Z", directFilterData.Id, "node");
            }

            globals.queryController.buildingQuery.RemoveFilter(directFilterData.Id, "node");
        }    
    }

}
