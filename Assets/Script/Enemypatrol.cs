using UnityEngine;

/// <summary>
/// 決まった地点(waypoints)を順番に巡回する敵。
/// 最後の地点に着いたら最初に戻る。視界判定は別のスクリプトで作る。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;        // 巡回する地点(敵の子にしない)
    [SerializeField] private float speed = 1.5f;           // 歩く速さ
    [SerializeField] private float waitTime = 1f;          // 各地点で止まる秒数
    [SerializeField] private float arriveDistance = 0.05f; // この距離以内なら「着いた」とみなす

    private Rigidbody2D rb;
    private int targetIndex = 0;
    private float waitTimer = 0f;
    private Vector2 facing = Vector2.down; // 今向いている方向(視界判定で使う)

    public Vector2 Facing => facing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // 地点で待っている間は動かない
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 target = waypoints[targetIndex].position;
        Vector2 toTarget = target - rb.position;

        // 着いたら次の地点へ
        if (toTarget.magnitude <= arriveDistance)
        {
            targetIndex = (targetIndex + 1) % waypoints.Length;
            waitTimer = waitTime;
            return;
        }

        facing = toTarget.normalized;
        rb.MovePosition(Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime));
    }
}