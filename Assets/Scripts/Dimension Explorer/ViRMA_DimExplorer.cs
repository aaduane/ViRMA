using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorer : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;

    private Rigidbody horizontalRigidbody;
    public List<Rigidbody> verticalRigidbodies;
    public Rigidbody activeVerticalRigidbody;
    
    public Bounds dimExBounds;
    private Vector3 maxRight;
    private Vector3 maxLeft;
    private float distToMaxRight;
    private float distToMaxLeft;

    public bool dimensionExpLorerLoaded;

    public GameObject tagBtnHoveredByUser;
    public GameObject filterBtnHoveredByUser;

    public ViRMA_Keyboard dimExKeyboard;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        horizontalRigidbody = GetComponent<Rigidbody>();

        dimensionExpLorerLoaded = false;
    }

    private void Update()
    {
        if (dimensionExpLorerLoaded)
        {
            DimExMovementLimiter();
            DimExplorerMovement();
        }    
    }

    // general
    public IEnumerator ClearDimExplorer()
    {
        transform.position = new Vector3(0, 9999, 0);
        transform.rotation = Quaternion.identity;
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        yield return new WaitForEndOfFrame();
    }
    public IEnumerator LoadDimExplorer(List<Tag> nodes)
    {
        dimensionExpLorerLoaded = false;

        // clear any current children (must be in coroutine to ensure children are destroyed first
        yield return StartCoroutine(ClearDimExplorer());

        // create dimension explorer button groupings
        float dimExGrpPos = 0;
        float dimExGrpSpacing = 0.2f;
        foreach (var node in nodes)
        {
            // create gameobject group for parent
            GameObject dimExpGrpParent = new GameObject("DimExpGrpParent");
            dimExpGrpParent.transform.parent = transform;
            dimExpGrpParent.transform.localPosition = new Vector3(dimExGrpPos, 0, 0);
            dimExpGrpParent.transform.localRotation = Quaternion.identity;
            dimExGrpPos += dimExGrpSpacing;

            // create gameobject group for siblings
            GameObject dimExpSiblings = new GameObject("DimExpGrpSiblings");
            dimExpSiblings.transform.parent = transform;
            dimExpSiblings.transform.localPosition = new Vector3(dimExGrpPos, 0, 0);
            dimExpSiblings.transform.localRotation = Quaternion.identity;
            dimExGrpPos += dimExGrpSpacing;

            // create gameobject group for children
            GameObject dimExpChildren = new GameObject("DimExpGrpChildren");
            dimExpChildren.transform.parent = transform;
            dimExpChildren.transform.localRotation = Quaternion.identity;
            dimExpChildren.transform.localPosition = new Vector3(dimExGrpPos, 0, 0);
            dimExGrpPos += dimExGrpSpacing;

            // assign tag's parent info
            ViRMA_DimExplorerGroup dimExpGrpParentGrp = dimExpGrpParent.AddComponent<ViRMA_DimExplorerGroup>();
            dimExpGrpParentGrp.tagsInGroup = new List<Tag>() { node.Parent };
            dimExpGrpParentGrp.parentDimExGrp = dimExpGrpParent;
            dimExpGrpParentGrp.siblingsDimExGrp = dimExpSiblings;
            dimExpGrpParentGrp.childrenDimExGrp = dimExpChildren;

            // assign tag and siblings info
            ViRMA_DimExplorerGroup dimExpSiblingsGrp = dimExpSiblings.AddComponent<ViRMA_DimExplorerGroup>();
            dimExpSiblingsGrp.tagsInGroup = node.Siblings;
            dimExpSiblingsGrp.searchedForTagData = node;
            dimExpSiblingsGrp.parentDimExGrp = dimExpGrpParent;
            dimExpSiblingsGrp.siblingsDimExGrp = dimExpSiblings;
            dimExpSiblingsGrp.childrenDimExGrp = dimExpChildren;

            // assign tag's children info
            ViRMA_DimExplorerGroup dimExpChildrenGrp = dimExpChildren.AddComponent<ViRMA_DimExplorerGroup>();
            dimExpChildrenGrp.tagsInGroup = node.Children;
            dimExpChildrenGrp.parentDimExGrp = dimExpGrpParent;
            dimExpChildrenGrp.siblingsDimExGrp = dimExpSiblings;
            dimExpChildrenGrp.childrenDimExGrp = dimExpChildren;

            dimExGrpPos += 1;
        }

        verticalRigidbodies.Clear();
        foreach (Transform dimExGrp in transform)
        {
            verticalRigidbodies.Add(dimExGrp.GetComponent<Rigidbody>());
        }

        // wait a second for DimExGroups to finish loading so AABB is calculated correctly
        yield return new WaitForSeconds(1);

        CalculateBounds();
        PositionDimExplorer();

        dimensionExpLorerLoaded = true;

        globals.dimExplorerActions.Activate();
    }
    public void DimExgroupSPacingAdjustment()
    {
        ViRMA_DimExplorerGroup[] dimExGrps = GetComponentsInChildren<ViRMA_DimExplorerGroup>();
        foreach (ViRMA_DimExplorerGroup dimExGrp in dimExGrps)
        {
            if (dimExGrp.siblingsDimExGrp == dimExGrp.gameObject)
            {
                float dist = Vector3.Distance(dimExGrp.siblingsDimExGrp.transform.localPosition, dimExGrp.parentDimExGrp.transform.localPosition);

                float collidersSpace = (dimExGrp.dimExCollider.size.x / 2) + (dimExGrp.parentDimExGrp.GetComponent<ViRMA_DimExplorerGroup>().dimExCollider.size.x / 2);
            
                if (dist < collidersSpace)
                {
                    // dimExSibling is too close to dimExParent ! ! ! 
                }
            }
        }
    }
    public void CalculateBounds()
    {
        Renderer[] meshes = GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer mesh in meshes)
        {
            bounds.Encapsulate(mesh.bounds);
        }
        dimExBounds = bounds;
    }
    public void PositionDimExplorer()
    {
        // if keyboard is in the scene, position the dim explorer directly behind it
        if (dimExKeyboard.gameObject != null && dimExKeyboard.gameObject.activeSelf)
        {
            Vector3 keyboardPos = dimExKeyboard.gameObject.transform.position;
            float playerHeight = Player.instance.eyeHeight;
            transform.position = new Vector3(keyboardPos.x, playerHeight, keyboardPos.z);
            transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);

            float distanceBehind = 0.25f;
            Vector3 newPosition = transform.position + (transform.forward * distanceBehind);
            transform.position = newPosition;
        }
        else
        {
            // get position directly in front of the player at a specific distance
            Vector3 flattenedVector = Player.instance.bodyDirectionGuess;
            flattenedVector.y = 0;
            flattenedVector.Normalize();
            Vector3 spawnPos = Player.instance.hmdTransform.position + flattenedVector * 0.6f;
            transform.position = spawnPos;
            transform.LookAt(2 * transform.position - Player.instance.hmdTransform.position);
        }

        // calculate max left and right positions of dimension explorer
        float maxDistanceX = dimExBounds.extents.x;
        Vector3 movement = transform.right * maxDistanceX;
        maxRight = transform.position - (movement * 2);
        maxLeft = transform.position;
        //maxRight = transform.position + movement;
        //maxLeft = transform.position - movement;
    }   
    public void SubmitTagForTraversal(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (tagBtnHoveredByUser != null)
        {
            Tag submittedTagData = tagBtnHoveredByUser.GetComponent<ViRMA_DimExplorerBtn>().tagData;
            StartCoroutine(GetTraversedHierarchyNodes(submittedTagData));
        }       
    }
    private IEnumerator GetTraversedHierarchyNodes(Tag submittedTagData)
    {
        dimensionExpLorerLoaded = false;

        // assign parent groupings
        GameObject parentGroupObj = tagBtnHoveredByUser.transform.parent.GetComponent<ViRMA_DimExplorerGroup>().parentDimExGrp;
        ViRMA_DimExplorerGroup parentGroup = parentGroupObj.GetComponent<ViRMA_DimExplorerGroup>();
        Tag parentTagData = new Tag();


        // assign children groupings
        GameObject childrenGroupObj = tagBtnHoveredByUser.transform.parent.GetComponent<ViRMA_DimExplorerGroup>().childrenDimExGrp;
        ViRMA_DimExplorerGroup childrenGroup = childrenGroupObj.GetComponent<ViRMA_DimExplorerGroup>();
        List<Tag> childrenTagData = new List<Tag>();


        // assign siblings groupings
        GameObject siblingsGroupObj = tagBtnHoveredByUser.transform.parent.GetComponent<ViRMA_DimExplorerGroup>().siblingsDimExGrp;
        ViRMA_DimExplorerGroup siblingsGroup = siblingsGroupObj.GetComponent<ViRMA_DimExplorerGroup>();
        List<Tag> siblingsTagData = new List<Tag>();


        // fetch and wait for parent data
        yield return StartCoroutine(ViRMA_APIController.GetHierarchyParent(submittedTagData.Id, (response) => {
            parentTagData = response;
        }));

        // fetch and wait for children data
        yield return StartCoroutine(ViRMA_APIController.GetHierarchyChildren(submittedTagData.Id, (response) => {
            childrenTagData = response;
        }));

        // fetch and wait for sibling data
        yield return StartCoroutine(ViRMA_APIController.GetHierarchyChildren(parentTagData.Id, (response) => {
            siblingsTagData = response;
        }));

        
        // reload parent dim ex grouo
        if (parentTagData.Name == null)
        {
            parentGroup.ClearDimExplorerGroup();
        }
        else
        {
            parentGroup.tagsInGroup = new List<Tag>() { parentTagData };
            StartCoroutine(parentGroup.LoadDimExplorerGroup());
            //parentGroup.LoadDimExplorerGroup();
        }

        // reload childen dim ex grouo
        if (childrenTagData.Count < 1)
        {
            childrenGroup.ClearDimExplorerGroup();
        }
        else
        {
            childrenGroup.tagsInGroup = childrenTagData;
            StartCoroutine(childrenGroup.LoadDimExplorerGroup());
            //childrenGroup.LoadDimExplorerGroup();
        }

        // reload sibling dim ex grouo
        if (siblingsTagData.Count < 1)
        {
            siblingsGroup.ClearDimExplorerGroup();
        }
        else
        {
            siblingsGroup.searchedForTagData = submittedTagData;
            siblingsGroup.tagsInGroup = siblingsTagData;
            StartCoroutine(siblingsGroup.LoadDimExplorerGroup());
            //siblingsGroup.LoadDimExplorerGroup();
        }

        dimensionExpLorerLoaded = true;
    }
    public void SubmitTagForContextMenu(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (tagBtnHoveredByUser != null)
        {
            GameObject submuttedTagBtn = tagBtnHoveredByUser;
            tagBtnHoveredByUser = null;

            ToggleDimExFade(true);

            submuttedTagBtn.GetComponent<ViRMA_DimExplorerBtn>().LoadContextMenu();
        }    
    }
    public void SubmitContextBtnForQuery(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (filterBtnHoveredByUser != null)
        {
            // grab data from context menu
            string axisQueryType = filterBtnHoveredByUser.GetComponent<ViRMA_DimExplorerContextMenuBtn>().axisQueryType;
            Tag tagQueryData = filterBtnHoveredByUser.GetComponent<ViRMA_DimExplorerContextMenuBtn>().tagQueryData;

            // push data to query controller
            Query currentQuery = globals.queryController.activeQuery;
            if (axisQueryType == "filter")
            {
                currentQuery.AddFilter(tagQueryData.Id, "Hierarchy");
            }
            else
            {
                currentQuery.SetAxis(axisQueryType, tagQueryData.Id, "Hieratchy");
            }

            // destroy context menu and return dimension explorer to normal state
            ToggleDimExFade(false);
            filterBtnHoveredByUser.transform.parent.transform.parent.GetComponent<ViRMA_DimExplorerBtn>().contextMenuActiveOnBtn = false;
            Destroy(filterBtnHoveredByUser.transform.parent.gameObject);
        }
    }
    public void ToggleDimExFade(bool toFade)
    {
        toFade = !toFade;

        // stop all movement and prevent any further movement
        dimensionExpLorerLoaded = toFade;

        horizontalRigidbody.velocity = Vector3.zero;
        horizontalRigidbody.angularVelocity = Vector3.zero;

        if (activeVerticalRigidbody)
        {
            activeVerticalRigidbody.velocity = Vector3.zero;
            activeVerticalRigidbody.angularVelocity = Vector3.zero;
            activeVerticalRigidbody = null;
        }       
       
        // disable all other dim ex colliders
        BoxCollider[] dimExColliders = GetComponentsInChildren<BoxCollider>();
        foreach (var dimExCollider in dimExColliders)
        {
            dimExCollider.enabled = toFade;
        }
    }

    // fixed update 
    private void DimExMovementLimiter()
    {
        if (Player.instance)
        {
            Vector3 adjustVelocity = horizontalRigidbody.velocity;

            int DimExPosChecker = 0;

            // check if dim explorer is moving horizontally toward it's max right position
            float distToMaxRightTemp = Vector3.Distance(maxRight, transform.position);
            if (distToMaxRightTemp < distToMaxRight)
            {
                distToMaxRight = distToMaxRightTemp;
            }
            else if (distToMaxRightTemp > distToMaxRight)
            {
                distToMaxRight = distToMaxRightTemp;
                DimExPosChecker++;
            }

            // check if dim explorer is moving horizontally toward it's max left position
            float distToMaxLeftTemp = Vector3.Distance(maxLeft, transform.position);
            if (distToMaxLeftTemp < distToMaxLeft)
            {
                distToMaxLeft = distToMaxLeftTemp;
            }
            else if (distToMaxLeftTemp > distToMaxLeft)
            {
                distToMaxLeft = distToMaxLeftTemp;
                DimExPosChecker++;
            }

            // if dim explorer is moving away from both it's max positions, set it's velocity to zero
            if (DimExPosChecker > 1)
            {
                adjustVelocity = Vector3.zero;
            }

            horizontalRigidbody.velocity = adjustVelocity;
        }
    }
    private void DimExplorerMovement()
    {
        Hand activeHand = null;
        if (globals.dimExplorer_Scroll.GetState(SteamVR_Input_Sources.RightHand))
        {
            activeHand = Player.instance.rightHand;
        }
        if (globals.dimExplorer_Scroll.GetState(SteamVR_Input_Sources.LeftHand))
        {
            activeHand = Player.instance.leftHand;
        }
        if (globals.dimExplorer_Scroll.GetState(SteamVR_Input_Sources.RightHand) && globals.dimExplorer_Scroll.GetState(SteamVR_Input_Sources.LeftHand))
        {
            activeHand = null;
        }

        if (activeHand)
        {
            if (activeVerticalRigidbody == null)
            {
                // enable horizontal movement
                horizontalRigidbody.velocity = Vector3.zero;
                horizontalRigidbody.angularVelocity = Vector3.zero;

                foreach (var verticalRigidbody in verticalRigidbodies)
                {
                    if (verticalRigidbody != null)
                    {
                        verticalRigidbody.isKinematic = true;
                    }                
                }
                horizontalRigidbody.isKinematic = false;

                Vector3 rightHandVelocity = transform.InverseTransformDirection(activeHand.GetTrackedObjectVelocity());
                rightHandVelocity.y = 0;
                rightHandVelocity.z = 0;
                horizontalRigidbody.velocity = transform.TransformDirection(rightHandVelocity);
            }
            else
            {
                // enable vertical movement
                activeVerticalRigidbody.velocity = Vector3.zero;
                activeVerticalRigidbody.angularVelocity = Vector3.zero;

                horizontalRigidbody.isKinematic = true;
                activeVerticalRigidbody.isKinematic = false;

                Vector3 rightHandVelocity = transform.InverseTransformDirection(activeHand.GetTrackedObjectVelocity());
                rightHandVelocity.x = 0;
                rightHandVelocity.z = 0;
                activeVerticalRigidbody.velocity = transform.TransformDirection(rightHandVelocity);
            }
        }

        // if velocity falls below a certain threshold, just stop all movement completely
        if (horizontalRigidbody.velocity.magnitude < 0.2f)
        {
            horizontalRigidbody.velocity = Vector3.zero;
        }
        else
        {
            dimExKeyboard.FadeKeyboard(true);
        }
        if (activeVerticalRigidbody)
        {
            if (activeVerticalRigidbody.velocity.magnitude < 0.2f)
            {
                activeVerticalRigidbody.velocity = Vector3.zero;
            }
            else
            {
                dimExKeyboard.FadeKeyboard(true);
            }
        }
        
    }


    // editor
    void OnDrawGizmosSelected()
    {
        //CalculateBounds();
        //Gizmos.color = new Color(1, 0, 0, 0.5f);
        //Gizmos.DrawCube(transform.position, new Vector3(dimExBounds.size.x, dimExBounds.size.y, dimExBounds.size.z));
    }
}
