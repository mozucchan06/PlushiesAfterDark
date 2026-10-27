using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private enum Direction { Front, Back, Left, Right }

    [SerializeField] private float normalSpeed = 2.5f;
    [SerializeField] private float carrySpeed = 1f;

    // 歩きの絵(各方向に複数枚。1枚目は「立っている絵」にしておく)
    [Header("歩きの絵(各方向に複数枚)")]
    [SerializeField] private Sprite[] frontSprites; // 下向き(正面)
    [SerializeField] private Sprite[] backSprites;  // 上向き(後ろ姿)
    [SerializeField] private Sprite[] leftSprites;  // 左向き
    [SerializeField] private Sprite[] rightSprites; // 右向き
    [SerializeField] private float frameInterval = 0.12f; // 1コマの秒数(通常の速さの時)

    [Header("足音")]
    [Tooltip("足音の間隔(秒)。通常の速さの時。思い出を持って遅い時は、自動で長くなる")]
    [SerializeField] private float _stepInterval = 0.38f;
    [Tooltip("足音の大きさ(0?1)")]
    [SerializeField, Range(0f, 1f)] private float _stepVolume = 0.6f;

    private bool isCarrying = false;

    // 掴んでいる間の移動軸(zeroなら掴んでいない)
    private Vector2 grabAxis = Vector2.zero;

    // 隠れ場に隠れている間は true
    private bool isHidden = false;

    // 見つかった時などに、操作を止めている間は true
    private bool isFrozen = false;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private SpriteRenderer spriteRenderer;

    // 歩きアニメーション用
    private Direction facing = Direction.Front;
    private int frameIndex = 0;
    private float frameTimer = 0f;
    private float _stepTimer = 0f; // 足音用のタイマー

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        moveInput = Vector2.zero;

        // 隠れている間・止められている間は操作を受け付けない
        if (isHidden || isFrozen) return;

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
                SetFacing(Direction.Back);   // 上に進む → 後ろ姿
            }
            else if (moveInput.y < 0)
            {
                SetFacing(Direction.Front);  // 下に進む → 正面
            }
            else if (moveInput.x < 0)
            {
                SetFacing(Direction.Left);   // 左に進む → 左向き
            }
            else if (moveInput.x > 0)
            {
                SetFacing(Direction.Right);  // 右に進む → 右向き
            }
        }

        UpdateWalkAnimation();
    }

    private void FixedUpdate()
    {
        if (isHidden || isFrozen) return;

        float currentSpeed = GetCurrentSpeed();

        Vector2 move = moveInput;

        // 掴んでいる間は、掴んだ軸方向の成分だけ使う
        if (grabAxis != Vector2.zero)
        {
            move = grabAxis * Vector2.Dot(moveInput, grabAxis);
        }

        rb.MovePosition(rb.position + move * currentSpeed * Time.fixedDeltaTime);
    }

    // ---------- 歩きアニメーション ----------

    private Sprite[] GetFrames()
    {
        switch (facing)
        {
            case Direction.Back: return backSprites;
            case Direction.Left: return leftSprites;
            case Direction.Right: return rightSprites;
            default: return frontSprites;
        }
    }

    // 向きを変える。変わった時は、1枚目(立っている絵)から始める
    private void SetFacing(Direction newFacing)
    {
        if (facing == newFacing) return;

        facing = newFacing;
        frameIndex = 0;
        frameTimer = 0f;
        ApplySprite();
    }

    // 歩いている間だけコマを進める。止まったら、1枚目(立っている絵)に戻す
    private void UpdateWalkAnimation()
    {
        // 掴んでいる間は、掴んだ軸の方向に動いている時だけ「歩いている」とみなす
        bool moving = grabAxis == Vector2.zero
            ? moveInput != Vector2.zero
            : Mathf.Abs(Vector2.Dot(moveInput, grabAxis)) > 0.1f;

        UpdateFootstep(moving);

        Sprite[] frames = GetFrames();
        if (frames == null || frames.Length == 0) return;

        if (moving && frames.Length > 1)
        {
            // 遅い時(思い出を持っている間)は、コマ送りもゆっくりにする
            float interval = frameInterval * normalSpeed / Mathf.Max(0.01f, GetCurrentSpeed());

            frameTimer += Time.deltaTime;
            if (frameTimer >= interval)
            {
                frameTimer -= interval;
                frameIndex = (frameIndex + 1) % frames.Length;
            }
        }
        else
        {
            frameIndex = 0;
            frameTimer = 0f;
        }

        ApplySprite();
    }

    // 歩いている間、一定の間隔で足音を鳴らす。遅い時(思い出を持っている間)は、間隔を長くする
    private void UpdateFootstep(bool moving)
    {
        if (!moving)
        {
            // 次に歩き出した時、少ししてから最初の足音が鳴るようにする
            _stepTimer = _stepInterval * 0.5f;
            return;
        }

        float interval = _stepInterval * normalSpeed / Mathf.Max(0.01f, GetCurrentSpeed());

        _stepTimer += Time.deltaTime;
        if (_stepTimer >= interval)
        {
            _stepTimer = 0f;
            AudioManager.PlayRandomSE("SE/Step", _stepVolume, 0.08f);
        }
    }

    private void ApplySprite()
    {
        Sprite[] frames = GetFrames();
        if (frames == null || frames.Length == 0) return;

        spriteRenderer.sprite = frames[frameIndex % frames.Length];
    }

    // ---------- 外から使う ----------

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

        if (axis.x > 0) SetFacing(Direction.Right);
        else if (axis.x < 0) SetFacing(Direction.Left);
        else if (axis.y > 0) SetFacing(Direction.Back);
        else if (axis.y < 0) SetFacing(Direction.Front);
    }

    // 掴み終了
    public void ClearGrabAxis()
    {
        grabAxis = Vector2.zero;
    }

    // 操作を止める(見つかった時など)。シーンを読み込み直すまで解除しない
    public void Freeze()
    {
        isFrozen = true;
        moveInput = Vector2.zero;

        // 立っている絵に戻す
        frameIndex = 0;
        frameTimer = 0f;
        ApplySprite();
    }

    // 隠れているか(敵の視界判定で使う)
    public bool IsHidden()
    {
        return isHidden;
    }

    // 隠れ場に入る:絵を消して、物理も止めて、隠れ場の位置へ移動する
    public void Hide(Vector2 spotPosition)
    {
        isHidden = true;
        moveInput = Vector2.zero;

        spriteRenderer.enabled = false;
        rb.simulated = false;
        transform.position = spotPosition;
    }

    // 隠れ場から出る:元の位置に戻って、絵と物理を元に戻す
    public void Unhide(Vector2 exitPosition)
    {
        transform.position = exitPosition;

        rb.simulated = true;
        spriteRenderer.enabled = true;
        isHidden = false;
    }
}