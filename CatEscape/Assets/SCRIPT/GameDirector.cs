using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameDirector : MonoBehaviour
{
    // === UI/ステータス変数 ===
    public GameObject fullnessGauge;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI fullnessValueText;

    // ★ 修正：ブースト用とストップ用で個別にTextを管理 ★
    public TextMeshProUGUI boostStatusText;
    public TextMeshProUGUI timeStopStatusText;

    public float score = 0;
    public bool isGameOver = false;

    // 視覚効果用の変数
    [Header("視覚効果")]
    private Camera mainCamera;
    private SpriteRenderer playerRenderer;
    private Color originalCameraColor = Color.black;
    private Color originalPlayerColor = Color.white;

    // コルーチン変数を独立させる
    private Coroutine timeStopCoroutine;
    private Coroutine boostCoroutine;

    [Header("満腹度設定")]
    public float fullnessDecreaseRate = 1.5f;
    public float initialFullness = 40f;
    private float currentFullness;
    private const float MAX_FULLNESS = 200f;

    [Header("特殊効果設定")]
    public float timeStopDuration = 5f;
    public float boostDuration = 3f;
    public int boostScoreBonus = 300;
    public float boostFullnessMultiplier = 1.5f;
    public string gameOverSceneName = "GameOver";

    public bool isFullnessDrainStopped = false;
    public bool isFullnessChangeStopped = false;
    public bool isBoostActive = false;

    [Header("出現設定")]
    public List<FoodItemData> allFoodItems;

    // === シングルトンパターン ===
    public static GameDirector Instance { get; private set; }

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

        currentFullness = initialFullness;

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
            currentFullness = initialFullness;
            isGameOver = false;
            isFullnessDrainStopped = false;
            isFullnessChangeStopped = false;
            isBoostActive = false;

            timeStopCoroutine = null;
            boostCoroutine = null;

            // UIの参照を取得
            fullnessGauge = GameObject.Find("FullnessGauge")?.gameObject;
            scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();

            GameObject fullnessTextObject = GameObject.Find("FullnessValueText");
            if (fullnessTextObject != null)
            {
                fullnessValueText = fullnessTextObject.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogError("UI Error: 'FullnessValueText' がシーンで見つかりませんでした。");
            }

            // ★ 修正：Boost用とTimeStop用のテキスト参照を取得し、初期化 ★
            boostStatusText = GameObject.Find("BoostStatusText")?.GetComponent<TextMeshProUGUI>();
            timeStopStatusText = GameObject.Find("TimeStopStatusText")?.GetComponent<TextMeshProUGUI>();

            if (boostStatusText != null) boostStatusText.text = "";
            if (timeStopStatusText != null) timeStopStatusText.text = "";
            // --------------------------------------------------------

            // 視覚効果用の参照を取得し、元の色を保存 (リセット) 
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                originalCameraColor = mainCamera.backgroundColor;
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerRenderer = playerObject.GetComponent<SpriteRenderer>();
                if (playerRenderer != null)
                {
                    originalPlayerColor = playerRenderer.color;
                    playerRenderer.color = originalPlayerColor;
                }
            }

            ResetVisualEffects();
            UpdateFullnessAndScore();

            if (fullnessGauge == null)
            {
                Debug.LogWarning("満腹ゲージ (FullnessGauge) がシーン 'GameScene' で見つかりませんでした。");
            }
        }
    }

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
        switch (type)
        {
            case "TimeStop":
                if (timeStopCoroutine != null)
                {
                    StopCoroutine(timeStopCoroutine);
                    if (playerRenderer != null) playerRenderer.color = originalPlayerColor;
                    // ★ 修正：TimeStopStatusTextもリセット ★
                    if (timeStopStatusText != null) timeStopStatusText.text = "";
                    isFullnessDrainStopped = false;
                    isFullnessChangeStopped = false;
                }
                timeStopCoroutine = StartCoroutine(TimeStopEffect(timeStopDuration));
                return true;

            case "Boost":
                if (boostCoroutine != null)
                {
                    StopCoroutine(boostCoroutine);
                    if (mainCamera != null) mainCamera.backgroundColor = originalCameraColor;
                    // ★ 修正：BoostStatusTextもリセット ★
                    if (boostStatusText != null) boostStatusText.text = "";
                    isBoostActive = false;
                }
                boostCoroutine = StartCoroutine(BoostEffect(boostDuration));
                return true;

            case "MaxOut":
                if (timeStopCoroutine != null) StopCoroutine(timeStopCoroutine);
                if (boostCoroutine != null) StopCoroutine(boostCoroutine);

                ResetVisualEffects();
                isFullnessDrainStopped = false;
                isFullnessChangeStopped = false;
                isBoostActive = false;

                MaxOutEffect(4000);
                return true;

            default:
                return false;
        }
    }

    private IEnumerator TimeStopEffect(float duration)
    {
        Debug.Log($"特殊効果: TimeStop (満腹ゲージ変動停止) {duration}秒開始");

        if (playerRenderer == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) playerRenderer = playerObject.GetComponent<SpriteRenderer>();
        }

        isFullnessDrainStopped = true;
        isFullnessChangeStopped = true;

        if (playerRenderer != null) playerRenderer.color = Color.green;

        // ★ 修正：timeStopStatusText のみを使用 ★
        if (timeStopStatusText != null) timeStopStatusText.text = "満腹ゲージロック中！";

        yield return new WaitForSeconds(duration);

        isFullnessDrainStopped = false;
        isFullnessChangeStopped = false;

        if (playerRenderer != null) playerRenderer.color = originalPlayerColor;

        // ★ 修正：TimeStopStatusTextのみをクリア ★
        if (timeStopStatusText != null) timeStopStatusText.text = "";

        timeStopCoroutine = null;
        Debug.Log("特殊効果: TimeStop 終了");
    }

    private IEnumerator BoostEffect(float duration)
    {
        Debug.Log($"特殊効果: Boost (効果中、満腹度とスコア増加) {duration}秒開始");

        if (mainCamera == null) mainCamera = Camera.main;

        isBoostActive = true;

        if (mainCamera != null)
        {
            mainCamera.backgroundColor = Color.Lerp(originalCameraColor, Color.yellow, 0.7f);
        }

        // ★ 修正：boostStatusText のみを使用 ★
        if (boostStatusText != null) boostStatusText.text = "ブースト中！";

        yield return new WaitForSeconds(duration);

        isBoostActive = false;

        if (mainCamera != null) mainCamera.backgroundColor = originalCameraColor;

        // ★ 修正：BoostStatusTextのみをクリア ★
        if (boostStatusText != null) boostStatusText.text = "";

        boostCoroutine = null;
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

    // --- 視覚効果リセットヘルパーメソッド ---
    private void ResetVisualEffects()
    {
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = originalCameraColor;
        }

        if (playerRenderer != null)
        {
            playerRenderer.color = originalPlayerColor;
        }

        // ★ 修正：両方のテキストをリセット ★
        if (boostStatusText != null) boostStatusText.text = "";
        if (timeStopStatusText != null) timeStopStatusText.text = "";
    }
    // -------------------------------------

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
                Color targetColor = Color.white;

                if (currentFullness <= 29f || currentFullness >= 160f)
                {
                    targetColor = Color.red;
                }

                gaugeImage.fillAmount = currentFullness / MAX_FULLNESS;
                gaugeImage.color = targetColor;
            }
        }

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        if (fullnessValueText != null)
        {
            fullnessValueText.text = Mathf.FloorToInt(currentFullness).ToString();
        }
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