using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    public static ARPlacementManager Instance;

    [Header("AR Components")]
    public ARPlaneManager planeManager;
    public ARRaycastManager raycastManager;

    [Header("Prefabs")]
    public GameObject pico2DPrefab;
    public GameObject interactivePlaneOverlayPrefab; // 平面交互层预制体
    public GameObject levelPrefab; // 2D关卡预制体

    [Header("Game State")]
    public bool isInGameMode = false;
    public ARPlane currentGamePlane;
    public GameObject current2DPico;

    // 存储每个平面的交互层
    private Dictionary<TrackableId, InteractivePlaneOverlay> planeOverlays = new Dictionary<TrackableId, InteractivePlaneOverlay>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void OnEnable()
    {
        planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDisable()
    {
        planeManager.planesChanged -= OnPlanesChanged;
    }

    /// <summary>
    /// 监听平面变化，为每个平面创建交互层
    /// </summary>
    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // 新增平面
        foreach (var plane in args.added)
        {
            CreateInteractiveOverlay(plane);
        }

        // 更新平面
        foreach (var plane in args.updated)
        {
            UpdateInteractiveOverlay(plane);
        }

        // 移除平面
        foreach (var plane in args.removed)
        {
            RemoveInteractiveOverlay(plane);
        }
    }

    /// <summary>
    /// 为AR平面创建交互层
    /// 这是让平面可交互的关键
    /// </summary>
    void CreateInteractiveOverlay(ARPlane plane)
    {
        if (interactivePlaneOverlayPrefab == null)
        {
            Debug.LogWarning("interactivePlaneOverlayPrefab not assigned!");
            return;
        }

        // 在平面上创建交互层
        GameObject overlayObj = Instantiate(interactivePlaneOverlayPrefab, plane.transform);
        overlayObj.transform.localPosition = Vector3.zero;
        overlayObj.transform.localRotation = Quaternion.identity;
        overlayObj.name = $"InteractiveOverlay_{plane.trackableId}";

        // 添加网格碰撞器，使用平面的mesh
        MeshCollider meshCollider = overlayObj.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = overlayObj.AddComponent<MeshCollider>();

        MeshFilter planeMeshFilter = plane.GetComponent<MeshFilter>();
        if (planeMeshFilter != null && planeMeshFilter.mesh != null)
        {
            meshCollider.sharedMesh = planeMeshFilter.mesh;
            meshCollider.convex = false; // 使用精确碰撞
        }

        // 添加或获取交互组件
        InteractivePlaneOverlay overlay = overlayObj.GetComponent<InteractivePlaneOverlay>();
        if (overlay == null)
            overlay = overlayObj.AddComponent<InteractivePlaneOverlay>();

        overlay.Initialize(plane, this);

        // 存储引用
        planeOverlays[plane.trackableId] = overlay;

        Debug.Log($"Created interactive overlay for plane {plane.trackableId}");
    }

    /// <summary>
    /// 更新平面的交互层（当平面mesh更新时）
    /// </summary>
    void UpdateInteractiveOverlay(ARPlane plane)
    {
        if (planeOverlays.TryGetValue(plane.trackableId, out InteractivePlaneOverlay overlay))
        {
            MeshCollider meshCollider = overlay.GetComponent<MeshCollider>();
            MeshFilter planeMeshFilter = plane.GetComponent<MeshFilter>();

            if (meshCollider != null && planeMeshFilter != null && planeMeshFilter.mesh != null)
            {
                meshCollider.sharedMesh = null; // 先清空
                meshCollider.sharedMesh = planeMeshFilter.mesh; // 重新赋值
            }
        }
    }

    /// <summary>
    /// 移除平面的交互层
    /// </summary>
    void RemoveInteractiveOverlay(ARPlane plane)
    {
        if (planeOverlays.TryGetValue(plane.trackableId, out InteractivePlaneOverlay overlay))
        {
            if (overlay != null)
                Destroy(overlay.gameObject);

            planeOverlays.Remove(plane.trackableId);
        }
    }

    /// <summary>
    /// 获取距离某个世界坐标最近的平面
    /// </summary>
    public ARPlane GetNearestPlane(Vector3 worldPos)
    {
        ARPlane nearest = null;
        float minDist = float.MaxValue;

        foreach (var plane in planeManager.trackables)
        {
            float dist = Vector3.Distance(worldPos, plane.center);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = plane;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 判断世界坐标点是否在平面多边形内
    /// 使用射线法（Ray Casting Algorithm）
    /// </summary>
    public bool IsInsidePlanePolygon(Vector3 worldPoint, ARPlane plane)
    {
        if (plane == null) return false;

        // 转换到平面的本地坐标系
        Vector3 localPoint = plane.transform.InverseTransformPoint(worldPoint);
        Vector2 testPoint = new Vector2(localPoint.x, localPoint.z);

        var poly = plane.boundary;
        if (poly == null || poly.Length < 3) return false;

        int count = poly.Length;
        bool inside = false;

        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            Vector2 pi = poly[i];
            Vector2 pj = poly[j];

            bool intersect = ((pi.y > testPoint.y) != (pj.y > testPoint.y)) &&
                             (testPoint.x < (pj.x - pi.x) * (testPoint.y - pi.y) / (pj.y - pi.y) + pi.x);
            if (intersect) inside = !inside;
        }

        return inside;
    }

    /// <summary>
    /// 生成2D Pico并进入游戏模式
    /// </summary>
    public void Spawn2DPico(Vector3 pos, ARPlane plane)
    {
        if (isInGameMode)
        {
            Debug.LogWarning("Already in game mode!");
            return;
        }

        // 将位置投影到平面表面
        Vector3 planeNormal = plane.transform.up;
        Vector3 planeCenter = plane.center;
        // 计算点到平面的距离，然后投影到平面上方0.1f的位置
        Vector3 toPlane = pos - planeCenter;
        float distanceFromPlane = Vector3.Dot(toPlane, planeNormal);
        Vector3 projectedPos = planeCenter + planeNormal * 0.1f + (toPlane - planeNormal * distanceFromPlane);

        GameObject obj = Instantiate(pico2DPrefab, projectedPos, Quaternion.identity);

        // 调整朝向：让 Pico2D 面向平面的 forward 方向
        if (Vector3.Dot(plane.transform.up, Vector3.up) > 0.7f)
        {
            obj.transform.rotation = Quaternion.identity;
        }
        else
        {
            // 计算朝向：forward 沿着平面的 forward，up 沿着平面的 up
            obj.transform.rotation = Quaternion.LookRotation(plane.transform.forward, plane.transform.up);
        }

        // 设置为平面的子对象
        obj.transform.SetParent(plane.transform);

        current2DPico = obj;
        currentGamePlane = plane;

        // 进入游戏模式
        EnterGameMode(plane, obj);
    }

    /// <summary>
    /// 进入2D游戏模式
    /// </summary>
    public void EnterGameMode(ARPlane plane, GameObject pico2D)
    {
        isInGameMode = true;

        // 获取该平面的交互层
        if (planeOverlays.TryGetValue(plane.trackableId, out InteractivePlaneOverlay overlay))
        {
            overlay.StartGameplay(pico2D);
        }

        // 切换UI
        UIManager.Instance?.SwitchToGameplayUI();

        Debug.Log("Entered game mode on plane " + plane.trackableId);
    }

    /// <summary>
    /// 退出2D游戏模式，恢复3D
    /// </summary>
    public void ExitGameMode(Vector3 exitPosition)
    {
        if (!isInGameMode || current2DPico == null) return;

        isInGameMode = false;

        // 停止当前平面的游戏
        if (currentGamePlane != null && planeOverlays.TryGetValue(currentGamePlane.trackableId, out InteractivePlaneOverlay overlay))
        {
            overlay.EndGameplay();
        }

        // 销毁2D Pico
        Destroy(current2DPico);
        current2DPico = null;
        currentGamePlane = null;

        // 切换UI
        UIManager.Instance?.SwitchToDragUI();

        Debug.Log("Exited game mode");
    }

    /// <summary>
    /// 任务完成回调
    /// </summary>
    public void OnLevelCompleted()
    {
        Debug.Log("Level completed! You can now drag Pico out.");
        // 可以在这里显示提示UI，告诉用户可以拖出Pico了
    }
}