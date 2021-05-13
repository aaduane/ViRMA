using UnityEngine;


public class pointer : MonoBehaviour
{
    public vr_inputModule inputModule;

    public float defaultLength = 5.0f;

    public GameObject dot;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        updateLine();
    }

    private void updateLine()
    {
        float targetLength = defaultLength;

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

    private RaycastHit createRaycast(float length)
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        Physics.Raycast(ray, out hit, defaultLength);
        return hit;
    }
}
