using UnityEngine;

/// <summary>
/// エリア制限を管理するスクリプト
/// バイクが3ブロック（半径300m）の範囲外に出ないようにコントロール
/// 透明な壁として機能し、バイクが衝突したら跳ね返す
/// </summary>
public class BoundaryManager : MonoBehaviour
{
    [SerializeField]
    private float boundaryRadius = MapConfig.BOUNDARY_RADIUS_METERS;

    [SerializeField]
    private Material boundaryWallMaterial;

    [SerializeField]
    private bool visualizeBoundary = true;

    private GameObject boundaryWallGameObject;

    private void Start()
    {
        if (visualizeBoundary)
        {
            CreateBoundaryWall();
        }
    }

    /// <summary>
    /// 境界の可視化用ウォールを作成（エディタでのデバッグ用）
    /// </summary>
    private void CreateBoundaryWall()
    {
        boundaryWallGameObject = new GameObject("BoundaryWall");
        boundaryWallGameObject.transform.parent = transform;
        boundaryWallGameObject.transform.localPosition = Vector3.zero;

        // トーラス型の壁をメッシュで描画
        MeshFilter meshFilter = boundaryWallGameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = boundaryWallGameObject.AddComponent<MeshRenderer>();

        // 単純な円柱型の壁として表現
        Mesh wallMesh = CreateCylinderMesh(boundaryRadius, 10f);
        meshFilter.mesh = wallMesh;

        if (boundaryWallMaterial != null)
        {
            meshRenderer.material = boundaryWallMaterial;
        }

        // 透明な壁として見えるように設定
        meshRenderer.enabled = visualizeBoundary;
    }

    /// <summary>
    /// 円柱型メッシュを作成（簡易版）
    /// </summary>
    private Mesh CreateCylinderMesh(float radius, float height)
    {
        Mesh mesh = new Mesh();
        int segments = 64;
        Vector3[] vertices = new Vector3[segments * 2];
        int[] triangles = new int[segments * 6];

        // 上下の円を作成
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * 360f * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            // 下部
            vertices[i] = new Vector3(x, 0, z);
            // 上部
            vertices[segments + i] = new Vector3(x, height, z);
        }

        // 三角形を作成
        int triIndex = 0;
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;

            // 側面
            triangles[triIndex++] = i;
            triangles[triIndex++] = segments + i;
            triangles[triIndex++] = next;

            triangles[triIndex++] = next;
            triangles[triIndex++] = segments + i;
            triangles[triIndex++] = segments + next;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// ゲームオブジェクトが境界内にあるかを確認
    /// </summary>
    public bool IsWithinBoundary(Vector3 position)
    {
        return MapConfig.IsWithinBoundary(position);
    }

    /// <summary>
    /// ゲームオブジェクトを境界内にクランプ
    /// </summary>
    public Vector3 ClampToBoundary(Vector3 position)
    {
        return MapConfig.ClampToBoundary(position);
    }

    /// <summary>
    /// 境界への衝突を処理（バイクが壁に当たった時）
    /// </summary>
    public Vector3 HandleBoundaryCollision(Vector3 currentPosition, Vector3 attemptedPosition, float objectRadius = 1f)
    {
        // 試みた位置がエリア外の場合
        if (!IsWithinBoundary(attemptedPosition))
        {
            // 現在位置と試みた位置を結ぶ線と境界円の交点を計算
            Vector2 current2D = new Vector2(currentPosition.x, currentPosition.z);
            Vector2 attempted2D = new Vector2(attemptedPosition.x, attemptedPosition.z);
            Vector2 direction = (attempted2D - current2D).normalized;

            // 境界内での安全な位置を計算
            Vector2 safePos = current2D + direction * (boundaryRadius - objectRadius);
            return new Vector3(safePos.x, attemptedPosition.y, safePos.y);
        }

        return attemptedPosition;
    }
}
