using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;

[RequireComponent(typeof(ARPlane))]
[RequireComponent(typeof(LineRenderer))]
public class ARPlaneOutline : MonoBehaviour
{
    private ARPlane plane;
    private LineRenderer line;

    void Awake()
    {
        plane = GetComponent<ARPlane>();
        line = GetComponent<LineRenderer>();

        // 注册事件：当平面边界变化时更新描边
        plane.boundaryChanged += OnBoundaryChanged;
    }

    void OnBoundaryChanged(ARPlaneBoundaryChangedEventArgs args)
    {
        UpdateLineRenderer();
    }

    void UpdateLineRenderer()
    {
        NativeArray<Vector2> boundary = plane.boundary;
        if (!boundary.IsCreated || boundary.Length == 0) return;

        line.positionCount = boundary.Length;
        for (int i = 0; i < boundary.Length; i++)
        {
            Vector2 point = boundary[i];
            Vector3 worldPoint = plane.transform.TransformPoint(new Vector3(point.x, 0f, point.y));
            line.SetPosition(i, worldPoint);
        }

        line.loop = true;
    }

    void OnDestroy()
    {
        plane.boundaryChanged -= OnBoundaryChanged;
    }
}
