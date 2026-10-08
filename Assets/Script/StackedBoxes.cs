using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 高く積まれた箱。Eで掴んで押し続けると崩れ、上に乗っていたアイテムが落ちて拾えるようになる。
/// このスクリプトは「Collider2D(Is Triggerオフ)と Rigidbody2D(Static)を持つオブジェクト」に付ける。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class StackedBoxes : MonoBehaviour
{
    [Header("見た目(子オブジェクト)")]
    [SerializeField] private GameObject standingVisual;   // 積み上がった見た目
    [SerializeField] private GameObject collapsedVisual;  // 崩れた後の見た目

    [Header("アイテム(このオブジェクトの子にしない)")]
    [SerializeField] private Transform memoryItem;        // 上に乗っているアイテム
    [SerializeField] private Collider2D itemCollider;     // アイテムの当たり判定(最初は無効にされる)
    [SerializeField] private Transform dropPoint;         // 崩れた時にアイテムが落ちる位置

    [Header("崩れる条件")]
    [SerializeField] private float pushTimeToCollapse = 1.5f; // 押し続ける必要がある秒数
    [SerializeField] private float releaseDistance = 1.5f;    // これ以上離れたら自動で離す

    private Collider2D blockCollider;
    private PlayerController touchingPlayer;
    private PlayerController holder;
    private Vector2 grabDir;
    private float pushTimer;
    private bool collapsed;

    private bool IsGrabbed => holder != null;

    private void Awake()
    {
        blockCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        standingVisual.SetActive(true);
        collapsedVisual.SetActive(false);
        itemCollider.enabled = false; // 崩れるまで拾えない
    }

    private void OnDisable()
    {
        if (holder != null) holder.ClearGrabAxis();
        holder = null;
        if (GrabLock.Current == this) GrabLock.Current = null;
    }

    private void Update()
    {
        if (collapsed) return;

#if UNITY_EDITOR
        // 動作確認用:Tキーで即座に崩す(完成したら消してよい)
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            Release();
            Collapse();
            return;
        }
#endif

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
        if (!IsGrabbed || collapsed) return;

        // プレイヤーが離れすぎたら自動で離す
        float dist = Vector2.Distance(holder.transform.position, transform.position);
        if (dist > releaseDistance)
        {
            Release();
            return;
        }

        // 積み箱の方向へ押している間だけ、押した時間をためる
        float along = Vector2.Dot(holder.GetMoveInput(), grabDir);
        if (along > 0.5f)
        {
            pushTimer += Time.fixedDeltaTime;
        }
        else
        {
            pushTimer = Mathf.Max(0f, pushTimer - Time.fixedDeltaTime);
        }

        if (pushTimer >= pushTimeToCollapse)
        {
            Release();
            Collapse();
        }
    }

    private void Grab(PlayerController player)
    {
        Vector2 toStack = (Vector2)transform.position - (Vector2)player.transform.position;
        grabDir = Mathf.Abs(toStack.x) > Mathf.Abs(toStack.y)
            ? new Vector2(Mathf.Sign(toStack.x), 0f)
            : new Vector2(0f, Mathf.Sign(toStack.y));

        holder = player;
        pushTimer = 0f;
        GrabLock.Current = this;
        holder.SetGrabAxis(grabDir);
    }

    private void Release()
    {
        if (holder != null) holder.ClearGrabAxis();
        holder = null;
        pushTimer = 0f;
        if (GrabLock.Current == this) GrabLock.Current = null;
    }

    public void Collapse()
    {
        if (collapsed) return;
        collapsed = true;

        standingVisual.SetActive(false);
        collapsedVisual.SetActive(true);
        blockCollider.enabled = false; // 崩れたら通れるようにする(残したいなら削除)

        memoryItem.position = dropPoint.position;
        itemCollider.enabled = true;   // ここから拾える
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