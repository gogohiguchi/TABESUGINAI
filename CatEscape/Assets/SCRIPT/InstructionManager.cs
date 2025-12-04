using UnityEngine;
using UnityEngine.UI; // UI要素（Image）を使うために必要
using UnityEngine.SceneManagement; // シーン切り替えのために必要

public class TutorialManager : MonoBehaviour
{
    // 操作説明の画像（1から4の順に設定）
    public Sprite[] tutorialImages;
    // 表示用のImageコンポーネント
    public Image displayImage;

    // 現在表示している画像のインデックス
    private int currentImageIndex = 0;

    // 遷移先のシーン名
    public string gameSceneName = "GameScene";

    void Start()
    {
        // 最初に最初の画像を表示
        UpdateDisplayImage();
    }

    void UpdateDisplayImage()
    {
        if (tutorialImages != null && tutorialImages.Length > 0)
        {
            // インデックスが範囲内であることを確認
            currentImageIndex = Mathf.Clamp(currentImageIndex, 0, tutorialImages.Length - 1);
            // ImageコンポーネントのSpriteを更新
            displayImage.sprite = tutorialImages[currentImageIndex];

            // デバッグ用に現在の画像番号をログ出力（オプション）
            Debug.Log("現在の画像: " + (currentImageIndex + 1));
        }
    }
    void Update()
    {
        // **右ボタン（次へ）の処理**
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // 現在の画像が最後の画像（4枚目）でなければ
            if (currentImageIndex < tutorialImages.Length - 1)
            {
                currentImageIndex++;
                UpdateDisplayImage();
            }
        }

        // **左ボタン（前へ）の処理**
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // 現在の画像が最初の画像（1枚目）でなければ
            if (currentImageIndex > 0)
            {
                currentImageIndex--;
                UpdateDisplayImage();
            }
        }

        // **エンターキー（ゲームシーンへ）の処理**
        // 4枚目の画像が表示されている時のみ
        if (currentImageIndex == tutorialImages.Length - 1 && Input.GetKeyDown(KeyCode.Return))
        {
            // GameSceneへ遷移
            SceneManager.LoadScene(gameSceneName);
        }
    }
}