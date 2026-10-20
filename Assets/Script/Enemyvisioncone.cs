using UnityEngine;

/// <summary>
/// 敵の視界を、ゲーム画面に半透明の扇形で表示する。
/// 壁(Obstacle Mask)にさえぎられる部分は、扇形が短くなる。
/// 敵の中心から少し離れた所から始まるので、猫の顔と重ならない。
/// EnemyVision / EnemyPatrol と同じオブジェクトに付ける。
/// </summary>
[RequireComponent(typeof(EnemyVision), typeof(EnemyPatrol))]
public class EnemyVisionCone : MonoBehaviour
{
    [SerializeField] private int rayCount = 30;  // 扇形の細かさ(多いほどなめらか)
    [SerializeField] private Color coneColor = new Color(1f, 0.9f, 0.2f, 0.35f);
    [SerializeField] private Material coneMaterial; // 空なら Sprites/Default を使う
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 1;  // 床より上、敵の絵より下になるよう調整する

    [Tooltip("扇形が始まる位置(敵の中心から前方へ、どれだけ離すか)。猫の顔と重ならないようにする")]
    [SerializeField] private float _startDistance = 0.5f;

    private EnemyVision vision;
    private EnemyPatrol patrol;
    private Transform coneTransform;
    private Mesh mesh;

    private Vector3[] vertices;
    private Color[] colors;
    private int[] triangles;

    private void Awake()
    {
        vision = GetComponent<EnemyVision>();
        patrol = GetComponent<EnemyPatrol>();

        rayCount = Mathf.Max(2, rayCount);

        // 表示用の子オブジェクトを作る
        var go = new GameObject("VisionCone");
        coneTransform = go.transform;
        coneTransform.SetParent(transform, false);

        var meshFilter = go.AddComponent<MeshFilter>();
        var meshRenderer = go.AddComponent<MeshRenderer>();

        mesh = new Mesh { name = "VisionConeMesh" };
        meshFilter.mesh = mesh;

        meshRenderer.material = coneMaterial != null
            ? coneMaterial
            : new Material(Shader.Find("Sprites/Default"));
        meshRenderer.sortingLayerName = sortingLayerName;
        meshRenderer.sortingOrder = sortingOrder;

        // 頂点: 光線ごとに「内側(始まり)」と「外側(先端)」の2つ。内側 = 2i、外側 = 2i+1
        int pointCount = rayCount + 1;
        vertices = new Vector3[pointCount * 2];
        colors = new Color[pointCount * 2];
        triangles = new int[rayCount * 6];

        Color rimColor = coneColor;
        rimColor.a *= 0.4f; // 外側ほど薄くする
        for (int i = 0; i < pointCount; i++)
        {
            colors[i * 2] = coneColor;
            colors[i * 2 + 1] = rimColor;
        }

        // 隣り合う光線の間を、四角形(三角形2つ)でつなぐ
        for (int i = 0; i < rayCount; i++)
        {
            int t = i * 6;
            triangles[t] = i * 2;
            triangles[t + 1] = i * 2 + 3;
            triangles[t + 2] = i * 2 + 1;
            triangles[t + 3] = i * 2;
            triangles[t + 4] = i * 2 + 2;
            triangles[t + 5] = i * 2 + 3;
        }
    }

    private void LateUpdate()
    {
        Vector2 facing = patrol.Facing;
        float baseAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        float half = vision.ViewAngle * 0.5f;

        Vector3 origin = transform.position;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = baseAngle - half + (half * 2f) * i / rayCount;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // 壁にぶつかったら、そこまでで止める
            float dist = vision.ViewDistance;
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, vision.ObstacleMask);
            if (hit.collider != null) dist = hit.distance;

            // 内側は、敵の中心から少し離した所。壁が近い時は、壁の手前まで
            float innerDist = Mathf.Min(_startDistance, dist);

            Vector3 inner = origin + (Vector3)(dir * innerDist);
            Vector3 outer = origin + (Vector3)(dir * dist);

            vertices[i * 2] = coneTransform.InverseTransformPoint(inner);
            vertices[i * 2 + 1] = coneTransform.InverseTransformPoint(outer);
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
    }
}