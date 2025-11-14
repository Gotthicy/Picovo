using UnityEngine;

public class Pico2DController : MonoBehaviour
{
    public float moveSpeed = 2f;   // 移动速度
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void StartGame()
    {
        // 游戏开始时可初始化内容
        Debug.Log("Pico 2D Game Started!");
    }

    void FixedUpdate()
    {
        // 暂时使用键盘输入来模拟摇杆
        float h = Input.GetAxis("Horizontal");  // A/D 或 ← →
        float v = Input.GetAxis("Vertical");    // W/S 或 ↑ ↓

        Vector2 move = new Vector2(h, v) * moveSpeed;
        rb.velocity = move;
    }

    public void Jump()
    {
        // 可选：如果想让 Pico 跳起来
        rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
    }
}
