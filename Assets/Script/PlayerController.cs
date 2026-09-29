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

        // 向き変更
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

    private void FixedUpdate()
    {
        float currentSpeed = isCarrying ? carrySpeed : normalSpeed;

        rb.MovePosition(
            rb.position + moveInput * currentSpeed * Time.fixedDeltaTime
        );
    }

    public void SetCarrying(bool carrying)
    {
        isCarrying = carrying;
    }

    public bool IsCarrying()
    {
        return isCarrying;
    }
}