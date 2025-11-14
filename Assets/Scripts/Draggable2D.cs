using UnityEngine;

public class Draggable2D : MonoBehaviour
{
    private Vector3 offset;
    private float zPos;

    void OnMouseDown()
    {
        zPos = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zPos));
        offset = transform.position - mouseWorld;
    }

    void OnMouseDrag()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zPos));
        Vector3 targetPos = mouseWorld + offset;

        // y坐标固定在平面
        targetPos.y = ARPlacementManager.Instance.GetPlaneY();
        transform.position = targetPos;
    }
}
