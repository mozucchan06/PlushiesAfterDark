using UnityEngine;

/// <summary>
/// 敵(猫)の歩きアニメーション。
/// EnemyPatrol の向きと「歩いているか」を見て、各方向の絵を順に切り替える。
/// EnemyPatrol と同じオブジェクトに付ける。
/// </summary>
[RequireComponent(typeof(EnemyPatrol))]
public class EnemyWalkAnimation : MonoBehaviour
{
    #region Field

    private enum Direction { Front, Back, Left, Right }

    [Header("絵の表示先")]
    [Tooltip("空なら自分、なければ子から探す。「!」マークなど他のSpriteRendererが子にある場合は、猫の絵のSpriteRendererを直接入れる")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("歩きの絵(各方向に複数枚。1枚目は立っている絵)")]
    [SerializeField] private Sprite[] _frontSprites; // 下向き(正面)
    [SerializeField] private Sprite[] _backSprites;  // 上向き(後ろ姿)
    [SerializeField] private Sprite[] _leftSprites;  // 左向き
    [SerializeField] private Sprite[] _rightSprites; // 右向き

    [Header("アニメーション")]
    [Tooltip("1コマの秒数。小さいほど速く切り替わる")]
    [SerializeField, Range(0.02f, 1f)] private float _frameInterval = 0.15f;

    private EnemyPatrol _patrol;
    private Direction _direction = Direction.Front;
    private int _frameIndex = 0;
    private float _frameTimer = 0f;

    #endregion

    #region UnityLifeCycle

    private void Awake()
    {
        _patrol = GetComponent<EnemyPatrol>();

        // 表示先が空なら、自分 → 子の順で探す
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_spriteRenderer == null)
        {
            Debug.LogError("[EnemyWalkAnimation] Awake : SpriteRenderer is Null!!! ");
        }
    }

    private void Start()
    {
        CheckSprites(_frontSprites, "Front Sprites");
        CheckSprites(_backSprites, "Back Sprites");
        CheckSprites(_leftSprites, "Left Sprites");
        CheckSprites(_rightSprites, "Right Sprites");
    }

    private void Update()
    {
        if (_spriteRenderer == null) return;

        UpdateDirection();
        UpdateFrame();
        ApplySprite();
    }

    #endregion

    #region Animation

    // 敵の向き(Facing)を、上下左右の4方向に丸める
    private void UpdateDirection()
    {
        Vector2 facing = _patrol.Facing;

        Direction newDirection;
        if (Mathf.Abs(facing.x) > Mathf.Abs(facing.y))
        {
            newDirection = facing.x > 0f ? Direction.Right : Direction.Left;
        }
        else
        {
            newDirection = facing.y > 0f ? Direction.Back : Direction.Front;
        }

        // 向きが変わったら、1枚目(立っている絵)から始める
        if (newDirection != _direction)
        {
            _direction = newDirection;
            _frameIndex = 0;
            _frameTimer = 0f;
        }
    }

    // 歩いている間だけコマを進める。止まったら1枚目に戻す
    private void UpdateFrame()
    {
        Sprite[] frames = GetFrames();
        if (frames == null || frames.Length == 0) return;

        // 見つかって止まった時(EnemyPatrol が無効)は、歩いていない扱いにする
        bool moving = _patrol.enabled && _patrol.isMoving;

        if (moving && frames.Length > 1)
        {
            _frameTimer += Time.deltaTime;
            if (_frameTimer >= _frameInterval)
            {
                _frameTimer -= _frameInterval;
                _frameIndex = (_frameIndex + 1) % frames.Length;
            }
        }
        else
        {
            _frameIndex = 0;
            _frameTimer = 0f;
        }
    }

    private void ApplySprite()
    {
        Sprite[] frames = GetFrames();
        if (frames == null || frames.Length == 0) return;

        _spriteRenderer.sprite = frames[_frameIndex % frames.Length];
    }

    private Sprite[] GetFrames()
    {
        switch (_direction)
        {
            case Direction.Back: return _backSprites;
            case Direction.Left: return _leftSprites;
            case Direction.Right: return _rightSprites;
            default: return _frontSprites;
        }
    }

    #endregion

    #region Debug

    // 絵が入っていない向きがあれば、知らせる
    private void CheckSprites(Sprite[] sprites, string label)
    {
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[EnemyWalkAnimation] Start : {name} の {label} が未設定です");
        }
    }

    #endregion
}