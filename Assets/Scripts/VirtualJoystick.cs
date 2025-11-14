using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 虚拟摇杆控制器
/// </summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public static VirtualJoystick Instance;

    [Header("Joystick Settings")]
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;
    public float handleRange = 50f;

    [Header("Jump Button")]
    public GameObject jumpButton;

    private Vector2 inputVector;
    private bool jumpPressed = false;

    public float Horizontal => inputVector.x;
    public float Vertical => inputVector.y;
    public bool JumpPressed
    {
        get
        {
            bool pressed = jumpPressed;
            jumpPressed = false; // 读取后重置
            return pressed;
        }
    }

    void Awake()
    {
        Instance = this;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out position);

        position = Vector2.ClampMagnitude(position, handleRange);
        joystickHandle.anchoredPosition = position;

        inputVector = position / handleRange;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        joystickHandle.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// 跳跃按钮点击（在Inspector中绑定）
    /// </summary>
    public void OnJumpButtonClicked()
    {
        jumpPressed = true;
    }
}