using UnityEngine;

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
    public LayerMask groundLayer;

    private Rigidbody rb;
    private InteractivePlaneOverlay planeOverlay;
    private bool isGrounded = false;
    private Vector3 velocity;
    private bool isControlEnabled = false;

    public void Initialize(InteractivePlaneOverlay overlay)
    {
        planeOverlay = overlay;
        isControlEnabled = true;

        // 配置Rigidbody
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // 我们自己处理重力
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // 防止旋转

        // 确保有碰撞器
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(0.5f, 0.5f, 0.1f);
        }
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

        ApplyMovement();
    }

    /// <summary>
    /// 地面检测
    /// </summary>
    void CheckGround()
    {
        // 向下发射射线检测地面
        Vector3 down = -planeOverlay.arPlane.transform.up;
        Ray ray = new Ray(transform.position, down);

        isGrounded = Physics.Raycast(ray, groundCheckDistance, groundLayer);

        Debug.DrawRay(transform.position, down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    /// <summary>
    /// 处理输入
    /// </summary>
    void HandleInput()
    {
        // 从虚拟摇杆获取输入
        float horizontal = VirtualJoystick.Instance != null ? VirtualJoystick.Instance.Horizontal : 0f;

        // 跳跃
        if (VirtualJoystick.Instance != null && VirtualJoystick.Instance.JumpPressed && isGrounded)
        {
            velocity += planeOverlay.arPlane.transform.up * jumpForce;
        }

        // 水平移动（沿着平面的forward和right方向）
        Vector3 forward = planeOverlay.arPlane.transform.forward;
        Vector3 right = planeOverlay.arPlane.transform.right;

        // 根据平面朝向调整移动方向
        Vector3 moveDir = right * horizontal;
        velocity = new Vector3(moveDir.x * moveSpeed, velocity.y, moveDir.z * moveSpeed);
    }

    /// <summary>
    /// 应用移动和重力
    /// </summary>
    void ApplyMovement()
    {
        // 应用重力
        if (!isGrounded)
        {
            Vector3 gravityDir = -planeOverlay.arPlane.transform.up;
            velocity += gravityDir * gravity * Time.fixedDeltaTime;
        }
        else
        {
            // 在地面上时，重置Y轴速度
            Vector3 up = planeOverlay.arPlane.transform.up;
            float upSpeed = Vector3.Dot(velocity, up);
            if (upSpeed < 0)
            {
                velocity -= up * upSpeed;
            }
        }

        // 应用速度
        rb.velocity = velocity;

        // 边界限制（确保Pico不离开平面多边形）
        Vector3 nextPos = transform.position + rb.velocity * Time.fixedDeltaTime;
        if (!ARPlacementManager.Instance.IsInsidePlanePolygon(nextPos, planeOverlay.arPlane))
        {
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