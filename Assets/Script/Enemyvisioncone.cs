using UnityEngine;

/// <summary>
/// 敵の視界を、ゲーム画面に半透明の扇形で表示する。
/// 壁(Obstacle Mask)にさえぎられる部分は、扇形が短くなる。
/// EnemyVision / EnemyPatrol と同じオブジェクトに付ける。
/// </summary>
[RequireComponent(typeof(EnemyVision), typeof(EnemyPatrol))]
public class EnemyVisionCone : MonoBehaviour
{
    [SerializeField] private int rayCount = 30;  // 扇形の細かさ(多いほどなめらか)
    [SerializeField] private Color coneColor = new Color(1f, 0.9f, 0.2f, 0.35f);
    [SerializeField] private Material coneMaterial; // 空なら Sprites/Default を使う
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 1;  // 床より上、プレイヤーより下になるよう調整する

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

        // 頂点: 先頭が敵の位置、残りが扇形の外側
        vertices = new Vector3[rayCount + 2];
        colors = new Color[rayCount + 2];
        triangles = new int[rayCount * 3];

        Color rimColor = coneColor;
        rimColor.a *= 0.4f; // 外側ほど薄くする
        colors[0] = coneColor;
        for (int i = 1; i < colors.Length; i++) colors[i] = rimColor;

        for (int i = 0; i < rayCount; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 2;
            triangles[i * 3 + 2] = i + 1;
        }
    }

    private void LateUpdate()
    {
        Vector2 facing = patrol.Facing;
        float baseAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        float half = vision.ViewAngle * 0.5f;

        Vector3 origin = transform.position;
        vertices[0] = coneTransform.InverseTransformPoint(origin);

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = baseAngle - half + (half * 2f) * i / rayCount;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // 壁にぶつかったら、そこまでで止める
            float dist = vision.ViewDistance;
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, vision.ObstacleMask);
            if (hit.collider != null) dist = hit.distance;

            Vector3 world = origin + (Vector3)(dir * dist);
            vertices[i + 1] = coneTransform.InverseTransformPoint(world);
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
    }
}