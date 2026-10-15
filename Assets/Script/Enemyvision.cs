using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(EnemyPatrol))]
public class EnemyVision : MonoBehaviour
{
    [SerializeField] private float viewDistance = 4f;                 // 見える距離
    [SerializeField, Range(10f, 180f)] private float viewAngle = 90f; // 視界の広さ(全体の角度)
    [SerializeField] private LayerMask obstacleMask;                  // 視界をさえぎるもの(壁など)のレイヤー
    [SerializeField] private float restartDelay = 1f;                 // 見つかってからやり直すまでの秒数
    [SerializeField] private GameObject alertMark;                    // 見つかった時に出す「!」(敵の頭の上に置く)

    private EnemyPatrol patrol;
    private PlayerController player;
    private bool caught = false;

    // 視界の表示(EnemyVisionCone)が読むための値
    public float ViewDistance => viewDistance;
    public float ViewAngle => viewAngle;
    public LayerMask ObstacleMask => obstacleMask;

    private void Start()
    {
        patrol = GetComponent<EnemyPatrol>();
        player = FindFirstObjectByType<PlayerController>();

        if (alertMark != null) alertMark.SetActive(false);
    }

    private void Update()
    {
        if (caught || player == null) return;

        if (CanSeePlayer())
        {
            OnCaught();
        }
    }

    private bool CanSeePlayer()
    {
        // 隠れていたら見えない
        if (player.IsHidden()) return false;

        Vector2 toPlayer = (Vector2)player.transform.position - (Vector2)transform.position;
        float dist = toPlayer.magnitude;

        // 遠すぎる
        if (dist > viewDistance) return false;

        // 向いている方向から外れている
        if (Vector2.Angle(patrol.Facing, toPlayer) > viewAngle * 0.5f) return false;

        // 間に壁があれば見えない
        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, dist, obstacleMask);
        return hit.collider == null;
    }

    private void OnCaught()
    {
        caught = true;
        Debug.Log("見つかった!");

        patrol.enabled = false; // 敵を止める
        player.Freeze();        // プレイヤーも動けなくする

        // 「!」をポンと出す
        if (alertMark != null)
        {
            alertMark.SetActive(true);
            StartCoroutine(PopMark());
        }

        Invoke(nameof(Restart), restartDelay);
    }

    // 「!」が少し大きくなってから、元の大きさに落ち着く動き
    private IEnumerator PopMark()
    {
        Transform t = alertMark.transform;
        Vector3 targetScale = t.localScale;
        const float duration = 0.2f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float k = Mathf.Clamp01(time / duration);
            float scale = k < 0.7f
                ? Mathf.Lerp(0f, 1.3f, k / 0.7f)
                : Mathf.Lerp(1.3f, 1f, (k - 0.7f) / 0.3f);

            t.localScale = targetScale * scale;
            yield return null;
        }

        t.localScale = targetScale;
    }

    private void Restart()
    {
        SceneFader.ReloadCurrent();
    }

    // シーンビューで視界を確認するための線(敵を選択している時だけ表示)
    private void OnDrawGizmosSelected()
    {
        Vector2 facing = (Application.isPlaying && patrol != null) ? patrol.Facing : Vector2.down;
        float half = viewAngle * 0.5f;

        Vector3 origin = transform.position;
        Vector3 left = Quaternion.Euler(0f, 0f, half) * (Vector3)facing * viewDistance;
        Vector3 right = Quaternion.Euler(0f, 0f, -half) * (Vector3)facing * viewDistance;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + left);
        Gizmos.DrawLine(origin, origin + right);
        Gizmos.DrawLine(origin + left, origin + right);
    }
}