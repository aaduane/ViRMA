using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorerGroup : MonoBehaviour
{
    List<Tag> tagsInGroup;

    public Bounds dimExBounds;
    public BoxCollider dimExCollider;
    public Rigidbody dimExRigidbody;

    private void Awake()
    {
        dimExRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
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
