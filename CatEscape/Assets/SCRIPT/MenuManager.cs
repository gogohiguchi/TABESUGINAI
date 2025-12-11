using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Unity Editorで設定するUI要素
    [Header("UI Elements")]
    public GameObject menuWindow;
    public GameObject menuButton;

    [Header("Scene Settings")]
    public string titleSceneName = "Title"; // タイトルシーンの名前を設定

    private bool isPaused = false;

    void Start()
    {
        // 初期状態ではメニューウィンドウを非表示にする
        if (menuWindow != null)
        {
            menuWindow.SetActive(false);
        }
        else
        {
            Debug.LogError("Menu Windowが設定されていません。Inspectorで設定してください。");
        }
    }

    /// <summary>
    /// メニューボタンが押されたときに呼ばれる関数
    /// </summary>
    public void OnMenuButtonPressed()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// ゲームをポーズし、メニューを開く
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;

        // 1. メニューウィンドウを表示
        if (menuWindow != null)
        {
            menuWindow.SetActive(true);
        }

        // 2. ゲーム時間の停止
        Time.timeScale = 0f;
    }

    /// <summary>
    /// ゲームを再開し、メニューを閉じる
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;

        // 1. メニューウィンドウを非表示
        if (menuWindow != null)
        {
            menuWindow.SetActive(false);
        }

        // 2. ゲーム時間を元に戻す
        Time.timeScale = 1f;
    }

    /// <summary>
    /// タイトルに戻るボタンが押されたときに呼ばれる関数
    /// </summary>
    public void OnBackToTitleButtonPressed()
    {
        // 1. ポーズを解除して時間を元に戻す（重要！）
        Time.timeScale = 1f;

        // 2. シーンをロード
        SceneManager.LoadScene(titleSceneName);
    }
}