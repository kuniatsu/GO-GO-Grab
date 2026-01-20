using UnityEngine;

/// <summary>
/// 乗客システム
/// 個々の乗客の情報（名前、ピックアップ位置、配達先）を管理
/// ピックアップ・配達完了の判定を実行
/// </summary>
public class Passenger : MonoBehaviour
{
    [SerializeField]
    private string passengerName = "乗客";

    [SerializeField]
    private Vector3 pickupLocation;

    [SerializeField]
    private Vector3 deliveryLocation;

    [SerializeField]
    private float pickupRadius = 5f; // ピックアップ判定半径（m）

    [SerializeField]
    private float deliveryRadius = 5f; // 配達完了判定半径（m）

    private string pickupLocationName = "ピックアップ地点";
    private string deliveryLocationName = "配達先";
    private bool isPickedUp = false;
    private GameManager gameManager;

    /// <summary>
    /// 乗客を初期化
    /// </summary>
    public void Initialize(
        string passengerName,
        Vector3 pickupLocation,
        Vector3 deliveryLocation,
        float pickupRadius = 5f,
        float deliveryRadius = 5f)
    {
        this.passengerName = passengerName;
        this.pickupLocation = pickupLocation;
        this.deliveryLocation = deliveryLocation;
        this.pickupRadius = pickupRadius;
        this.deliveryRadius = deliveryRadius;

        // ロケーション名を座標から生成（将来的には住所マッピング）
        this.pickupLocationName = $"Location A ({pickupLocation.x:F0}, {pickupLocation.z:F0})";
        this.deliveryLocationName = $"Location B ({deliveryLocation.x:F0}, {deliveryLocation.z:F0})";

        isPickedUp = false;
    }

    /// <summary>
    /// ピックアップ完了判定
    /// </summary>
    public bool IsPickedUp(Vector3 bikePosition)
    {
        float distance = Vector3.Distance(bikePosition, pickupLocation);
        bool isClose = distance <= pickupRadius;

        if (isClose && !isPickedUp)
        {
            Debug.Log($"[Passenger] ピックアップ完了: {passengerName} - 距離 {distance:F1}m");
        }

        return isClose;
    }

    /// <summary>
    /// 配達完了判定
    /// </summary>
    public bool IsDelivered(Vector3 bikePosition)
    {
        if (!isPickedUp)
        {
            return false; // ピックアップ前は判定しない
        }

        float distance = Vector3.Distance(bikePosition, deliveryLocation);
        bool isClose = distance <= deliveryRadius;

        if (isClose)
        {
            Debug.Log($"[Passenger] 配達完了: {passengerName} - 距離 {distance:F1}m");
        }

        return isClose;
    }

    /// <summary>
    /// ピックアップ状態を設定
    /// </summary>
    public void SetPickedUp(bool picked)
    {
        isPickedUp = picked;

        if (isPickedUp)
        {
            // ピックアップ後、乗客表示を配達先に移動
            transform.position = deliveryLocation;
            Debug.Log($"[Passenger] {passengerName} を乗せました。配達先に移動します。");
        }
    }

    /// <summary>
    /// ピックアップ位置を取得
    /// </summary>
    public Vector3 GetPickupLocation()
    {
        return pickupLocation;
    }

    /// <summary>
    /// 配達先位置を取得
    /// </summary>
    public Vector3 GetDeliveryLocation()
    {
        return deliveryLocation;
    }

    /// <summary>
    /// ピックアップ判定半径を取得
    /// </summary>
    public float GetPickupRadius()
    {
        return pickupRadius;
    }

    /// <summary>
    /// 配達判定半径を取得
    /// </summary>
    public float GetDeliveryRadius()
    {
        return deliveryRadius;
    }

    /// <summary>
    /// 乗客名を取得
    /// </summary>
    public string GetPassengerName()
    {
        return passengerName;
    }

    /// <summary>
    /// ピックアップ位置名を取得
    /// </summary>
    public string GetPickupLocationName()
    {
        return pickupLocationName;
    }

    /// <summary>
    /// 配達先位置名を取得
    /// </summary>
    public string GetDeliveryLocationName()
    {
        return deliveryLocationName;
    }

    /// <summary>
    /// GameManager を設定
    /// </summary>
    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }

    /// <summary>
    /// ピックアップ状態を確認
    /// </summary>
    public bool IsPickedUpState()
    {
        return isPickedUp;
    }

    /// <summary>
    /// Gizmos でピックアップ・配達範囲を可視化（デバッグ用）
    /// </summary>
    private void OnDrawGizmos()
    {
        // ピックアップ位置（黄色）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pickupLocation, pickupRadius);

        // 配達先位置（青色）
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(deliveryLocation, deliveryRadius);

        // 位置を結ぶ線（白色）
        Gizmos.color = Color.white;
        Gizmos.DrawLine(pickupLocation, deliveryLocation);
    }
}
