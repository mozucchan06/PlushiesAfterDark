using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Eキーで掴む → もう一度Eキーで離す、押して動かせる箱。
/// 掴んでいない間は Kinematic(動かない)、掴んでいる間だけ Dynamic になる。
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PushBox : MonoBehaviour
{
    [SerializeField] private bool allowPull = true;        // 引くことを許可するか
    [SerializeField] private float releaseDistance = 1.5f; // これ以上離れたら自動で離す(中心同士の距離)

    private Rigidbody2D rb;
    private PlayerController touchingPlayer; // 今触れているプレイヤー
    private PlayerController holder;         // 掴んでいるプレイヤー
    private Vector2 grabDir;                 // 掴んだ時の「プレイヤー→箱」の軸方向

    private bool IsGrabbed => holder != null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 最初は掴まれていない状態(体当たりしても動かない)
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnDisable()
    {
        if (holder != null) holder.ClearGrabAxis();
        holder = null;
        if (GrabLock.Current == this) GrabLock.Current = null;
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        if (IsGrabbed)
        {
            Release();
        }
        else if (touchingPlayer != null && GrabLock.Current == null && !touchingPlayer.IsCarrying())
        {
            Grab(touchingPlayer);
        }
    }

    private void FixedUpdate()
    {
        if (!IsGrabbed) return;

        // プレイヤーが離れすぎたら自動で離す
        float dist = Vector2.Distance(holder.transform.position, transform.position);
        if (dist > releaseDistance)
        {
            Release();
            return;
        }

        // 掴んだ軸方向の入力だけを使う(+なら押す、-なら引く)
        float along = Vector2.Dot(holder.GetMoveInput(), grabDir);
        if (!allowPull) along = Mathf.Max(0f, along);

        // プレイヤーと同じ速さで動かす
        rb.linearVelocity = grabDir * along * holder.GetCurrentSpeed();
    }

    private void Grab(PlayerController player)
    {
        // プレイヤーが箱のどの側に立っているか(上下左右のどれか)
        Vector2 toBox = (Vector2)transform.position - (Vector2)player.transform.position;
        grabDir = Mathf.Abs(toBox.x) > Mathf.Abs(toBox.y)
            ? new Vector2(Mathf.Sign(toBox.x), 0f)
            : new Vector2(0f, Mathf.Sign(toBox.y));

        holder = player;
        GrabLock.Current = this;
        holder.SetGrabAxis(grabDir);
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private void Release()
    {
        if (holder != null) holder.ClearGrabAxis();
        holder = null;
        if (GrabLock.Current == this) GrabLock.Current = null;

        rb.linearVelocity = Vector2.zero; // Kinematicにする前に止める
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        var p = collision.collider.GetComponentInParent<PlayerController>();
        if (p != null) touchingPlayer = p;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        var p = collision.collider.GetComponentInParent<PlayerController>();
        if (p != null && p == touchingPlayer) touchingPlayer = null;
    }
}