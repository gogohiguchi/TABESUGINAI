using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshProを使用するために必要
using System.Collections;
using System.Collections.Generic;

public class GameDirector : MonoBehaviour
{
    // === UI/ステータス変数 ===
    public GameObject fullnessGauge;
    public TextMeshProUGUI scoreText;

    // ★ 追加：満腹度数値を表示するためのTextMeshProUGUIの参照 ★
    public TextMeshProUGUI fullnessValueText;

    public float score = 0;
    public bool isGameOver = false;

    [Header("満腹度設定")]
    public float fullnessDecreaseRate = 1.5f;
    private float currentFullness = 40f;
    private const float MAX_FULLNESS = 200f;

    // ... (特殊効果設定、フラグの宣言は省略) ...
    public float timeStopDuration = 5f;
    public float boostDuration = 3f;
    public int boostScoreBonus = 300;
    public float boostFullnessMultiplier = 1.5f;
    public string gameOverSceneName = "GameOver";

    public bool isFullnessDrainStopped = false;
    public bool isFullnessChangeStopped = false;
    public bool isBoostActive = false;

    private Coroutine activeSpecialEffectCoroutine;

    [Header("出現設定")]
    public List<FoodItemData> allFoodItems;

    // === シングルトンパターン ===
    public static GameDirector Instance { get; private set; }
    // ... (Awake, Start の内容は省略) ...
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        isFullnessDrainStopped = false;
        isFullnessChangeStopped = false;
        isBoostActive = false;

        if (fullnessGauge != null)
        {
            this.fullnessGauge.GetComponent<Image>().fillAmount = 0.0f;
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            score = 0;
            currentFullness = 40f;
            isGameOver = false;
            isFullnessDrainStopped = false;
            isFullnessChangeStopped = false;
            isBoostActive = false;

            fullnessGauge = GameObject.Find("FullnessGauge")?.gameObject;
            scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();

            // ★ 修正：FullnessValueTextの参照を取得 ★
            // ヒエラルキー内に "FullnessValueText" という名前のTextMeshProオブジェクトがあることを想定
            fullnessValueText = GameObject.Find("FullnessValueText")?.GetComponent<TextMeshProUGUI>();

            UpdateFullnessAndScore();

            if (fullnessGauge == null)
            {
                Debug.LogWarning("満腹ゲージ (FullnessGauge) がシーン 'GameScene' で見つかりませんでした。");
            }
        }
    }

    // ... (EatFood, GetRandomFoodItem, HandleSpecialItem の内容は省略) ...
    public void EatFood(float fullnessValue, int scoreValue, string specialType)
    {
        if (isGameOver) return;

        if (isFullnessChangeStopped && specialType != "TimeStop")
        {
            score += scoreValue;
            UpdateFullnessAndScore();
            return;
        }

        if (HandleSpecialItem(specialType, fullnessValue, scoreValue))
        {
            if (specialType == "MaxOut") return;
        }

        float finalFullness = fullnessValue;
        int finalScore = scoreValue;

        if (isBoostActive)
        {
            finalFullness *= boostFullnessMultiplier;
            finalScore = (int)(finalScore * boostFullnessMultiplier) + boostScoreBonus;
        }

        if (!isFullnessChangeStopped)
        {
            currentFullness += finalFullness;
        }

        score += finalScore;

        UpdateFullnessAndScore();

        if (currentFullness >= MAX_FULLNESS)
        {
            isGameOver = true;
            Debug.Log("Game Over! 満腹度が上限に達しました。Score: " + score);

            PlayerPrefs.SetInt("SCORE", (int)score);
            SceneManager.LoadScene(gameOverSceneName);
            return;
        }
    }

    public FoodItemData GetRandomFoodItem()
    {
        if (allFoodItems == null || allFoodItems.Count == 0)
        {
            Debug.LogError("FoodItemData リストが空です。");
            return null;
        }

        float totalWeight = 0f;
        foreach (var item in allFoodItems)
        {
            if (item.dropWeight > 0)
            {
                totalWeight += item.dropWeight;
            }
        }

        float randomPoint = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var item in allFoodItems)
        {
            if (item.dropWeight > 0)
            {
                currentWeight += item.dropWeight;

                if (randomPoint <= currentWeight)
                {
                    return item;
                }
            }
        }

        return allFoodItems[0];
    }

    private bool HandleSpecialItem(string type, float baseFullness, int baseScore)
    {
        if (activeSpecialEffectCoroutine != null)
        {
            StopCoroutine(activeSpecialEffectCoroutine);
            if (isFullnessDrainStopped) isFullnessDrainStopped = false;
            if (isFullnessChangeStopped) isFullnessChangeStopped = false;
            if (isBoostActive) isBoostActive = false;
        }

        switch (type)
        {
            case "TimeStop":
                activeSpecialEffectCoroutine = StartCoroutine(TimeStopEffect(timeStopDuration));
                return true;

            case "Boost":
                activeSpecialEffectCoroutine = StartCoroutine(BoostEffect(boostDuration));
                return true;

            case "MaxOut":
                MaxOutEffect(4000);
                return true;

            default:
                return false;
        }
    }

    private IEnumerator TimeStopEffect(float duration)
    {
        Debug.Log($"特殊効果: TimeStop (満腹ゲージ変動停止) {duration}秒開始");
        isFullnessDrainStopped = true;
        isFullnessChangeStopped = true;

        yield return new WaitForSeconds(duration);

        isFullnessDrainStopped = false;
        isFullnessChangeStopped = false;
        Debug.Log("特殊効果: TimeStop 終了");
    }

    private IEnumerator BoostEffect(float duration)
    {
        Debug.Log($"特殊効果: Boost (効果中、満腹度とスコア増加) {duration}秒開始");
        isBoostActive = true;

        yield return new WaitForSeconds(duration);

        isBoostActive = false;
        Debug.Log("特殊効果: Boost 終了");
    }

    private void MaxOutEffect(int bonusScore)
    {
        score += bonusScore;
        currentFullness = MAX_FULLNESS;

        Debug.Log("特殊効果: MaxOut発動！即ゲームオーバーです。");

        isGameOver = true;
        PlayerPrefs.SetInt("SCORE", (int)score);

        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.LogError("Game Over Scene Name が設定されていません！");
        }
    }

    // --- 状態更新ヘルパーメソッド ---
    private void UpdateFullnessAndScore()
    {
        currentFullness = Mathf.Clamp(currentFullness, 0, MAX_FULLNESS);

        // UIゲージの更新と色変化
        if (fullnessGauge != null)
        {
            Image gaugeImage = fullnessGauge.GetComponent<Image>();
            if (gaugeImage != null)
            {
                // 満腹度の値に基づき色を決定するロジック
                Color targetColor = Color.white;

                if (currentFullness <= 29f || currentFullness >= 160f)
                {
                    targetColor = Color.red;
                }

                gaugeImage.fillAmount = currentFullness / MAX_FULLNESS;
                gaugeImage.color = targetColor;
            }
        }

        // スコア表示を更新
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        // ★ 追加：満腹度数値を更新 ★
        if (fullnessValueText != null)
        {
            // 小数点以下を切り捨てて整数で表示
            fullnessValueText.text = Mathf.FloorToInt(currentFullness).ToString();
        }
        // ★ 変更ここまで ★
    }

    // --- Update ---
    void Update()
    {
        if (isGameOver) return;

        // 1. 左右キーによる満腹度減少 (操作コスト)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            if (!isFullnessChangeStopped)
            {
                currentFullness -= 1.5f;
            }
        }

        // 2. 満腹ゲージ減少 (時間コスト)
        if (!isFullnessDrainStopped)
        {
            currentFullness -= fullnessDecreaseRate * Time.deltaTime;
        }

        // 3. 満腹度が0以下でゲームオーバー
        if (currentFullness <= 0)
        {
            currentFullness = 0;
            isGameOver = true;
            Debug.Log("Game Over! 満腹度が0になりました。");

            PlayerPrefs.SetInt("SCORE", (int)score);
            SceneManager.LoadScene(gameOverSceneName);
            return;
        }

        // 4. UI更新
        UpdateFullnessAndScore();
    }
}