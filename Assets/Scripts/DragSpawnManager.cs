using UnityEngine;
using UnityEngine.EventSystems;

public class DragSpawnManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public GameObject picoPreviewPrefab;   // 拖拽生成的 Pico 预览
    private GameObject currentPreview;     // 当前正在拖动的预览对象
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    // 按下按钮时触发
    public void OnPointerDown(PointerEventData eventData)
    {
        if (picoPreviewPrefab == null) return;
        currentPreview = Instantiate(picoPreviewPrefab, Vector3.zero, Quaternion.identity);
        currentPreview.transform.localScale = Vector3.one * 0.1f; // 预览小一点
    }

    // 拖动时更新位置
    public void OnDrag(PointerEventData eventData)
    {
        if (currentPreview == null) return;

        // 把屏幕坐标转到世界坐标
        Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, 0.3f); 
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);

        currentPreview.transform.position = worldPos;
        currentPreview.transform.LookAt(mainCamera.transform);
    }

    // 松开按钮时销毁预览
    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
    }
}
