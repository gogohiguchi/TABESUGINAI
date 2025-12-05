using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 操作説明シーンの画像とナビゲーションを管理するスクリプト
/// </summary>
public class TutorialManager : MonoBehaviour
{
    // --- Public Fields (Unity Editorで設定) ---

    // 操作説明の画像スプライト（1から4の順に設定）
    public Sprite[] tutorialImages;
    // 画像を表示するImageコンポーネント
    public Image displayImage;

    // 遷移先のシーン名
    public string gameSceneName = "GameScene";

    // ナビゲーション用のUIオブジェクト
    public GameObject navigationTextPrev; // 「← : 前へ」
    public GameObject navigationTextNext; // 「→ : 次へ」
    public GameObject enterText;          // 「Enter : ゲームへ」
    public Text pageNumberText;           // 「1/4」などを表示するTextコンポーネント

    // --- Private Fields ---

    // 現在表示している画像のインデックス（0からスタート）
    private int currentImageIndex = 0;

    // --- Unity Life Cycle ---

    void Start()
    {
        // 最初の画像をロードし、UIの状態を更新
        UpdateDisplayState();
    }

    void Update()
    {
        // 入力処理を実行
        HandleInput();
    }

    // --- Core Logic ---

    /// <summary>
    /// キーボード入力を受け付け、インデックスの変更やシーン遷移を行う
    /// </summary>
    private void HandleInput()
    {
        // **右ボタン（次へ）の処理**
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentImageIndex < tutorialImages.Length - 1)
        {
            currentImageIndex++;
            UpdateDisplayState();
        }

        // **左ボタン（前へ）の処理**
        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentImageIndex > 0)
        {
            currentImageIndex--;
            UpdateDisplayState();
        }

        // **エンターキー（ゲームシーンへ）の処理**
        // 最後の画像（4枚目）が表示されている時のみ実行可能
        if (currentImageIndex == tutorialImages.Length - 1 && Input.GetKeyDown(KeyCode.Return))
        {
            // GameSceneへ遷移
            SceneManager.LoadScene(gameSceneName);
        }
    }

    /// <summary>
    /// 画像の表示とUI（ナビゲーション）の状態を更新する
    /// </summary>
    private void UpdateDisplayState()
    {
        if (tutorialImages == null || tutorialImages.Length == 0)
        {
            Debug.LogError("Tutorial Imagesが設定されていません！");
            return;
        }

        // 1. 画像の更新
        displayImage.sprite = tutorialImages[currentImageIndex];

        // 2. UIナビゲーションの更新
        bool isFirst = currentImageIndex == 0;
        bool isLast = currentImageIndex == tutorialImages.Length - 1;

        // 「前へ」の指示: 1枚目の時以外は表示
        if (navigationTextPrev != null) navigationTextPrev.SetActive(!isFirst);

        // 「次へ」の指示: 4枚目の時以外は表示
        if (navigationTextNext != null) navigationTextNext.SetActive(!isLast);

        // 「Enter : ゲームへ」の指示: 4枚目の時のみ表示
        if (enterText != null) enterText.SetActive(isLast);

        // 3. ページ番号の更新（例: "1/4"）
        if (pageNumberText != null)
        {
            pageNumberText.text = $"{currentImageIndex + 1}/{tutorialImages.Length}";
        }
    }
}