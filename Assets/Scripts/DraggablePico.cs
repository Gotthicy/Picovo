using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class DraggablePico : MonoBehaviour
{
    private Camera mainCam;
    private Rigidbody rb;
    private bool isDragging = false;
    private Vector3 offset;
    private float distanceToCam;

    void Start()
    {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Update()
    {
        // 鼠标按下开始拖拽
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider == GetComponent<Collider>())
                {
                    isDragging = true;
                    distanceToCam = Vector3.Distance(mainCam.transform.position, transform.position);
                    Vector3 mouseWorld = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCam));
                    offset = transform.position - mouseWorld;
                }
            }
        }

        // 鼠标释放停止拖拽
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // 拖拽
        if (isDragging)
        {
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCam));
            Vector3 targetPos = mouseWorld + offset;
            rb.MovePosition(targetPos);

            // 检查是否与平面重合
            if (ARPlacementManager.Instance != null && ARPlacementManager.Instance.IsOverPlane(targetPos))
            {
                ARPlacementManager.Instance.PlacePico2D(gameObject);
                isDragging = false;
            }
        }
    }
}
