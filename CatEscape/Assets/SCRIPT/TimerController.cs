using UnityEngine;
using TMPro; // TextMeshProを使用するために必要

public class TimerController : MonoBehaviour
{
    // === インスペクターから調整可能な変数 ===

    [Header("タイマー設定")]
    [Tooltip("タイマーの初期設定時間 (秒)")]
    [SerializeField]
    private float initialTime = 99f;


    // ★ 新しく追加する変数
    [Header("シーン設定")]
    [Tooltip("ゲームオーバー時に遷移するシーンの名前")]
    [SerializeField]
    private string gameOverSceneName = "GameOverScene";

    // === プライベート変数 ===

    private float currentTime;          // 現在のタイマー時間
    private bool isTimerRunning = true; // タイマーが実行中かどうか

    [Header("コンポーネント参照")]
    [Tooltip("時間を表示する TextMeshPro コンポーネント")]
    [SerializeField]
    private TextMeshProUGUI timerText;  // UI用のTextMeshProコンポーネント

    // --- Start ---
    void Start()
    {
        // 初期時間を設定し、Textコンポーネントの参照をチェック
        currentTime = initialTime;

        if (timerText == null)
        {
            Debug.LogError("TimerController: timerText (TextMeshProUGUI) が設定されていません。インスペクターで設定してください。");
            isTimerRunning = false;
        }
    }

    // --- Update ---
    void Update()
    {
        if (isTimerRunning && timerText != null)
        {
            // 時間を減らす
            currentTime -= Time.deltaTime;

            // 時間の表示を更新 (小数点以下を切り捨てて整数表示)
            // Math.Max(0, ...) で時間が負になるのを防ぐ
            timerText.text = Mathf.CeilToInt(Mathf.Max(0, currentTime)).ToString();

            // 0秒以下になったらゲームオーバー処理
            if (currentTime <= 0f)
            {
                currentTime = 0f; // 念のため時間を0に固定
                isTimerRunning = false; // タイマーを停止
                GameOver();
            }
        }
    }

    // --- ゲームオーバー処理 ---
    private void GameOver()
    {
        Debug.Log("🎉 ゲームオーバー！時間がなくなりました。");
        // ここにゲームオーバー画面への遷移、時間停止、スコア表示などの
        // 実際のゲームオーバー処理を記述します。

        // 例: Time.timeScale = 0f; // ゲーム全体を停止する場合
    }

    // 外部からタイマーをリセットしたい場合のために公開
    public void ResetTimer()
    {
        currentTime = initialTime;
        isTimerRunning = true;
    }
}