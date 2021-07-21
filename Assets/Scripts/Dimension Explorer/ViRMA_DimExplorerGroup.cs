using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorerGroup : MonoBehaviour
{
    private ViRMA_GlobalsAndActions globals;
    private GameObject dimExBtnPrefab;

    public bool dimensionExpLorerGroupLoaded;

    public List<Tag> tagsInGroup;
    public Tag searchedForTagData;

    public Bounds dimExBounds;
    public BoxCollider dimExCollider;
    public Rigidbody dimExRigidbody;

    public Vector3 searchForTagStartPos;

    private Transform topMostChild;
    private Transform bottomMostChild;

    public GameObject parentDimExGrp;
    public GameObject siblingsDimExGrp;
    public GameObject childrenDimExGrp;

    private void Awake()
    {
        globals = Player.instance.gameObject.GetComponent<ViRMA_GlobalsAndActions>();

        dimExCollider = gameObject.AddComponent<BoxCollider>();

        dimExRigidbody = gameObject.AddComponent<Rigidbody>();
        dimExRigidbody.isKinematic = true;
        dimExRigidbody.useGravity = false;
        dimExRigidbody.drag = 0.5f;

        gameObject.layer = 9;

        dimensionExpLorerGroupLoaded = false;

        dimExBtnPrefab = Resources.Load("Prefabs/DimExBtn") as GameObject;
    }

    private void Start()
    {
        StartCoroutine(LoadDimExplorerGroup());
    }

    private void Update()
    {
        if (dimensionExpLorerGroupLoaded)
        {
            DimExGroupMovementLimiter();
        }     
    }

    // triggers for UI drumsticks
    private void OnTriggerEnter(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            globals.dimExplorer.activeVerticalRigidbody = dimExRigidbody;
        }
    }
    private void OnTriggerExit(Collider triggeredCol)
    {
        if (triggeredCol.GetComponent<ViRMA_Drumstick>())
        {
            if (globals.dimExplorer.activeVerticalRigidbody == dimExRigidbody)
            {
                globals.dimExplorer.activeVerticalRigidbody = null;
            }         
        }
    }

    public IEnumerator LoadDimExplorerGroup()
    {
        dimensionExpLorerGroupLoaded = false;

        ClearDimExplorerGroup();

        yield return new WaitForEndOfFrame();

        transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        transform.localRotation = Quaternion.identity;
        dimExRigidbody.velocity = Vector3.zero;

        if (tagsInGroup != null && tagsInGroup.Count > 0)
        {
            if (searchedForTagData != null)
            {
                // get index of searched for tag in siblings
                int starterIndex = 0;
                for (int i = 0; i < tagsInGroup.Count; i++)
                {
                    if (searchedForTagData.Id == tagsInGroup[i].Id)
                    {
                        starterIndex = i;
                        break;
                    }
                }

                // generate positions of tags above searched for tag
                int indexPlaceholderPos = starterIndex;
                for (int i = 0; i < starterIndex; i++)
                {
                    GameObject dimExBtn = Instantiate(dimExBtnPrefab);
                    dimExBtn.GetComponent<ViRMA_DimExplorerBtn>().LoadDimExButton(tagsInGroup[i]);
                    dimExBtn.transform.parent = transform;
                    dimExBtn.transform.localRotation = Quaternion.identity;

                    float yPos = indexPlaceholderPos * 0.1f;
                    dimExBtn.transform.localPosition = new Vector3(0, yPos, 0);
                    indexPlaceholderPos--;
                }

                // generate positions of tags below searched for tag
                int indexPlaceholderNeg = 0;
                for (int i = starterIndex; i < tagsInGroup.Count; i++)
                {
                    GameObject dimExBtn = Instantiate(dimExBtnPrefab);
                    if (i == starterIndex)
                    {
                        dimExBtn.GetComponent<ViRMA_DimExplorerBtn>().searchedForTag = true;
                    }
                    dimExBtn.GetComponent<ViRMA_DimExplorerBtn>().LoadDimExButton(tagsInGroup[i]);
                    dimExBtn.transform.parent = transform;
                    dimExBtn.transform.localRotation = Quaternion.identity;

                    

                    float yPos = indexPlaceholderNeg * -0.1f;
                    dimExBtn.transform.localPosition = new Vector3(0, yPos, 0);
                    indexPlaceholderNeg++;
                }
            }
            else
            {
                // generate positions for tags in children (and parent) dim ex groups
                float yIndex = 0;
                foreach (var tag in tagsInGroup)
                {
                    GameObject dimExBtn = Instantiate(dimExBtnPrefab);
                    dimExBtn.GetComponent<ViRMA_DimExplorerBtn>().LoadDimExButton(tag);
                    dimExBtn.transform.parent = transform;
                    dimExBtn.transform.localRotation = Quaternion.identity;

                    float yPos = yIndex * -0.1f;
                    dimExBtn.transform.localPosition = new Vector3(0, yPos, 0);

                    yIndex++;
                }
            }

        }

        // calculate bounds of meshes
        CalculateBounds();

        // create colliders based on bounds
        float colForwardThickness = dimExBounds.size.z * 10f;
        dimExCollider.size = new Vector3(dimExBounds.size.x, dimExBounds.size.y, colForwardThickness);
        dimExCollider.center = new Vector3(dimExBounds.center.x, dimExBounds.center.y, (colForwardThickness / 2) * -1);

        // set topmost and bottommost children for scrolling limits
        if (transform.childCount > 1)
        {
            topMostChild = transform.GetChild(0);
            bottomMostChild = transform.GetChild(transform.childCount - 1);
        }

        // start limiting scrolling
        dimensionExpLorerGroupLoaded = true;
    }
    public void ClearDimExplorerGroup()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void CalculateBounds()
    {
        Vector3 savePosition = transform.position;
        Quaternion saveRotation = transform.rotation;

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        Renderer[] meshes = GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer mesh in meshes)
        {
            bounds.Encapsulate(mesh.bounds);
        }
        dimExBounds = bounds;

        transform.position = savePosition;
        transform.rotation = saveRotation;
    }
    private void DimExGroupMovementLimiter()
    {
        if (Player.instance)
        {
            if (topMostChild != null && bottomMostChild != null)
            {
                Vector3 adjustVelocity = dimExRigidbody.velocity;

                // prevent dim ex group from vertically scrolling too far down
                if (topMostChild.position.y <= Player.instance.eyeHeight && adjustVelocity.y < 0)
                {
                    adjustVelocity.y = 0;
                    dimExRigidbody.velocity = adjustVelocity;
                }

                // prevent dim ex group from vertically scrolling too far up
                if (bottomMostChild.position.y >= Player.instance.eyeHeight && adjustVelocity.y > 0)
                {
                    adjustVelocity.y = 0;
                    dimExRigidbody.velocity = adjustVelocity;
                }
            } 
            else
            {
                dimExRigidbody.velocity = Vector3.zero; 
            }
        }
    }

}
