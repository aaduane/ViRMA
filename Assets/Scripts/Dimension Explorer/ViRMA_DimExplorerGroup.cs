using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorerGroup : MonoBehaviour
{
    public List<Tag> tagsInGroup;

    public Bounds dimExBounds;
    public BoxCollider dimExCollider;
    public Rigidbody dimExRigidbody;

    private void Awake()
    {
        gameObject.AddComponent<BoxCollider>();
        gameObject.AddComponent<Rigidbody>();

        dimExCollider = gameObject.GetComponent<BoxCollider>();

        dimExRigidbody = gameObject.GetComponent<Rigidbody>();
        dimExRigidbody.useGravity = false;
    }

    private void Start()
    {
        if (tagsInGroup != null && tagsInGroup.Count > 0)
        {
            float yPos = 0;
            foreach (var tag in tagsInGroup)
            {
                GameObject dimExBtnPrefab = Resources.Load("Prefabs/DimExBtn") as GameObject;
                //dimExBtnPrefab.GetComponent<ViRMA_DimExplorerBtn>().tagData = tag;
                GameObject dimExBtn = Instantiate(dimExBtnPrefab);

                dimExBtn.GetComponent<ViRMA_DimExplorerBtn>().LoadDimExButton(tag);

                //dimExBtn.name = tag.Name;
                dimExBtn.transform.parent = transform;

                dimExBtn.transform.localRotation = Quaternion.identity;
                dimExBtn.transform.localPosition = new Vector3(0, yPos, 0);

                yPos += 0.1f;
            }
        }

        CalculateBounds();
    }

    private void Update()
    {
        DimExGroupLimiter();
    }

    private void CalculateBounds()
    {
        Renderer[] meshes = GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer mesh in meshes)
        {
            bounds.Encapsulate(mesh.bounds);
        }
        dimExBounds = bounds;
        dimExCollider = GetComponent<BoxCollider>();
        dimExCollider.size = new Vector3(dimExBounds.size.x * 1.2f, dimExBounds.size.y * 1.5f, dimExBounds.size.z * 1.2f);
    }

    private void DimExGroupLimiter()
    {
        if (Player.instance)
        {
            Vector3 adjustVelocity = dimExRigidbody.velocity;

            // y max
            float maxDistanceY = Player.instance.eyeHeight + dimExBounds.extents.y;
            if (transform.position.y >= maxDistanceY && adjustVelocity.y > 0)
            {
                adjustVelocity.y = 0;
                dimExRigidbody.velocity = adjustVelocity;
            }

            // y min
            float minDistanceY = Player.instance.eyeHeight - dimExBounds.extents.y;
            if (transform.position.y <= minDistanceY && adjustVelocity.y < 0)
            {
                adjustVelocity.y = 0;
                dimExRigidbody.velocity = adjustVelocity;
            }
        }
    }

}
