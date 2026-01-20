using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI システム管理
/// スマホ風HUD、通知、タイマー、スコア表示を管理
/// Canvas ベースの 2D UI を制御
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Canvas 設定")]
    [SerializeField]
    private Canvas mainCanvas;

    [SerializeField]
    private Vector2 notificationSize = new Vector2(400, 120); // 通知ウィンドウサイズ

    [SerializeField]
    private Vector2 scoreDisplaySize = new Vector2(300, 80); // スコア表示サイズ

    // UI パネル（実行時に生成）
    private GameObject notificationPanel;
    private Text notificationTitle;
    private Text notificationMessage;
    private Text timerText;
    private Text scoreText;
    private Text stateText;
    private GameObject resultPanel;
    private Text resultPassengerName;
    private Text resultScore;
    private Text resultTime;

    private void Start()
    {
        // Canvas が設定されていない場合、自動取得
        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
        }

        // UI パネルの作成
        CreateUIElements();
    }

    /// <summary>
    /// UI エレメントを作成
    /// </summary>
    private void CreateUIElements()
    {
        if (mainCanvas == null)
        {
            Debug.LogError("UIManager: Canvas が見つかりません");
            return;
        }

        // === 通知パネル（上部中央） ===
        notificationPanel = new GameObject("NotificationPanel");
        notificationPanel.transform.SetParent(mainCanvas.transform, false);

        RectTransform notifRect = notificationPanel.AddComponent<RectTransform>();
        notifRect.anchoredPosition = new Vector2(0, 300);
        notifRect.sizeDelta = notificationSize;

        Image notifBG = notificationPanel.AddComponent<Image>();
        notifBG.color = new Color(0, 0, 0, 0.8f); // 半透明黒

        // 通知タイトル
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(notificationPanel.transform, false);
        notificationTitle = titleObj.AddComponent<Text>();
        notificationTitle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        notificationTitle.text = "お知らせ";
        notificationTitle.fontSize = 24;
        notificationTitle.fontStyle = FontStyle.Bold;
        notificationTitle.alignment = TextAnchor.UpperCenter;
        notificationTitle.color = Color.white;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, -30);
        titleRect.sizeDelta = new Vector2(380, 40);

        // 通知メッセージ
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(notificationPanel.transform, false);
        notificationMessage = messageObj.AddComponent<Text>();
        notificationMessage.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        notificationMessage.text = "準備中...";
        notificationMessage.fontSize = 18;
        notificationMessage.alignment = TextAnchor.MiddleCenter;
        notificationMessage.color = Color.yellow;

        RectTransform messageRect = messageObj.GetComponent<RectTransform>();
        messageRect.anchoredPosition = new Vector2(0, -65);
        messageRect.sizeDelta = new Vector2(380, 30);

        // === タイマー表示（右上） ===
        GameObject timerObj = new GameObject("TimerText");
        timerObj.transform.SetParent(mainCanvas.transform, false);
        timerText = timerObj.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        timerText.text = "00:00";
        timerText.fontSize = 32;
        timerText.fontStyle = FontStyle.Bold;
        timerText.alignment = TextAnchor.UpperRight;
        timerText.color = Color.red;

        RectTransform timerRect = timerObj.GetComponent<RectTransform>();
        timerRect.anchoredPosition = new Vector2(-50, -50);
        timerRect.sizeDelta = new Vector2(200, 50);

        // === スコア表示（左上） ===
        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(mainCanvas.transform, false);
        scoreText = scoreObj.AddComponent<Text>();
        scoreText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        scoreText.text = "Score: 0";
        scoreText.fontSize = 24;
        scoreText.fontStyle = FontStyle.Bold;
        scoreText.alignment = TextAnchor.UpperLeft;
        scoreText.color = Color.green;

        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.anchoredPosition = new Vector2(50, -50);
        scoreRect.sizeDelta = new Vector2(300, 50);

        // === ゲームステート表示（左下） ===
        GameObject stateObj = new GameObject("StateText");
        stateObj.transform.SetParent(mainCanvas.transform, false);
        stateText = stateObj.AddComponent<Text>();
        stateText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        stateText.text = "State: Idle";
        stateText.fontSize = 16;
        stateText.alignment = TextAnchor.LowerLeft;
        stateText.color = Color.cyan;

        RectTransform stateRect = stateObj.GetComponent<RectTransform>();
        stateRect.anchoredPosition = new Vector2(50, 50);
        stateRect.sizeDelta = new Vector2(300, 40);

        // === 結果パネル（中央、隠れた状態） ===
        resultPanel = new GameObject("ResultPanel");
        resultPanel.transform.SetParent(mainCanvas.transform, false);
        resultPanel.SetActive(false);

        RectTransform resultRect = resultPanel.AddComponent<RectTransform>();
        resultRect.sizeDelta = new Vector2(500, 300);

        Image resultBG = resultPanel.AddComponent<Image>();
        resultBG.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // ほぼ不透明黒

        // 結果パネル - タイトル
        GameObject resultTitleObj = new GameObject("ResultTitle");
        resultTitleObj.transform.SetParent(resultPanel.transform, false);
        Text resultTitle = resultTitleObj.AddComponent<Text>();
        resultTitle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        resultTitle.text = "配達完了！";
        resultTitle.fontSize = 32;
        resultTitle.fontStyle = FontStyle.Bold;
        resultTitle.alignment = TextAnchor.UpperCenter;
        resultTitle.color = Color.green;

        RectTransform resultTitleRect = resultTitleObj.GetComponent<RectTransform>();
        resultTitleRect.anchoredPosition = new Vector2(0, -40);
        resultTitleRect.sizeDelta = new Vector2(480, 50);

        // 結果パネル - 乗客名
        GameObject passengerNameObj = new GameObject("PassengerName");
        passengerNameObj.transform.SetParent(resultPanel.transform, false);
        resultPassengerName = passengerNameObj.AddComponent<Text>();
        resultPassengerName.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        resultPassengerName.text = "乗客 1";
        resultPassengerName.fontSize = 20;
        resultPassengerName.alignment = TextAnchor.MiddleCenter;
        resultPassengerName.color = Color.white;

        RectTransform passengerNameRect = passengerNameObj.GetComponent<RectTransform>();
        passengerNameRect.anchoredPosition = new Vector2(0, -110);
        passengerNameRect.sizeDelta = new Vector2(460, 40);

        // 結果パネル - スコア
        GameObject resultScoreObj = new GameObject("Score");
        resultScoreObj.transform.SetParent(resultPanel.transform, false);
        resultScore = resultScoreObj.AddComponent<Text>();
        resultScore.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        resultScore.text = "Score: 150";
        resultScore.fontSize = 24;
        resultScore.fontStyle = FontStyle.Bold;
        resultScore.alignment = TextAnchor.MiddleCenter;
        resultScore.color = Color.yellow;

        RectTransform resultScoreRect = resultScoreObj.GetComponent<RectTransform>();
        resultScoreRect.anchoredPosition = new Vector2(0, -160);
        resultScoreRect.sizeDelta = new Vector2(460, 40);

        // 結果パネル - 時間
        GameObject resultTimeObj = new GameObject("Time");
        resultTimeObj.transform.SetParent(resultPanel.transform, false);
        resultTime = resultTimeObj.AddComponent<Text>();
        resultTime.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        resultTime.text = "残り時間: 45秒";
        resultTime.fontSize = 18;
        resultTime.alignment = TextAnchor.MiddleCenter;
        resultTime.color = Color.cyan;

        RectTransform resultTimeRect = resultTimeObj.GetComponent<RectTransform>();
        resultTimeRect.anchoredPosition = new Vector2(0, -210);
        resultTimeRect.sizeDelta = new Vector2(460, 40);
    }

    /// <summary>
    /// 通知を表示
    /// </summary>
    public void ShowNotification(string title, string message)
    {
        if (notificationTitle != null)
        {
            notificationTitle.text = title;
        }

        if (notificationMessage != null)
        {
            notificationMessage.text = message;
        }

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
        }
    }

    /// <summary>
    /// タイマーを更新
    /// </summary>
    public void UpdateTimer(float timeRemaining)
    {
        if (timerText == null)
        {
            return;
        }

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes:D2}:{seconds:D2}";

        // 時間が少なくなったら色を変える
        if (timeRemaining <= 10f)
        {
            timerText.color = Color.red;
        }
        else if (timeRemaining <= 30f)
        {
            timerText.color = new Color(1, 0.5f, 0); // オレンジ
        }
        else
        {
            timerText.color = Color.white;
        }
    }

    /// <summary>
    /// スコアを更新
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    /// <summary>
    /// ゲームステートを更新
    /// </summary>
    public void UpdateState(string state)
    {
        if (stateText != null)
        {
            stateText.text = $"State: {state}";
        }
    }

    /// <summary>
    /// 結果を表示
    /// </summary>
    public void ShowResult(string passengerName, int score, float timeRemaining)
    {
        if (resultPassengerName != null)
        {
            resultPassengerName.text = passengerName;
        }

        if (resultScore != null)
        {
            resultScore.text = $"Score: {score}";
        }

        if (resultTime != null)
        {
            int seconds = Mathf.FloorToInt(timeRemaining);
            resultTime.text = $"残り時間: {seconds}秒";
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            Invoke(nameof(HideResultPanel), 3f);
        }
    }

    /// <summary>
    /// 結果パネルを非表示
    /// </summary>
    private void HideResultPanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }
}
