using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // この行を追加

public class GameOverManager : MonoBehaviour
{
    // スコアを表示するためのUIテキスト
    public TextMeshProUGUI finalScoreText;

    // Start is called before the first frame update
    void Start()
    {
        // PlayerPrefsから最終スコアを読み込む
        int finalScore = PlayerPrefs.GetInt("SCORE", 0);

        // UIにスコアを表示
        finalScoreText.text = "" + finalScore;
    }

    // Update is called once per frame
    void Update()
    {
        // エンターキーが押されたら
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // ゲームシーンに戻る
            SceneManager.LoadScene("GameScene");
        }
    }
}