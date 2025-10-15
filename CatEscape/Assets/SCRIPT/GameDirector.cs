using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using TMPro; 

public class GameDirector : MonoBehaviour
{
    public GameObject fullnessGauge;
    public TextMeshProUGUI scoreText;
    public float score = 0;
    public bool isGameOver = false;
    public float fullnessDecreaseRate = 1.5f; // 1秒あたりの減少量を設定

    // 満腹ゲージの現在の値と最大値を管理する変数を追加
    private float currentFullness = 0;
    private const float MAX_FULLNESS = 150f;

    // シングルトンパターンを適用
    public static GameDirector Instance { get; private set; }
    public void Awake()
    {
        // 既にインスタンスが存在する場合、この新しいインスタンスを破棄する
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        // このインスタンスをシングルトンにする
        Instance = this;

        // シーンを切り替えても破棄されないようにする
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        // ゲージの見た目を初期状態の0にする（内部のcurrentFullnessも0なので、ここは表示用）
        this.fullnessGauge.GetComponent<Image>().fillAmount = 0.0f;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ゲームシーンに戻ったときにゲームの状態をリセット
        if (scene.name == "GameScene")
        {
            // すべてのゲーム変数をリセットする
            score = 0;
            currentFullness = 0;
            isGameOver = false;

            // UIの参照を再取得して更新する
            fullnessGauge = GameObject.Find("fullnessGauge");
            scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            
            if (fullnessGauge != null)
            {
                fullnessGauge.GetComponent<Image>().fillAmount = 0.0f;
            }
            if (scoreText != null)
            {
                scoreText.text = "0";
            }
        }
    }

    // 食材を食べた時に呼び出すメソッド
    public void EatFood(float fullnessValue, int scoreValue)
    {
        // インスタンスが正しく設定されているか確認
        if (Instance != this)
        {
            return;
        }

        if (isGameOver) return;

        // 満腹ゲージの内部値を増やす
        currentFullness += fullnessValue;

        // 最大値を超えないようにクランプする
        currentFullness = Mathf.Clamp(currentFullness, 0, MAX_FULLNESS);

        // UIゲージを更新（内部値を0-1の範囲に変換して表示）
        if (fullnessGauge != null)
        {
            fullnessGauge.GetComponent<Image>().fillAmount = currentFullness / MAX_FULLNESS;
        }
        // スコアを加算
        score += scoreValue;

        // スコア表示を更新
        if (scoreText != null)
        {
            scoreText.text = "" + score;
        }

        // 満腹ゲージが満タンになったかチェック
        if (currentFullness >= MAX_FULLNESS)
        {
            isGameOver = true;
            Debug.Log("Game Over! Your Score: " + score);

            // 最終スコアを保存
            PlayerPrefs.SetInt("SCORE", (int)score);

            // ゲームオーバー画面へ遷移
            SceneManager.LoadScene("GameOver");
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isGameOver) return;

        // 時間経過で満腹ゲージの内部値を減らす
        currentFullness -= fullnessDecreaseRate * Time.deltaTime;

        // ゲージがマイナスにならないようにクランプする
        currentFullness = Mathf.Clamp(currentFullness, 0, MAX_FULLNESS);

        // UIゲージを更新
        if (fullnessGauge != null)
        {
            fullnessGauge.GetComponent<Image>().fillAmount = currentFullness / MAX_FULLNESS;
        }
    }
}