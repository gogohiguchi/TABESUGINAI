using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // コルーチンを使うために必要

public class GameOverManager : MonoBehaviour
{
    // === UIコンポーネント ===

    // 最終スコアを表示するためのUIテキスト
    public TextMeshProUGUI finalScoreText;

    // スコア以外で、ディレイ表示したいテキスト要素の配列 (例: "GAME OVER", "SCORE:")
    public TextMeshProUGUI[] delayedTextElements;

    // 点滅させたいテキスト (例: "PRESS ENTER")
    public TextMeshProUGUI blinkingText;

    // === ディレイ表示設定 ===

    // 一文字あたりのディレイ時間
    public float delayPerChar = 0.1f;

    // テキスト要素間の待ち時間 (一つの要素の表示が終わってから次の要素が始まるまでの時間)
    public float delayBetweenElements = 0.5f;

    // === 点滅設定 ===

    // 点滅の速さ (値が小さいほど速い)
    public float blinkSpeed = 0.8f;

    // 最も薄い時のアルファ値 (0.0fが完全に透明)
    [Range(0.0f, 1.0f)]
    public float minAlpha = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        // PlayerPrefsから最終スコアを読み込む
        int finalScore = PlayerPrefs.GetInt("SCORE", 0);
        string scoreString = "" + finalScore;

        // 1. すべてのテキスト表示処理を開始し、完了後にEnter受付を開始する
        StartCoroutine(DisplayAllTexts(scoreString));

        // 2. 点滅処理を開始 (テキスト表示と並行して実行されます)
        if (blinkingText != null)
        {
            StartCoroutine(BlinkText());
        }
    }

    // Update is called once per frame
    // テキスト表示完了後にキー受付を行うため、Update()からは受付処理を削除しました
    void Update()
    {
        // ここには、テキスト表示の完了とは無関係に常に行うべき処理のみを記述します
    }

    // ===========================================
    // ★ コルーチン (処理を一時停止・待機させる機能)
    // ===========================================

    // すべてのテキスト要素を順番にディレイ表示し、その後にボタン受付を開始するコルーチン
    IEnumerator DisplayAllTexts(string scoreString)
    {
        // 1. 配列に登録されたテキストを順番に表示
        foreach (TextMeshProUGUI textElement in delayedTextElements)
        {
            // 元のテキストを保持し、ディレイ表示コルーチンを開始
            string originalText = textElement.text;
            yield return StartCoroutine(DisplayTextCoroutine(textElement, originalText));

            // 要素間の待ち時間を設ける
            yield return new WaitForSeconds(delayBetweenElements);
        }

        // 2. 最後にスコアを表示
        // finalScoreTextの元テキストをスコア値で上書きし、ディレイ表示
        yield return StartCoroutine(DisplayTextCoroutine(finalScoreText, scoreString));

        // 3. すべてのテキスト表示が完了したら、ボタン受付コルーチンを開始
        StartCoroutine(WaitForEnterInput());
    }

    // 特定のTextMeshProUGUIを指定された文字列でディレイ表示する汎用コルーチン
    IEnumerator DisplayTextCoroutine(TextMeshProUGUI textComponent, string textToDisplay)
    {
        // まず、表示するテキストを空にする
        textComponent.text = "";

        // 文字列の各文字をループ
        foreach (char c in textToDisplay)
        {
            textComponent.text += c;

            // delayPerChar秒だけ待機する
            yield return new WaitForSeconds(delayPerChar);
        }
    }

    // テキストを周期的に点滅させるコルーチン
    IEnumerator BlinkText()
    {
        Color originalColor = blinkingText.color;

        while (true)
        {
            // フェードアウト (明るい状態 -> minAlpha)
            yield return StartCoroutine(FadeTo(minAlpha, originalColor));

            // フェードイン (minAlpha -> 明るい状態)
            yield return StartCoroutine(FadeTo(1.0f, originalColor));
        }
    }

    // 指定したアルファ値に滑らかに変化させるコルーチン
    IEnumerator FadeTo(float targetAlpha, Color baseColor)
    {
        float startAlpha = blinkingText.color.a; // 現在のアルファ値を取得
        float timer = 0f;
        float duration = 1.0f / blinkSpeed;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // Lerp (線形補間) でアルファ値を計算
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            // テキストの色を設定
            blinkingText.color = new Color(baseColor.r, baseColor.g, baseColor.b, currentAlpha);

            yield return null;
        }

        // 最後に目標のアルファ値を確実に設定
        blinkingText.color = new Color(baseColor.r, baseColor.g, baseColor.b, targetAlpha);
    }

    // Enterキーの入力を待つ専用コルーチン
    IEnumerator WaitForEnterInput()
    {
        // 無限ループでキー入力を待ち続ける
        while (true)
        {
            // Enterキーが押されたら
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // ゲームシーンに戻る
                SceneManager.LoadScene("GameScene");
                yield break;
            }
            // 毎フレームチェック
            yield return null;
        }
    }
}