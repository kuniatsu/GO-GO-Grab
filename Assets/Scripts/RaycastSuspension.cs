using UnityEngine;

/// <summary>
/// Raycast Suspension システム
/// バイク下部からRayを飛ばし、透明な Road Plane との距離を計測
/// 車体の浮遊高さを調整し、ガタガタや貫通を防止
/// </summary>
public class RaycastSuspension : MonoBehaviour
{
    [Header("Suspension パラメータ")]
    [SerializeField]
    private float suspensionHeight = 0.5f; // 床からの目標高さ (m)

    [SerializeField]
    private float suspensionForce = 30f; // サスペンション力（N）

    [SerializeField]
    private float suspensionDamping = 2f; // 減衰係数

    [SerializeField]
    private LayerMask roadLayer; // Road レイヤーマスク

    [Header("Raycast 設定")]
    [SerializeField]
    private float raycastDistance = 2f; // Rayの最大距離 (m)

    [SerializeField]
    private int rayCount = 4; // Rayの数

    [SerializeField]
    private float raycastRadius = 0.5f; // Rayの配置半径 (m)

    // 内部変数
    private Rigidbody bikeRigidbody;
    private float previousHeight = 0f; // 前フレームの高さ（速度計算用）

    private void Start()
    {
        bikeRigidbody = GetComponent<Rigidbody>();
        if (bikeRigidbody == null)
        {
            Debug.LogError("RaycastSuspension: Rigidbody が見つかりません");
            enabled = false;
            return;
        }

        // Road レイヤーが未設定の場合、デフォルト設定
        if (roadLayer == 0)
        {
            roadLayer = LayerMask.GetMask("Road");
        }

        previousHeight = transform.position.y;
    }

    private void FixedUpdate()
    {
        ApplySuspensionForce();
    }

    /// <summary>
    /// Raycast を使ってサスペンション力を適用
    /// </summary>
    private void ApplySuspensionForce()
    {
        // 複数の Raycast ポイントから距離を計測
        float averageDistance = GetAverageRaycastDistance();

        if (averageDistance < 0)
        {
            // Ray が何も当たらなかった場合はスキップ
            return;
        }

        // 目標高さとの差分
        float distanceError = suspensionHeight - averageDistance;

        // 高さの速度（前フレームとの差分）
        float heightVelocity = (transform.position.y - previousHeight) / Time.fixedDeltaTime;
        previousHeight = transform.position.y;

        // サスペンション力を計算（バネ・ダンパー模型）
        float suspensionForceY = (distanceError * suspensionForce) - (heightVelocity * suspensionDamping);

        // 力を適用
        bikeRigidbody.AddForce(Vector3.up * suspensionForceY, ForceMode.Force);

        // デバッグ用の情報ログ
        #if UNITY_EDITOR
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log($"[Suspension] Distance: {averageDistance:F2}m, Error: {distanceError:F2}m, Force: {suspensionForceY:F1}N");
        }
        #endif
    }

    /// <summary>
    /// 複数の Raycast ポイントから平均距離を計測
    /// </summary>
    private float GetAverageRaycastDistance()
    {
        float totalDistance = 0f;
        int hitCount = 0;

        // 円形に配置された複数の Raycast ポイント
        for (int i = 0; i < rayCount; i++)
        {
            float angle = (i / (float)rayCount) * 360f * Mathf.Deg2Rad;
            Vector3 rayOffset = new Vector3(
                Mathf.Cos(angle) * raycastRadius,
                0,
                Mathf.Sin(angle) * raycastRadius
            );

            Vector3 rayStartPos = transform.position + rayOffset;
            Vector3 rayDirection = Vector3.down;

            RaycastHit hit;
            if (Physics.Raycast(rayStartPos, rayDirection, out hit, raycastDistance, roadLayer))
            {
                // 床までの距離を計測
                float distance = hit.distance;
                totalDistance += distance;
                hitCount++;

                // デバッグ用の Raycast ビジュアライズ
                #if UNITY_EDITOR
                Debug.DrawLine(rayStartPos, hit.point, Color.green, Time.fixedDeltaTime);
                #endif
            }
            else
            {
                // Ray が Road に当たらなかった場合も可視化
                #if UNITY_EDITOR
                Debug.DrawLine(rayStartPos, rayStartPos + rayDirection * raycastDistance, Color.red, Time.fixedDeltaTime);
                #endif
            }
        }

        // 平均距離を返す
        if (hitCount > 0)
        {
            return totalDistance / hitCount;
        }

        return -1f; // ヒットなし
    }

    /// <summary>
    /// Raycast ポイントの可視化（デバッグ用）
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!enabled || Application.isPlaying == false)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < rayCount; i++)
        {
            float angle = (i / (float)rayCount) * 360f * Mathf.Deg2Rad;
            Vector3 rayOffset = new Vector3(
                Mathf.Cos(angle) * raycastRadius,
                0,
                Mathf.Sin(angle) * raycastRadius
            );

            Gizmos.DrawSphere(transform.position + rayOffset, 0.1f);
        }

        // サスペンション目標高さを表示
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            transform.position + Vector3.down * suspensionHeight,
            transform.position + Vector3.down * suspensionHeight + Vector3.right * 0.5f
        );
    }

    /// <summary>
    /// サスペンション高さを設定
    /// </summary>
    public void SetSuspensionHeight(float height)
    {
        suspensionHeight = height;
    }

    /// <summary>
    /// サスペンション力を設定
    /// </summary>
    public void SetSuspensionForce(float force)
    {
        suspensionForce = force;
    }
}
