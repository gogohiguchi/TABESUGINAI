using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    // 降らせる食材のプレハブをインスペクターで設定
    public GameObject[] foodPrefabs;
    
    // 食材を生成する間隔
    public float spawnInterval = 2.0f;
    
    // 次に生成するまでの時間
    private float timer;

    // 画面の幅（ワールド座標）
    private float screenWidth;

    void Start()
    {
        // カメラの幅から画面の幅を計算する
        // カメラの右端のワールド座標を取得し、それを2倍することで画面全体の幅を得る
        screenWidth = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).x - Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).x;
        timer = spawnInterval;
    }

    public void Update()
    {
        // ゲームオーバー時は生成を停止
        if (FindObjectOfType<GameDirector>().isGameOver) return;
        
        // タイマーを減らす
        timer -= Time.deltaTime;

        // タイマーが0になったら食材を生成
        if (timer <= 0)
        {
            SpawnFood();
            // タイマーをリセット
            timer = spawnInterval;
        }
    }

    void SpawnFood()
    {
        // ランダムな食材のプレハブを選択
        GameObject foodToSpawn = foodPrefabs[Random.Range(0, foodPrefabs.Length)];

        // 画面の横幅内でランダムなX座標を計算
        float randomX = Random.Range(-screenWidth / 2f, screenWidth / 2f);
        
        // 生成する位置を決定
        Vector3 spawnPosition = new Vector3(randomX, Camera.main.ViewportToWorldPoint(new Vector3(0, 1.2f, 0)).y, 0);

        // 食材を生成
        GameObject clone = Instantiate(foodToSpawn, spawnPosition, Quaternion.identity);

        // 生成したオブジェクトを2秒後に自動的に削除
        Destroy(clone, 2f);
    }
}