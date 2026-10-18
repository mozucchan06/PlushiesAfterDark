using UnityEngine;

/// <summary>
/// 夜空の背景。濃い青のグラデーションと、またたく星を、画像なしで作る。
/// 空のオブジェクトに付けるだけで動く。このオブジェクトの位置が、夜空の中心になる。
/// </summary>
public class NightSky : MonoBehaviour
{
    [Header("範囲(カメラに映る大きさより、少し大きくする)")]
    [SerializeField] private Vector2 areaSize = new Vector2(20f, 12f);

    [Header("空の色(上から下へのグラデーション)")]
    [SerializeField] private bool showGradient = true;
    [SerializeField] private Color topColor = new Color(0.02f, 0.03f, 0.12f);    // 上:ほぼ黒に近い紺
    [SerializeField] private Color bottomColor = new Color(0.10f, 0.15f, 0.38f); // 下:明るめの青

    [Header("星")]
    [SerializeField] private int starCount = 80;
    [SerializeField] private Vector2 starSizeRange = new Vector2(0.04f, 0.16f);          // 星の大きさ(最小, 最大)
    [SerializeField] private Vector2 twinkleSpeedRange = new Vector2(0.8f, 2.5f);        // またたく速さ(最小, 最大)
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.2f;                       // いちばん暗い時の明るさ

    [Header("描画順(部屋の絵より小さい値にして、後ろに回す)")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int skySortingOrder = -100;

    private SpriteRenderer[] starRenderers;
    private Color[] baseColors;
    private Vector3[] baseScales;
    private float[] speeds;
    private float[] phases;

    private void Awake()
    {
        if (showGradient) CreateGradient();
        CreateStars();
    }

    private void Update()
    {
        for (int i = 0; i < starRenderers.Length; i++)
        {
            // 0?1 をなめらかに行き来する値
            float k = (Mathf.Sin(Time.time * speeds[i] + phases[i]) + 1f) * 0.5f;

            Color c = baseColors[i];
            c.a = Mathf.Lerp(minAlpha, 1f, k);
            starRenderers[i].color = c;

            // 明るい時は、ほんの少し大きくする
            starRenderers[i].transform.localScale = baseScales[i] * Mathf.Lerp(0.8f, 1.1f, k);
        }
    }

    // ---------- 空のグラデーション ----------

    private void CreateGradient()
    {
        const int height = 64;
        var texture = new Texture2D(4, height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        for (int y = 0; y < height; y++)
        {
            // y=0 が下、y=height-1 が上
            Color c = Color.Lerp(bottomColor, topColor, y / (float)(height - 1));
            for (int x = 0; x < 4; x++) texture.SetPixel(x, y, c);
        }
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 4, height), new Vector2(0.5f, 0.5f), height);

        var go = new GameObject("SkyGradient");
        go.transform.SetParent(transform, false);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = skySortingOrder;

        // 指定した範囲いっぱいに引き伸ばす
        Vector2 size = sprite.bounds.size;
        go.transform.localScale = new Vector3(areaSize.x / size.x, areaSize.y / size.y, 1f);
    }

    // ---------- 星 ----------

    private void CreateStars()
    {
        Sprite starSprite = CreateStarSprite();

        starRenderers = new SpriteRenderer[starCount];
        baseColors = new Color[starCount];
        baseScales = new Vector3[starCount];
        speeds = new float[starCount];
        phases = new float[starCount];

        for (int i = 0; i < starCount; i++)
        {
            var go = new GameObject("Star");
            go.transform.SetParent(transform, false);

            // 範囲の中のランダムな位置
            go.transform.localPosition = new Vector3(
                Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f),
                0f);

            float size = Random.Range(starSizeRange.x, starSizeRange.y);
            baseScales[i] = new Vector3(size, size, 1f);
            go.transform.localScale = baseScales[i];

            // 白?ほんのり黄色?ほんのり水色
            baseColors[i] = Color.Lerp(new Color(1f, 1f, 0.85f), new Color(0.8f, 0.9f, 1f), Random.value);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = starSprite;
            sr.color = baseColors[i];
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = skySortingOrder + 1; // グラデーションの手前

            starRenderers[i] = sr;
            speeds[i] = Random.Range(twinkleSpeedRange.x, twinkleSpeedRange.y);
            phases[i] = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    // 中心が白く、ふちがぼんやり消える、丸い光の絵を作る
    private Sprite CreateStarSprite()
    {
        const int size = 16;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        float center = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                float alpha = Mathf.Clamp01(1f - dist);
                alpha *= alpha; // ふちをなめらかに消す
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply();

        // 1ユニットの大きさの絵になる(スケールで星の大きさを決める)
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // シーンビューで範囲を確認するための枠(このオブジェクトを選択している時だけ表示)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.6f, 1f);
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 0f));
    }
}