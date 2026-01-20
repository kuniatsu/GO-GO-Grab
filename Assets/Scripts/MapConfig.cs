using UnityEngine;

/// <summary>
/// マップ座標とエリア制限の設定を管理するスクリプト
/// ベンタイン市場を中心として、3ブロック分（半径300m）の範囲を定義
/// </summary>
public class MapConfig : MonoBehaviour
{
    // ベンタイン市場の座標
    public static readonly double CENTER_LATITUDE = 10.7725;
    public static readonly double CENTER_LONGITUDE = 106.6980;

    // エリア制限（半径：メートル）
    public static readonly float BOUNDARY_RADIUS_METERS = 300f;

    // Cesium用の座標系変換定数
    // WGS84座標をUnity上のメートル単位に変換する際の基準値
    private static readonly double METERS_PER_DEGREE_LATITUDE = 111319.5;
    private static readonly double METERS_PER_DEGREE_LONGITUDE = 40075017.0 / 360.0;

    /// <summary>
    /// 緯度・経度をメートル単位のローカル座標に変換
    /// 中心座標（ベンタイン市場）を原点（0, 0）とする
    /// </summary>
    public static Vector2 GeoToLocalPosition(double latitude, double longitude)
    {
        double latOffset = (latitude - CENTER_LATITUDE) * METERS_PER_DEGREE_LATITUDE;
        double lonOffset = (longitude - CENTER_LONGITUDE) * METERS_PER_DEGREE_LONGITUDE * Mathf.Cos((float)CENTER_LATITUDE * Mathf.Deg2Rad);

        return new Vector2((float)lonOffset, (float)latOffset);
    }

    /// <summary>
    /// ローカル座標を緯度・経度に逆変換
    /// </summary>
    public static (double latitude, double longitude) LocalPositionToGeo(Vector2 localPosition)
    {
        double latOffset = localPosition.y / METERS_PER_DEGREE_LATITUDE;
        double lonOffset = localPosition.x / (METERS_PER_DEGREE_LONGITUDE * Mathf.Cos((float)CENTER_LATITUDE * Mathf.Deg2Rad));

        return (CENTER_LATITUDE + latOffset, CENTER_LONGITUDE + lonOffset);
    }

    /// <summary>
    /// 指定座標がエリア内かどうかを判定
    /// </summary>
    public static bool IsWithinBoundary(Vector3 position)
    {
        Vector2 localPos = new Vector2(position.x, position.z);
        return localPos.magnitude <= BOUNDARY_RADIUS_METERS;
    }

    /// <summary>
    /// 指定座標がエリア外の場合、境界内にクランプ
    /// </summary>
    public static Vector3 ClampToBoundary(Vector3 position)
    {
        Vector2 localPos = new Vector2(position.x, position.z);
        float distance = localPos.magnitude;

        if (distance <= BOUNDARY_RADIUS_METERS)
        {
            return position;
        }

        // 境界内にクランプ
        Vector2 clampedPos = localPos.normalized * BOUNDARY_RADIUS_METERS;
        return new Vector3(clampedPos.x, position.y, clampedPos.y);
    }
}
