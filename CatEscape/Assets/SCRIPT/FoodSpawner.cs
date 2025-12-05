using UnityEngine;
using System.Collections;

public class FoodSpawner : MonoBehaviour
{
    // ... (他の変数は省略) ...

    // 食材を生成する間隔
    public float spawnInterval = 0.4f;
    // ... (他の変数は省略) ...
    private float timer;
    private float screenWidth;
    private int scoreThreshold = 3000;
    public float minSpawnInterval = 0.4f;

    void Start()
    {
        // カメラの幅から画面の幅を計算する
        screenWidth = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).x - Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).x;
        timer = spawnInterval;
    }

    public void Update()
    {
        if (GameDirector.Instance == null || GameDirector.Instance.isGameOver) return;

        // スコアが3000を超えるごとに生成間隔を短くする
        if (GameDirector.Instance.score > scoreThreshold)
        {
            spawnInterval -= 0.1f;
            if (spawnInterval < minSpawnInterval)
            {
                spawnInterval = minSpawnInterval;
            }
            scoreThreshold += 3000;
        }

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            SpawnFood();
            timer = spawnInterval;
        }
    }

    void SpawnFood()
    {
        // 1. GameDirectorから重み付きランダムでFoodItemDataを取得
        FoodItemData foodData = GameDirector.Instance.GetRandomFoodItem();

        if (foodData == null || foodData.prefab == null)
        {
            if (foodData == null)
            {
                Debug.LogWarning("FoodItemDataが返されなかったため、アイテム生成をスキップします。GameDirectorのリスト設定を確認してください。");
            }
            return;
        }

        // 2. 座標計算
        float randomX = Random.Range(-screenWidth / 2f, screenWidth / 2f);
        Vector3 spawnPosition = new Vector3(randomX, Camera.main.ViewportToWorldPoint(new Vector3(0, 1.05f, 0)).y, 0);

        // 3. 食材を生成
        GameObject clone = Instantiate(foodData.prefab, spawnPosition, Quaternion.identity);

        // ★★★ 修正点: FoodItemスクリプトにFoodItemDataを渡す ★★★
        // 4. 生成されたインスタンスのFoodItemスクリプトにデータアセットを割り当てる
        FoodItem foodItemScript = clone.GetComponent<FoodItem>();
        if (foodItemScript != null)
        {
            // ここで抽選で選ばれたFoodItemDataアセットを、インスタンス側の変数に設定する
            foodItemScript.foodData = foodData;
        }
        else
        {
            Debug.LogError($"Error: 生成されたプレハブ '{foodData.prefab.name}' に FoodItem.cs が見つかりませんでした。");
        }
        // ★★★ 修正点終わり ★★★

        // 5. 自動的に削除
        Destroy(clone, 3.5f);
    }
}