using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public GameObject dragUI; // 拖拽界面
    public GameObject playUI; // 游戏界面
    public GameObject pico2DPrefab; // 2D Pico
    public Transform picoParent; // Pico2D 父物体

    GameObject activePico3D;
    GameObject activePico2D;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 进入游戏模式
    public void EnterPlayMode(GameObject placed3DPico)
    {
        activePico3D = placed3DPico;

        dragUI.SetActive(false);
        playUI.SetActive(true);

        // 隐藏3D Pico
        activePico3D.SetActive(false);

        // 在相同位置生成2D Pico
        activePico2D = Instantiate(pico2DPrefab, picoParent);
        activePico2D.transform.position = activePico3D.transform.position;

        // 这里2D Pico会有自己的脚本控制移动
        activePico2D.GetComponent<Pico2DController>().StartGame();
    }

    // 退出游戏模式
    public void ExitPlayMode()
    {
        if (activePico2D != null) Destroy(activePico2D);

        if (activePico3D != null) activePico3D.SetActive(true);

        playUI.SetActive(false);
        dragUI.SetActive(true);
    }
}
