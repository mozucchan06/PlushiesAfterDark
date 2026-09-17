using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float normalSpeed = 2.5f;
    [SerializeField] private float carrySpeed = 1f;
    private bool isCarrying = false;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        moveInput = Vector2.zero;

        // ç∂âE
        if (Keyboard.current.aKey.isPressed)
        {
            moveInput.x = -1;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            moveInput.x = 1;
        }

        // è„â∫
        if (Keyboard.current.wKey.isPressed)
        {
            moveInput.y = 1;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            moveInput.y = -1;
        }

        moveInput = moveInput.normalized;
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