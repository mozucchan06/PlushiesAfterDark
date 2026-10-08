using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float normalSpeed = 2.5f;
    [SerializeField] private float carrySpeed = 1f;

    // 4方向の画像
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    private bool isCarrying = false;

    // 掴んでいる間の移動軸(zeroなら掴んでいない)
    private Vector2 grabAxis = Vector2.zero;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        moveInput = Vector2.zero;

        // 左右
        if (Keyboard.current.aKey.isPressed)
        {
            moveInput.x = -1;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            moveInput.x = 1;
        }

        // 上下
        if (Keyboard.current.wKey.isPressed)
        {
            moveInput.y = 1;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            moveInput.y = -1;
        }

        moveInput = moveInput.normalized;

        // 向き変更(掴んでいる間は変えない)
        if (grabAxis == Vector2.zero)
        {
            if (moveInput.y > 0)
            {
                // 上に進む → 後ろ姿
                spriteRenderer.sprite = backSprite;
            }
            else if (moveInput.y < 0)
            {
                // 下に進む → 正面
                spriteRenderer.sprite = frontSprite;
            }
            else if (moveInput.x < 0)
            {
                // 左に進む → 左向き
                spriteRenderer.sprite = leftSprite;
            }
            else if (moveInput.x > 0)
            {
                // 右に進む → 右向き
                spriteRenderer.sprite = rightSprite;
            }
        }
    }

    private void FixedUpdate()
    {
        float currentSpeed = GetCurrentSpeed();

        Vector2 move = moveInput;

        // 掴んでいる間は、掴んだ軸方向の成分だけ使う
        if (grabAxis != Vector2.zero)
        {
            move = grabAxis * Vector2.Dot(moveInput, grabAxis);
        }

        rb.MovePosition(rb.position + move * currentSpeed * Time.fixedDeltaTime);
    }

    // 思い出を持っている状態にする
    public void SetCarrying(bool carrying)
    {
        isCarrying = carrying;
    }

    // 思い出を持っているか
    public bool IsCarrying()
    {
        return isCarrying;
    }

    // Playerが現在移動している方向
    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    // 現在の移動速度
    public float GetCurrentSpeed()
    {
        return isCarrying ? carrySpeed : normalSpeed;
    }

    // 掴み開始:この軸方向にしか動けなくなり、箱の方を向く
    public void SetGrabAxis(Vector2 axis)
    {
        grabAxis = axis;

        if (axis.x > 0) spriteRenderer.sprite = rightSprite;
        else if (axis.x < 0) spriteRenderer.sprite = leftSprite;
        else if (axis.y > 0) spriteRenderer.sprite = backSprite;
        else if (axis.y < 0) spriteRenderer.sprite = frontSprite;
    }

    // 掴み終了
    public void ClearGrabAxis()
    {
        grabAxis = Vector2.zero;
    }
}