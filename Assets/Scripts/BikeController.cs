using UnityEngine;

/// <summary>
/// バイクのコントローラースクリプト
/// Arcade Styleの移動・回転・エリア制限を管理
/// </summary>
public class BikeController : MonoBehaviour
{
    [Header("移動パラメータ")]
    [SerializeField]
    private float maxSpeed = 30f; // 最大速度 (m/s)

    [SerializeField]
    private float acceleration = 50f; // 加速度 (m/s²)

    [SerializeField]
    private float deceleration = 30f; // 減速度 (m/s²)

    [SerializeField]
    private float turnSpeed = 180f; // 旋回速度 (度/s)

    [SerializeField]
    private float driftFriction = 0.95f; // ドリフト時の摩擦係数

    [Header("物理パラメータ")]
    [SerializeField]
    private float bikeRadius = 1f; // バイクの衝突半径

    [SerializeField]
    private float mass = 1000f; // バイクの質量 (kg)

    [Header("エリア制限")]
    [SerializeField]
    private BoundaryManager boundaryManager;

    // 内部変数
    private Rigidbody bikeRigidbody;
    private float currentSpeed = 0f; // 現在の速度 (m/s)
    private float inputForward = 0f; // 前後入力
    private float inputTurn = 0f; // 左右入力

    private void Start()
    {
        // Rigidbody の初期化
        bikeRigidbody = GetComponent<Rigidbody>();
        if (bikeRigidbody == null)
        {
            Debug.LogError("BikeController: Rigidbody が見つかりません");
            enabled = false;
            return;
        }

        // BoundaryManager の自動取得
        if (boundaryManager == null)
        {
            boundaryManager = FindObjectOfType<BoundaryManager>();
        }

        if (boundaryManager == null)
        {
            Debug.LogWarning("BikeController: BoundaryManager が見つかりません。エリア制限は機能しません。");
        }

        // 物理設定
        bikeRigidbody.mass = mass;
        bikeRigidbody.drag = 0.1f; // 空気抵抗
        bikeRigidbody.angularDrag = 2f; // 回転抵抗

        // Z軸（ロール軸）の回転を制限（転倒しない）
        RigidbodyConstraints constraints = bikeRigidbody.constraints;
        constraints |= RigidbodyConstraints.FreezeRotationZ;
        bikeRigidbody.constraints = constraints;
    }

    private void Update()
    {
        // 入力処理
        HandleInput();
    }

    private void FixedUpdate()
    {
        // 物理更新
        UpdateSpeed();
        UpdatePosition();
        UpdateRotation();
        HandleBoundaryCollision();
    }

    /// <summary>
    /// キーボード入力を処理
    /// </summary>
    private void HandleInput()
    {
        // 前後入力 (W/Up = 前, S/Down = 後ろ)
        inputForward = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            inputForward = 1f;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            inputForward = -0.5f; // 後ろは遅い
        }

        // 左右入力 (A/Left = 左, D/Right = 右)
        inputTurn = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            inputTurn = -1f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            inputTurn = 1f;
        }
    }

    /// <summary>
    /// 速度を更新（加速・減速）
    /// </summary>
    private void UpdateSpeed()
    {
        float targetSpeed = inputForward * maxSpeed;

        if (inputForward > 0)
        {
            // 加速
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime / maxSpeed);
        }
        else if (inputForward < 0)
        {
            // 後ろに移動
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime / maxSpeed);
        }
        else
        {
            // 減速（入力なし）
            currentSpeed = Mathf.Lerp(currentSpeed, 0, deceleration * Time.fixedDeltaTime / maxSpeed);
        }

        // 速度をクリップ
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * 0.5f, maxSpeed);
    }

    /// <summary>
    /// 位置を更新（前後方向の移動）
    /// </summary>
    private void UpdatePosition()
    {
        if (Mathf.Abs(currentSpeed) < 0.1f)
        {
            return; // 速度が小さい場合はスキップ
        }

        // バイクの向いている方向に移動
        Vector3 moveDirection = transform.forward * currentSpeed;
        Vector3 newPosition = bikeRigidbody.position + moveDirection * Time.fixedDeltaTime;

        // 試みた位置がエリア内かを確認
        newPosition = boundaryManager != null
            ? boundaryManager.HandleBoundaryCollision(bikeRigidbody.position, newPosition, bikeRadius)
            : MapConfig.ClampToBoundary(newPosition);

        bikeRigidbody.velocity = (newPosition - bikeRigidbody.position) / Time.fixedDeltaTime;
    }

    /// <summary>
    /// 回転を更新（左右の旋回）
    /// </summary>
    private void UpdateRotation()
    {
        if (Mathf.Abs(inputTurn) < 0.01f)
        {
            return; // 入力がない場合はスキップ
        }

        // 現在の速度に応じて旋回速度を調整（低速時は旋回できない）
        float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / (maxSpeed * 0.3f));
        float effectiveTurnSpeed = turnSpeed * inputTurn * speedFactor;

        // Y軸周りに回転
        Vector3 rotation = transform.eulerAngles;
        rotation.y += effectiveTurnSpeed * Time.fixedDeltaTime;
        transform.eulerAngles = rotation;
    }

    /// <summary>
    /// エリア境界との衝突を処理
    /// </summary>
    private void HandleBoundaryCollision()
    {
        if (boundaryManager == null)
        {
            return;
        }

        // エリア外にいないか確認
        if (!boundaryManager.IsWithinBoundary(transform.position))
        {
            // エリア内にクランプ
            Vector3 clampedPos = boundaryManager.ClampToBoundary(transform.position);
            bikeRigidbody.position = clampedPos;

            // 境界に当たった時の速度を減速
            currentSpeed *= 0.7f;
        }
    }

    /// <summary>
    /// 現在の速度を取得
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    /// <summary>
    /// 現在のローカル速度を取得
    /// </summary>
    public Vector3 GetVelocity()
    {
        return bikeRigidbody.velocity;
    }

    /// <summary>
    /// バイクが移動中かを判定
    /// </summary>
    public bool IsMoving()
    {
        return Mathf.Abs(currentSpeed) > 0.5f;
    }
}
