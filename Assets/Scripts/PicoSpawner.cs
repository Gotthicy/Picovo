using UnityEngine;
using UnityEngine.EventSystems;

public class PicoSpawner : MonoBehaviour, IPointerDownHandler
{
    public GameObject pico3DPrefab; // 3D Pico Prefab
    public float spawnDistance = 1.5f; // 摄像机前生成距离

    public void OnPointerDown(PointerEventData eventData)
    {

        if (pico3DPrefab == null)
        {
            Debug.LogWarning("pico3DPrefab not assigned!");
            return;
        }

        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x, Input.mousePosition.y, spawnDistance));

        GameObject pico3D = Instantiate(pico3DPrefab, spawnPos, Quaternion.identity);

        // 添加拖拽脚本
        if (pico3D.GetComponent<Draggable3D>() == null)
            pico3D.AddComponent<Draggable3D>();
    }
}
