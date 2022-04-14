using UnityEngine;
using Valve.VR.InteractionSystem;
using TMPro;

public class ViRMA_DirectFilterOption : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private BoxCollider col;
    private Rigidbody rigidBody;  
    private ViRMA_UiElement[] directFilterOptions;
    private BoxCollider[] optionCols;

    public TextMeshProUGUI labelText;
    public Tag directFilterData;
    public string filterType;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();
        rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        col = gameObject.AddComponent<BoxCollider>();
        labelText = transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        directFilterOptions = GetComponentsInChildren<ViRMA_UiElement>();
        optionCols = GetComponentsInChildren<BoxCollider>();
    }

    private void Start()
    {
        SetColliderSize();

        SetupDirectFilterOptions();
    }

    private void Update()
    {
        // ensure collider is only enabled when parent collider is
        foreach (BoxCollider optionCol in optionCols)
        {
            if (optionCol.transform != transform)
            {
                optionCol.enabled = col.enabled;
            }
        }
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
        foreach (ViRMA_UiElement child in directFilterOptions)
        {
            child.btn.onClick.AddListener(() => SubmitDirectFilterOption(child.name));

            if (child.name == "X_btn")
            {
                if (filterType == "node")
                {
                    child.GenerateBtnDefaults(ViRMA_Colors.axisRed, Color.white);
                }
                else
                {
                    Destroy(child);
                }        
            }
            
            if (child.name == "Y_btn")
            {
                if (filterType == "node")
                {
                    child.GenerateBtnDefaults(ViRMA_Colors.axisGreen, Color.white);
                }
                else
                {
                    Destroy(child);
                }             
            }
            
            if (child.name == "Z_btn")
            {
                if (filterType == "node")
                {
                    child.GenerateBtnDefaults(ViRMA_Colors.axisBlue, Color.white);
                }
                else
                {
                    Destroy(child);
                }      
            }
            
            if (child.name == "R_btn")
            {
                child.GenerateBtnDefaults(ViRMA_Colors.darkGrey, Color.white);
            }

            child.Hide(true);
        }
    }
    private void SubmitDirectFilterOption(string optionType)
    {
        if (directFilterData != null)
        {
            // enable and/or setting for concept tags 
            int orEnabled = -1;
            if (globals.queryController.queryModeOrSetting)
            {
                orEnabled = 0;
            }

            if (optionType == "X_btn")
            {
                if (globals.queryController.activeXAxisId != -1)
                {
                    // if there is something already projected to X, set it as a direct filter
                    globals.queryController.buildingQuery.AddFilter(globals.queryController.activeXAxisId, globals.queryController.activeXAxisType, orEnabled);
                }

                // project target filter to X axis
                globals.queryController.buildingQuery.SetAxis("X", directFilterData.Id, "node");
            }
            else if (optionType == "Y_btn")
            {

                if (globals.queryController.activeYAxisId != -1)
                {
                    // if there is something already projected to Y, set it as a direct filter
                    globals.queryController.buildingQuery.AddFilter(globals.queryController.activeYAxisId, globals.queryController.activeYAxisType, orEnabled);
                }

                // project target filter to Y axis
                globals.queryController.buildingQuery.SetAxis("Y", directFilterData.Id, "node");
            }
            else if (optionType == "Z_btn")
            {

                if (globals.queryController.activeZAxisId != -1)
                {
                    // if there is something already projected to Z, set it as a direct filter
                    globals.queryController.buildingQuery.AddFilter(globals.queryController.activeZAxisId, globals.queryController.activeZAxisType, orEnabled);
                }

                // project target filter to Z axis
                globals.queryController.buildingQuery.SetAxis("Z", directFilterData.Id, "node");
            }

            // remove target filter from the filter list
            if (filterType == "node")
            {
                globals.queryController.buildingQuery.RemoveFilter(directFilterData.Id, "node");
            }
            else if (filterType == "tag")
            {
                globals.queryController.buildingQuery.RemoveFilter(directFilterData.Id, "tag", directFilterData.Parent.Id);
            }
        }    
    }

}
