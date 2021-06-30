﻿using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ViRMA_DimExplorerGroup : MonoBehaviour
{
    public bool fullyLoaded = false;

    public List<Tag> tagsInGroup;
    public Tag searchedForTag;

    public Bounds dimExBounds;
    public BoxCollider dimExCollider;
    public Rigidbody dimExRigidbody;

    public Vector3 searchForTagStartPos;

    private void Awake()
    {
        gameObject.AddComponent<BoxCollider>();
        gameObject.AddComponent<Rigidbody>();

        gameObject.layer = 9;

        dimExCollider = gameObject.GetComponent<BoxCollider>();

        dimExRigidbody = gameObject.GetComponent<Rigidbody>();
        dimExRigidbody.useGravity = false;  
    }

    private void Start()
    {
        GameObject dimExBtnPrefab = Resources.Load("Prefabs/DimExBtn") as GameObject;
        if (tagsInGroup != null && tagsInGroup.Count > 0)
        {
            if (searchedForTag != null)
            {

                // get index of searched for tag in siblings
                int starterIndex = 0;
                for (int i = 0; i < tagsInGroup.Count; i++)
                {
                    if (searchedForTag.Id == tagsInGroup[i].Id)
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

        CalculateBounds();
        dimExCollider.size = new Vector3(dimExBounds.size.x, dimExBounds.size.y, dimExBounds.size.z);
        dimExCollider.center = new Vector3(0, dimExBounds.center.y - transform.parent.transform.position.y, 0);
        fullyLoaded = true;
    }

    private void Update()
    {
        if (fullyLoaded)
        {
            DimExGroupLimiter();
        }     
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
    }

    private void DimExGroupLimiter()
    {
        if (Player.instance)
        {
            Vector3 adjustVelocity = dimExRigidbody.velocity;

            /*
            // y max
            float maxDistanceY = Player.instance.eyeHeight + dimExBounds.size.y;
            if (transform.position.y >= maxDistanceY && adjustVelocity.y > 0)
            {
                adjustVelocity.y = 0;
                dimExRigidbody.velocity = adjustVelocity;
            }

            // y min
            float minDistanceY = Player.instance.eyeHeight - dimExBounds.size.y;
            if (transform.position.y <= minDistanceY && adjustVelocity.y < 0)
            {
                adjustVelocity.y = 0;
                dimExRigidbody.velocity = adjustVelocity;
            }
            */

            Vector3 topTagPos = transform.GetChild(0).position;
            Vector3 bottomTagPos = transform.GetChild(transform.childCount - 1).position;

            if (topTagPos.y <= Player.instance.eyeHeight && adjustVelocity.y < 0)
            {
                adjustVelocity.y = 0;
                dimExRigidbody.velocity = adjustVelocity;
            }

            if (bottomTagPos.y >= Player.instance.eyeHeight && adjustVelocity.y > 0)
            {
                adjustVelocity.y = 0;
                dimExRigidbody.velocity = adjustVelocity;
            }
        }
    }

}
