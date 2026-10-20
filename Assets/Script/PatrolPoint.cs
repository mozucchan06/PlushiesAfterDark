using UnityEngine;

/// <summary>
/// 巡回ポイントに付ける。敵がこの地点に着いた時に向く方角と、待ち時間を指定できる。
/// 付けなくても巡回はできる(その場合は、直前の進行方向のまま止まる)。
/// </summary>
public class PatrolPoint : MonoBehaviour
{
    public enum LookDirection { Keep, Up, Down, Left, Right }

    [SerializeField] private LookDirection lookDirection = LookDirection.Keep; // Keep = 向きを変えない
    [SerializeField] private float waitTime = -1f; // マイナスなら、敵側の標準の待ち時間を使う

    public float WaitTime => waitTime;

    // 向く方角を取得する。Keep の時は false
    public bool TryGetLookVector(out Vector2 look)
    {
        switch (lookDirection)
        {
            case LookDirection.Up: look = Vector2.up; return true;
            case LookDirection.Down: look = Vector2.down; return true;
            case LookDirection.Left: look = Vector2.left; return true;
            case LookDirection.Right: look = Vector2.right; return true;
            default: look = Vector2.zero; return false;
        }
    }

    // シーンビューで、地点と向く方角が分かるようにする
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.15f);

        if (TryGetLookVector(out Vector2 look))
        {
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(look * 0.8f));
        }
    }
}