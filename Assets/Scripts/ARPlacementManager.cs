using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARPlacementManager : MonoBehaviour
{
    public static ARPlacementManager Instance;

    public ARPlaneManager planeManager;
    public GameObject pico2DPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public bool IsOverPlane(Vector3 picoPos)
    {
        if (planeManager == null) return false;

        foreach (var plane in planeManager.trackables)
        {
            Vector3 center = plane.transform.position;
            Vector2 size = plane.size;
            float halfX = size.x / 2f;
            float halfZ = size.y / 2f;

            if (picoPos.x >= center.x - halfX && picoPos.x <= center.x + halfX &&
                picoPos.z >= center.z - halfZ && picoPos.z <= center.z + halfZ &&
                Mathf.Abs(picoPos.y - center.y) < 0.3f)
            {
                return true;
            }
        }
        return false;
    }

    public void PlacePico2D(GameObject pico3D)
    {
        Vector3 pos = pico3D.transform.position;
        Destroy(pico3D);

        GameObject pico2D = Instantiate(pico2DPrefab, pos, Quaternion.identity);

        Rigidbody rb = pico2D.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (pico2D.GetComponent<Draggable2D>() == null)
            pico2D.AddComponent<Draggable2D>();
    }

    public float GetPlaneY()
    {
        foreach (var plane in planeManager.trackables)
        {
            if (plane != null) return plane.transform.position.y;
        }
        return 0f;
    }
}
