using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Draggable2D : MonoBehaviour
{
    private Camera cam;
    private bool isDragging = false;
    private float distanceToCam;
    private Vector3 offset;
    private ARPlane plane;

    public void Init(ARPlane targetPlane)
    {
        plane = targetPlane;
        cam = Camera.main;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == GetComponent<Collider>())
            {
                isDragging = true;
                distanceToCam = Vector3.Distance(cam.transform.position, transform.position);
                Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCam));
                offset = transform.position - mouseWorld;
            }
        }

        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        if (isDragging && plane != null)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCam));
            Vector3 target = mouseWorld + offset;

            // 限制只能在 plane polygon 内
            if (ARPlacementManager.Instance.IsInsidePlanePolygon(target, plane))
            {
                transform.position = target;
            }
        }
    }
}
