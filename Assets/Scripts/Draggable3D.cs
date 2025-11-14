using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Draggable3D : MonoBehaviour
{
    private Camera cam;
    private Vector3 offset;
    private bool isDragging = false;
    private float distanceToCam;
    
    // 添加视觉反馈
    private Material originalMaterial;
    public Material hoverMaterial; // 悬停在平面上时的材质
    private Renderer render;

    void Start()
    {
        cam = Camera.main;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        
        render = GetComponent<Renderer>();
        if (render != null)
            originalMaterial = render.material;
    }

    void Update()
    {
        // 使用Touch而不是Mouse（移动设备）
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                TryStartDrag(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                DragObject(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended && isDragging)
            {
                TryPlaceOnPlane(touch.position);
            }
        }
        // Editor测试用鼠标
        #if UNITY_EDITOR
        else if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            DragObject(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            TryPlaceOnPlane(Input.mousePosition);
        }
        #endif
    }

    void TryStartDrag(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == GetComponent<Collider>())
        {
            isDragging = true;
            distanceToCam = Vector3.Distance(cam.transform.position, transform.position);
            Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distanceToCam));
            offset = transform.position - mouseWorld;
        }
    }

    void DragObject(Vector2 screenPos)
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distanceToCam));
        Vector3 targetPos = mouseWorld + offset;
        transform.position = targetPos;

        // 视觉反馈：检查是否在平面上
        if (ARPlacementManager.Instance != null)
        {
            ARPlane plane = ARPlacementManager.Instance.GetNearestPlane(targetPos);
            if (plane != null && ARPlacementManager.Instance.IsInsidePlanePolygon(targetPos, plane))
            {
                // 在平面内，改变材质
                if (render != null && hoverMaterial != null)
                    render.material = hoverMaterial;
            }
            else
            {
                // 不在平面内，恢复原材质
                if (render != null && originalMaterial != null)
                    render.material = originalMaterial;
            }
        }
    }

    void TryPlaceOnPlane(Vector2 screenPos)
    {
        isDragging = false;
        
        // 恢复原材质
        if (render != null && originalMaterial != null)
            render.material = originalMaterial;

        // 检查是否在AR平面上
        if (ARPlacementManager.Instance != null)
        {
            Vector3 currentPos = transform.position;
            ARPlane plane = ARPlacementManager.Instance.GetNearestPlane(currentPos);
            
            if (plane != null && ARPlacementManager.Instance.IsInsidePlanePolygon(currentPos, plane))
            {
                // 转换成2D Pico
                ARPlacementManager.Instance.Spawn2DPico(currentPos, plane);
                Destroy(gameObject);
                return;
            }
        }
        
        // 如果没有放置成功，可以添加回弹效果或保持在原位
    }

    void OnDestroy()
    {
        // 清理材质
        if (render != null && originalMaterial != null)
            render.material = originalMaterial;
    }
}