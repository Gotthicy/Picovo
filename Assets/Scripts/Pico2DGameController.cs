using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// 2D Pico游戏控制器
/// 实现类似马里奥的2D平台游戏玩法
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Pico2DGameController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = 20f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer = -1; // 默认包含所有层

    [Header("Plane Lock")]
    public float planeLockDistance = 0.1f; // 锁定在平面上方的距离

    private Rigidbody rb;
    private InteractivePlaneOverlay planeOverlay;
    private bool isGrounded = false;
    private Vector3 velocity;
    private bool isControlEnabled = false;

    void Awake()
    {
        // 提前配置 Rigidbody，避免掉落
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    public void Initialize(InteractivePlaneOverlay overlay)
    {
        planeOverlay = overlay;
        isControlEnabled = true;

        // 确保 Rigidbody 配置正确
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        
        rb.useGravity = false; // 我们自己处理重力
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // 防止旋转

        // 确保有碰撞器
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
          //  box.size = new Vector3(0.5f, 0.5f, 0.1f);
        }

        // 立即锁定到平面表面
        LockToPlane();
    }

    void Update()
    {
        if (!isControlEnabled) return;

        // 检测地面
        CheckGround();

        // 获取输入
        HandleInput();
    }

    void FixedUpdate()
    {
        if (!isControlEnabled || planeOverlay == null) return;

        // 应用移动
        ApplyMovement();

        // 确保锁定在平面上
        LockToPlane();
    }

    /// <summary>
    /// 将 Pico2D 锁定在平面表面上
    /// </summary>
    void LockToPlane()
    {
        if (planeOverlay == null || planeOverlay.arPlane == null) return;

        ARPlane plane = planeOverlay.arPlane;
        Vector3 planeNormal = plane.transform.up;
        Vector3 planeCenter = plane.center;

        // 计算当前位置到平面的距离
        Vector3 toPlane = transform.position - planeCenter;
        float distanceFromPlane = Vector3.Dot(toPlane, planeNormal);

        // 如果距离不对，调整位置
        float targetDistance = planeLockDistance;
        if (Mathf.Abs(distanceFromPlane - targetDistance) > 0.01f)
        {
            // 计算在平面上的投影位置
            Vector3 onPlane = planeCenter + (toPlane - planeNormal * distanceFromPlane);
            Vector3 targetPos = onPlane + planeNormal * targetDistance;
            transform.position = targetPos;
        }

        // 确保速度在平面内（移除垂直于平面的速度分量）
        Vector3 planeVelocity = rb.velocity - Vector3.Project(rb.velocity, planeNormal);
        rb.velocity = planeVelocity;
        velocity = planeVelocity;
    }

    /// <summary>
    /// 地面检测
    /// </summary>
    void CheckGround()
    {
        if (planeOverlay == null || planeOverlay.arPlane == null) return;

        // 向下发射射线检测地面（沿着平面法线的反方向）
        Vector3 down = -planeOverlay.arPlane.transform.up;
        Ray ray = new Ray(transform.position, down);

        isGrounded = Physics.Raycast(ray, groundCheckDistance + 0.05f, groundLayer);

        Debug.DrawRay(transform.position, down * (groundCheckDistance + 0.05f), isGrounded ? Color.green : Color.red);
    }

    /// <summary>
    /// 处理输入
    /// </summary>
   void HandleInput()
{
    if (planeOverlay == null || planeOverlay.arPlane == null) return;

    // 从虚拟摇杆获取输入
    float horizontal = VirtualJoystick.Instance != null ? VirtualJoystick.Instance.Horizontal : 0f;
    float vertical = VirtualJoystick.Instance != null ? VirtualJoystick.Instance.Vertical : 0f;

    // 跳跃
    if (VirtualJoystick.Instance != null && VirtualJoystick.Instance.JumpPressed && isGrounded)
    {
        velocity += planeOverlay.arPlane.transform.up * jumpForce;
    }

    // 水平移动（基于摄像机方向）
    Vector3 cameraForward = Camera.main.transform.forward;
    Vector3 cameraRight = Camera.main.transform.right;

    // 保持水平移动（忽略摄像机的上下倾斜）
    cameraForward.y = 0;
    cameraRight.y = 0;
    cameraForward.Normalize();
    cameraRight.Normalize();

    // 结合前后和左右移动
    Vector3 moveDir = (cameraForward * vertical + cameraRight * horizontal).normalized;

    // 更新速度：保持垂直分量，更新水平分量
    Vector3 planeNormal = planeOverlay.arPlane.transform.up;
    Vector3 currentHorizontal = velocity - Vector3.Project(velocity, planeNormal);
    Vector3 newHorizontal = moveDir * moveSpeed;
    Vector3 verticalVelocity = Vector3.Project(velocity, planeNormal);

    velocity = newHorizontal + verticalVelocity;
}

    /// <summary>
    /// 应用移动和重力
    /// </summary>
    void ApplyMovement()
    {
        if (planeOverlay == null || planeOverlay.arPlane == null) return;

        ARPlane plane = planeOverlay.arPlane;
        Vector3 planeNormal = plane.transform.up;

        // 应用重力（沿着平面法线的反方向）
        if (!isGrounded)
        {
            Vector3 gravityDir = -planeNormal;
            velocity += gravityDir * gravity * Time.fixedDeltaTime;
        }
        else
        {
            // 在地面上时，移除向下的速度分量
            float downSpeed = Vector3.Dot(velocity, -planeNormal);
            if (downSpeed < 0)
            {
                velocity += planeNormal * downSpeed;
            }
        }

        // 确保速度在平面内（移除垂直于平面的速度分量，除非是跳跃）
        Vector3 vertical = Vector3.Project(velocity, planeNormal);
        Vector3 horizontal = velocity - vertical;
        
        // 如果垂直速度向下且在地面上，则移除它
        if (isGrounded && Vector3.Dot(vertical, -planeNormal) > 0)
        {
            velocity = horizontal;
        }

        // 应用速度
        rb.velocity = velocity;

        // 边界限制（确保Pico不离开平面多边形）
        Vector3 nextPos = transform.position + rb.velocity * Time.fixedDeltaTime;
        if (!ARPlacementManager.Instance.IsInsidePlanePolygon(nextPos, plane))
        {
            // 将速度限制在平面边界内
            rb.velocity = Vector3.zero;
            velocity = Vector3.zero;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 碰到敌人、道具等的逻辑
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hit enemy!");
            // 处理受伤逻辑
        }

        if (collision.gameObject.CompareTag("Coin"))
        {
            Debug.Log("Collected coin!");
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Goal"))
        {
            Debug.Log("Level completed!");
            OnLevelComplete();
        }
    }

    /// <summary>
    /// 关卡完成
    /// </summary>
    void OnLevelComplete()
    {
        isControlEnabled = false;
        ARPlacementManager.Instance.OnLevelCompleted();

        // 可以添加2D Pico的拖拽脚本，允许拖出
        Draggable2D draggable = gameObject.AddComponent<Draggable2D>();
        draggable.Init(planeOverlay.arPlane);
    }
}