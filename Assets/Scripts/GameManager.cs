using UnityEngine;

/// <summary>
/// ゲーム全体のロジック管理
/// ゲームステート（Idle, Pickup, Delivery, Result）を管理
/// 客の生成、配置、検知ロジックを統括
/// </summary>
public class GameManager : MonoBehaviour
{
    // ゲームステート定義
    public enum GameState
    {
        Idle,       // 待機中・営業中
        Pickup,     // 乗客ピックアップ中
        Delivery,   // 配達中
        Result      // 結果表示
    }

    [Header("ゲーム設定")]
    [SerializeField]
    private float pickupTimeLimit = 60f; // ピックアップ制限時間（秒）

    [SerializeField]
    private float deliveryTimeLimit = 120f; // 配達制限時間（秒）

    [SerializeField]
    private Vector2 minMaxPassengers = new Vector2(2, 2); // 客の数（最小、最大）

    [Header("スポーン設定")]
    [SerializeField]
    private float spawnRadius = 250f; // スポーン範囲の半径（m）

    [SerializeField]
    private int maxPassengersPerGame = 2; // 1ゲーム中の最大客数

    [Header("参照")]
    [SerializeField]
    private BikeController bikeController;

    [SerializeField]
    private BoundaryManager boundaryManager;

    [SerializeField]
    private UIManager uiManager;

    // 内部変数
    private GameState currentState = GameState.Idle;
    private Passenger currentPassenger;
    private float timeRemaining;
    private int totalScore = 0;
    private int passengersCompleted = 0;

    private void Start()
    {
        // コンポーネント自動取得
        if (bikeController == null)
        {
            bikeController = FindObjectOfType<BikeController>();
        }

        if (boundaryManager == null)
        {
            boundaryManager = FindObjectOfType<BoundaryManager>();
        }

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        // ゲーム初期化
        EnterIdleState();
    }

    private void Update()
    {
        // ゲームステートに応じた処理
        switch (currentState)
        {
            case GameState.Pickup:
                UpdatePickupState();
                break;
            case GameState.Delivery:
                UpdateDeliveryState();
                break;
        }

        // UI 更新
        if (uiManager != null)
        {
            uiManager.UpdateTimer(timeRemaining);
            uiManager.UpdateScore(totalScore);
            uiManager.UpdateState(currentState.ToString());
        }
    }

    /// <summary>
    /// Idle ステートに入る（待機状態）
    /// </summary>
    public void EnterIdleState()
    {
        currentState = GameState.Idle;
        currentPassenger = null;
        timeRemaining = 0f;

        // UI 更新
        if (uiManager != null)
        {
            uiManager.ShowNotification("営業中...", "客を探しています");
        }

        // ランダムに客を配置
        if (passengersCompleted < (int)minMaxPassengers.y)
        {
            Invoke(nameof(GeneratePassenger), Random.Range(2f, 5f));
        }
    }

    /// <summary>
    /// Pickup ステートに入る（乗客ピックアップ）
    /// </summary>
    public void EnterPickupState(Passenger passenger)
    {
        currentState = GameState.Pickup;
        currentPassenger = passenger;
        timeRemaining = pickupTimeLimit;

        if (uiManager != null)
        {
            uiManager.ShowNotification(
                passenger.GetPassengerName(),
                $"ピックアップ: {passenger.GetPickupLocationName()}"
            );
        }

        Debug.Log($"[GameManager] Pickup ステート開始: {passenger.GetPickupLocationName()}");
    }

    /// <summary>
    /// Pickup ステートの更新処理
    /// </summary>
    private void UpdatePickupState()
    {
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            // 時間切れ
            if (uiManager != null)
            {
                uiManager.ShowNotification("失敗", "ピックアップ時間切れ");
            }

            EnterIdleState();
            return;
        }

        // バイクが乗客の場所に到達したか確認
        if (currentPassenger != null && currentPassenger.IsPickedUp(bikeController.transform.position))
        {
            EnterDeliveryState();
        }
    }

    /// <summary>
    /// Delivery ステートに入る（配達中）
    /// </summary>
    public void EnterDeliveryState()
    {
        if (currentPassenger == null)
        {
            return;
        }

        currentState = GameState.Delivery;
        timeRemaining = deliveryTimeLimit;
        currentPassenger.SetPickedUp(true);

        if (uiManager != null)
        {
            uiManager.ShowNotification(
                currentPassenger.GetPassengerName(),
                $"配達先: {currentPassenger.GetDeliveryLocationName()}"
            );
        }

        Debug.Log($"[GameManager] Delivery ステート開始: {currentPassenger.GetDeliveryLocationName()}");
    }

    /// <summary>
    /// Delivery ステートの更新処理
    /// </summary>
    private void UpdateDeliveryState()
    {
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            // 時間切れ
            if (uiManager != null)
            {
                uiManager.ShowNotification("失敗", "配達時間切れ");
            }

            EnterIdleState();
            return;
        }

        // バイクが配達先に到達したか確認
        if (currentPassenger != null && currentPassenger.IsDelivered(bikeController.transform.position))
        {
            EnterResultState();
        }
    }

    /// <summary>
    /// Result ステートに入る（結果表示）
    /// </summary>
    public void EnterResultState()
    {
        if (currentPassenger == null)
        {
            return;
        }

        currentState = GameState.Result;

        // スコア計算（時間に基づく）
        int score = CalculateScore(timeRemaining, deliveryTimeLimit);
        totalScore += score;
        passengersCompleted++;

        if (uiManager != null)
        {
            uiManager.ShowResult(
                currentPassenger.GetPassengerName(),
                score,
                timeRemaining
            );
        }

        Debug.Log($"[GameManager] 配達完了! スコア: {score}, 残り時間: {timeRemaining:F1}秒");

        // 一定時間後に Idle ステートに戻る
        Invoke(nameof(EnterIdleState), 3f);
    }

    /// <summary>
    /// スコアを計算（残り時間に基づく）
    /// </summary>
    private int CalculateScore(float remainingTime, float limitTime)
    {
        // 残り時間が多いほど高スコア
        float timeRatio = Mathf.Clamp01(remainingTime / limitTime);
        int baseScore = 100;
        int bonus = Mathf.RoundToInt(timeRatio * 50); // 最大 50 ボーナス
        return baseScore + bonus;
    }

    /// <summary>
    /// 客を生成・配置
    /// </summary>
    private void GeneratePassenger()
    {
        if (currentState != GameState.Idle || passengersCompleted >= maxPassengersPerGame)
        {
            return;
        }

        // ランダムなピックアップ・配達先を生成
        Vector3 pickupPos = GenerateRandomSpawnPosition();
        Vector3 deliveryPos = GenerateRandomSpawnPosition();

        // 別の場所に配置
        while (Vector3.Distance(pickupPos, deliveryPos) < 50f)
        {
            deliveryPos = GenerateRandomSpawnPosition();
        }

        // Passenger オブジェクトを作成
        GameObject passengerObj = new GameObject("Passenger_" + passengersCompleted);
        Passenger passenger = passengerObj.AddComponent<Passenger>();

        // Passenger 初期化
        passenger.Initialize(
            passengerName: $"乗客 {passengersCompleted + 1}",
            pickupLocation: pickupPos,
            deliveryLocation: deliveryPos,
            pickupRadius: 5f,
            deliveryRadius: 5f
        );

        // 客の可視化（仮のキューブ）
        GameObject visualization = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visualization.name = "PassengerVisual";
        visualization.transform.parent = passengerObj.transform;
        visualization.transform.localPosition = Vector3.zero;
        visualization.transform.localScale = new Vector3(1, 2, 1);

        // Collider を削除
        Collider collider = visualization.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        // ピックアップ位置に配置
        passengerObj.transform.position = pickupPos;

        // GameManager に登録
        passenger.SetGameManager(this);

        currentPassenger = passenger;
        EnterPickupState(passenger);
    }

    /// <summary>
    /// ランダムなスポーン位置を生成（エリア内）
    /// </summary>
    private Vector3 GenerateRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 position = new Vector3(randomCircle.x, 1f, randomCircle.y);

        // エリア内にクランプ
        if (boundaryManager != null)
        {
            position = boundaryManager.ClampToBoundary(position);
        }

        return position;
    }

    /// <summary>
    /// 現在のゲームステートを取得
    /// </summary>
    public GameState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// 総スコアを取得
    /// </summary>
    public int GetTotalScore()
    {
        return totalScore;
    }

    /// <summary>
    /// 完了した配達数を取得
    /// </summary>
    public int GetPassengersCompleted()
    {
        return passengersCompleted;
    }
}
