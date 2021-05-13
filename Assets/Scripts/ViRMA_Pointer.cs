using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR.InteractionSystem;

public class ViRMA_Pointer : MonoBehaviour
{
    public ViRMA_InputModule inputModule;
    public float defaultLength = 5.0f;
    public GameObject dot;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        inputModule.pointerUIEnabled = true;
        inputModule.contactUIEnabled = false;

        transform.localPosition = Vector3.zero;
    }

    private void Update()
    {
        updateLine();
    }

    private void updateLine()
    {
        PointerEventData data = inputModule.GetData();

        if (data.pointerCurrentRaycast.isValid)
        {
            lineRenderer.enabled = true;
            dot.SetActive(true);

            float targetLength = data.pointerCurrentRaycast.distance == 0 ? defaultLength : data.pointerCurrentRaycast.distance;

            RaycastHit hit = createRaycast(targetLength);

            Vector3 endPosition = transform.position + (transform.forward * targetLength);

            if (hit.collider != null)
            {
                endPosition = hit.point;
            }

            dot.transform.position = endPosition;

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, endPosition);    
        }
        else
        {
            lineRenderer.enabled = false;
            dot.SetActive(false);
        }
    }

    private RaycastHit createRaycast(float length)
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        Physics.Raycast(ray, out hit, defaultLength);
        return hit;
    }
}
