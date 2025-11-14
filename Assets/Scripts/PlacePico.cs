using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacePico : MonoBehaviour
{
    [Header("Pico 模型预制体")]
    public GameObject picoPrefab;

    [Header("AR 组件（真机用）")]
    public ARRaycastManager arRaycastManager;

    private GameObject spawnedPico;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

#if UNITY_EDITOR
    //------------------------------------------
    // 编辑器模拟参数
    //------------------------------------------
    public float moveSpeed = 2f;
    public float lookSpeed = 2f;
    private float yaw;
    private float pitch;
    private bool isDragging = false;
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // 假设地面是Y=0
    private Vector3 dragOffset;
#endif

    void Update()
    {
#if UNITY_EDITOR
        SimulateInEditor();
#else
        RunOnDevice();
#endif
    }

#if UNITY_EDITOR
    //------------------------------------------
    // 编辑器模式下：鼠标移动视角、点击放置与拖拽
    //------------------------------------------
    void SimulateInEditor()
    {
        HandleCameraMovement();
        HandlePicoPlacementOrDrag();
    }

    void HandleCameraMovement()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * lookSpeed;
            pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
            Camera.main.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }

        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += Camera.main.transform.forward;
        if (Input.GetKey(KeyCode.S)) move -= Camera.main.transform.forward;
        if (Input.GetKey(KeyCode.A)) move -= Camera.main.transform.right;
        if (Input.GetKey(KeyCode.D)) move += Camera.main.transform.right;
        if (Input.GetKey(KeyCode.Q)) move -= Camera.main.transform.up;
        if (Input.GetKey(KeyCode.E)) move += Camera.main.transform.up;
        Camera.main.transform.position += move * moveSpeed * Time.deltaTime;
    }

    void HandlePicoPlacementOrDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 开始拖拽
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == spawnedPico)
                {
                    isDragging = true;
                    // 记录初始点击位置
                    float enter;
                    if (groundPlane.Raycast(ray, out enter))
                    {
                        Vector3 hitPoint = ray.GetPoint(enter);
                        dragOffset = spawnedPico.transform.position - hitPoint;
                    }
                }
                else
                {
                    // 点击地面放置或移动
                    if (spawnedPico == null)
                        spawnedPico = Instantiate(picoPrefab, hit.point, Quaternion.identity);
                    else
                        spawnedPico.transform.position = hit.point;
                }
            }
        }

        // 拖拽中
        if (isDragging && Input.GetMouseButton(0))
        {
            float enter;
            if (groundPlane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                spawnedPico.transform.position = hitPoint + dragOffset;
            }
        }

        // 结束拖拽
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
#endif

    //------------------------------------------
    // 真机模式下：AR 平面点击放置
    //------------------------------------------
    void RunOnDevice()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                if (spawnedPico == null)
                    spawnedPico = Instantiate(picoPrefab, hitPose.position, hitPose.rotation);
                else
                    spawnedPico.transform.position = hitPose.position;
            }
        }
    }
}
