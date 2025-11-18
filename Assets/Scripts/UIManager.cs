using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI管理器
/// 控制拖拽UI和游戏UI的切换
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Panels")]
    public GameObject dragUIPanel; // 拖拽界面
    public GameObject gameplayUIPanel; // 游戏界面

    [Header("UI Elements")]
    public GameObject spawnButton; // SpawnButton（如果不在dragUIPanel中，需要单独控制）

    [Header("Gameplay UI Elements")]
    public VirtualJoystick virtualJoystick;
    public Button exitButton; // 退出游戏按钮

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // 初始显示拖拽UI
        SwitchToDragUI();

        // 绑定退出按钮
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    /// <summary>
    /// 切换到拖拽UI
    /// </summary>
    public void SwitchToDragUI()
    {
        if (dragUIPanel != null)
            dragUIPanel.SetActive(true);

        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(false);

        // 显示 SpawnButton（如果单独存在）
        if (spawnButton != null)
            spawnButton.SetActive(true);
    }

    /// <summary>
    /// 切换到游戏UI
    /// </summary>
    public void SwitchToGameplayUI()
    {
        if (dragUIPanel != null)
            dragUIPanel.SetActive(false);

        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(true);

        // 隐藏 SpawnButton（如果单独存在）
        if (spawnButton != null)
            spawnButton.SetActive(false);
    }

    /// <summary>
    /// 退出游戏按钮点击
    /// </summary>
    void OnExitButtonClicked()
    {
        if (ARPlacementManager.Instance != null && ARPlacementManager.Instance.isInGameMode)
        {
            // 获取2D Pico的当前位置作为退出位置
         //   Vector3 exitPos = ARPlacementManager.Instance.current2DPico.transform.position;
            ARPlacementManager.Instance.ExitGameMode();
        }
    }
}