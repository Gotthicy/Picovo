using UnityEngine;
using UnityEngine.EventSystems;

public class PicoSpawner : MonoBehaviour, IPointerDownHandler
{
    public GameObject pico3DPrefab;
    public float spawnDistance = 2f; // 摄像机前生成距离

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pico3DPrefab == null)
        {
            Debug.LogWarning("pico3DPrefab not assigned!");
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = spawnDistance; 
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(mousePos);

        GameObject pico3D = Instantiate(pico3DPrefab, spawnPos, Quaternion.identity);

        // 改这里：挂上正确的拖拽脚本
        if (pico3D.GetComponent<DraggablePico>() == null)
            pico3D.AddComponent<DraggablePico>();

        // 确保 Collider
        if (pico3D.GetComponent<Collider>() == null)
            pico3D.AddComponent<BoxCollider>();

        // Rigidbody
        if (pico3D.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = pico3D.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }
}
