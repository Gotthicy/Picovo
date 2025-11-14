using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// 交互平面覆盖层
/// 负责在AR平面上管理2D游戏逻辑
/// </summary>
public class InteractivePlaneOverlay : MonoBehaviour
{
    [Header("References")]
    public ARPlane arPlane;
    private ARPlacementManager manager;

    [Header("Gameplay Components")]
    public Camera gameplay2DCamera; // 2D游戏正交相机
    public GameObject currentLevel; // 当前关卡实例
    public GameObject currentPico; // 当前的2D Pico

    [Header("Settings")]
    public float cameraHeight = 3f; // 相机距离平面高度
    public float cameraSize = 2f; // 正交相机大小
    public LayerMask gameplayLayer; // 2D游戏专用层

    private bool isGameplayActive = false;

    public void Initialize(ARPlane plane, ARPlacementManager mgr)
    {
        arPlane = plane;
        manager = mgr;
    }

    /// <summary>
    /// 开始2D游戏
    /// </summary>
    public void StartGameplay(GameObject pico2D)
    {
        if (isGameplayActive) return;

        isGameplayActive = true;
        currentPico = pico2D;

        // 创建2D游戏相机
        Setup2DCamera();

        // 生成关卡
        SpawnLevel();

        // 配置2D Pico的游戏控制器
        Setup2DPicoController();

        Debug.Log("Gameplay started on plane " + arPlane.trackableId);
    }

    /// <summary>
    /// 设置2D游戏相机
    /// 关键：创建一个俯视平面的正交相机
    /// </summary>
    void Setup2DCamera()
    {
        // 创建相机GameObject
        GameObject camObj = new GameObject("Gameplay2DCamera_" + arPlane.trackableId);
        camObj.transform.SetParent(transform);

        gameplay2DCamera = camObj.AddComponent<Camera>();

        // 配置为正交相机
        gameplay2DCamera.orthographic = true;
        gameplay2DCamera.orthographicSize = cameraSize;
        gameplay2DCamera.nearClipPlane = 0.1f;
        gameplay2DCamera.farClipPlane = 10f;

        // 设置渲染顺序（在AR相机之上）
        gameplay2DCamera.depth = 10;
        gameplay2DCamera.clearFlags = CameraClearFlags.Depth; // 只清除深度，保留AR背景

        // 只渲染游戏层
        if (gameplayLayer.value != 0)
            gameplay2DCamera.cullingMask = gameplayLayer;

        // 定位相机：在平面上方，朝下看
        camObj.transform.position = arPlane.center + arPlane.transform.up * cameraHeight;
        camObj.transform.rotation = Quaternion.LookRotation(-arPlane.transform.up, arPlane.transform.forward);

        gameplay2DCamera.enabled = true;
    }

    /// <summary>
    /// 生成2D关卡
    /// </summary>
    void SpawnLevel()
    {
        if (manager.levelPrefab == null)
        {
            Debug.LogWarning("Level prefab not assigned!");
            return;
        }

        currentLevel = Instantiate(manager.levelPrefab, transform);
        currentLevel.transform.localPosition = Vector3.zero;
        currentLevel.transform.localRotation = Quaternion.identity;
        currentLevel.transform.localScale = Vector3.one;

        // 设置关卡的层级
        SetLayerRecursively(currentLevel, LayerMask.NameToLayer("Gameplay"));
    }

    /// <summary>
    /// 配置2D Pico的游戏控制器
    /// </summary>
    void Setup2DPicoController()
    {
        if (currentPico == null) return;

        // 移除拖拽脚本
        Draggable2D draggable = currentPico.GetComponent<Draggable2D>();
        if (draggable != null)
            Destroy(draggable);

        // 添加2D游戏控制器
        Pico2DGameController controller = currentPico.GetComponent<Pico2DGameController>();
        if (controller == null)
            controller = currentPico.AddComponent<Pico2DGameController>();

        controller.Initialize(this);

        // 设置Pico的层级
        SetLayerRecursively(currentPico, LayerMask.NameToLayer("Gameplay"));
    }

    /// <summary>
    /// 结束游戏
    /// </summary>
    public void EndGameplay()
    {
        if (!isGameplayActive) return;

        isGameplayActive = false;

        // 销毁关卡
        if (currentLevel != null)
            Destroy(currentLevel);

        // 禁用相机
        if (gameplay2DCamera != null)
            Destroy(gameplay2DCamera.gameObject);

        currentPico = null;
        currentLevel = null;
        gameplay2DCamera = null;

        Debug.Log("Gameplay ended");
    }

    /// <summary>
    /// 坐标转换：世界坐标 -> 平面本地坐标
    /// </summary>
    public Vector3 WorldToPlaneLocal(Vector3 worldPos)
    {
        return arPlane.transform.InverseTransformPoint(worldPos);
    }

    /// <summary>
    /// 坐标转换：平面本地坐标 -> 世界坐标
    /// </summary>
    public Vector3 PlaneLocalToWorld(Vector3 localPos)
    {
        return arPlane.transform.TransformPoint(localPos);
    }

    /// <summary>
    /// 递归设置GameObject及其所有子对象的Layer
    /// </summary>
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}