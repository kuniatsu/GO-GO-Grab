using UnityEngine;

/// <summary>
/// カメラ追従システム
/// バイクを中心としたカメラ位置を管理
/// Cinemachine を使用しない場合のシンプル実装
/// </summary>
public class CameraFollower : MonoBehaviour
{
    [Header("追従対象")]
    [SerializeField]
    private Transform bikeTransform;

    [Header("カメラ設定")]
    [SerializeField]
    private Vector3 cameraOffset = new Vector3(0, 4f, -8f); // バイクからのオフセット

    [SerializeField]
    private float followSpeed = 5f; // 追従速度（スムージング）

    [SerializeField]
    private float lookAheadDistance = 5f; // 前方を見る距離

    [SerializeField]
    private bool enableLookAhead = true; // 前方を見る機能を有効化

    [Header("カメラ揺れ（オプション）")]
    [SerializeField]
    private bool enableCameraShake = false;

    [SerializeField]
    private float cameraShakeAmount = 0.2f; // 揺れの大きさ

    // 内部変数
    private Camera mainCamera;
    private Vector3 currentOffset;
    private BikeController bikeController;
    private float cameraShakeNoise = 0f;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("CameraFollower: Camera コンポーネントが見つかりません");
            enabled = false;
            return;
        }

        // バイク対象の自動取得
        if (bikeTransform == null)
        {
            BikeController bike = FindObjectOfType<BikeController>();
            if (bike != null)
            {
                bikeTransform = bike.transform;
                bikeController = bike;
            }
        }
        else
        {
            bikeController = bikeTransform.GetComponent<BikeController>();
        }

        if (bikeTransform == null)
        {
            Debug.LogError("CameraFollower: バイク(BikeController)が見つかりません");
            enabled = false;
            return;
        }

        currentOffset = cameraOffset;
    }

    private void LateUpdate()
    {
        if (bikeTransform == null)
        {
            return;
        }

        UpdateCameraPosition();
        UpdateCameraRotation();
    }

    /// <summary>
    /// カメラの位置を更新
    /// </summary>
    private void UpdateCameraPosition()
    {
        // 目標位置を計算（バイクの向きに基づいてオフセットを回転）
        Vector3 rotatedOffset = bikeTransform.TransformDirection(cameraOffset);
        Vector3 targetPosition = bikeTransform.position + rotatedOffset;

        // カメラ揺れを追加
        if (enableCameraShake && bikeController != null && bikeController.IsMoving())
        {
            cameraShakeNoise += Time.deltaTime * 5f;
            Vector3 shake = new Vector3(
                Mathf.PerlinNoise(cameraShakeNoise, 0) - 0.5f,
                Mathf.PerlinNoise(0, cameraShakeNoise) - 0.5f,
                0
            ) * cameraShakeAmount;
            targetPosition += shake;
        }

        // スムーズにカメラを移動
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// カメラの回転を更新
    /// </summary>
    private void UpdateCameraRotation()
    {
        Vector3 lookDirection = bikeTransform.position - transform.position;

        if (enableLookAhead && bikeController != null)
        {
            // 前方を見るオプション
            Vector3 bikeForward = bikeTransform.forward * lookAheadDistance;
            Vector3 lookTarget = bikeTransform.position + bikeForward;
            lookDirection = lookTarget - transform.position;
        }

        // カメラの回転を計算
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        // スムーズに回転
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            followSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// カメラオフセットを設定（ズームアウト時など）
    /// </summary>
    public void SetCameraOffset(Vector3 offset)
    {
        cameraOffset = offset;
    }

    /// <summary>
    /// 追従速度を設定
    /// </summary>
    public void SetFollowSpeed(float speed)
    {
        followSpeed = speed;
    }

    /// <summary>
    /// 前方を見る機能を有効化/無効化
    /// </summary>
    public void SetLookAheadEnabled(bool enabled)
    {
        enableLookAhead = enabled;
    }

    /// <summary>
    /// カメラ揺れを有効化/無効化
    /// </summary>
    public void SetCameraShakeEnabled(bool enabled)
    {
        enableCameraShake = enabled;
    }
}
