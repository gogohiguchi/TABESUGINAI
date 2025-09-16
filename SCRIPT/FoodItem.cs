using UnityEngine;

public class FoodItem : MonoBehaviour
{
    // ScriptableObjectへの参照を保持
    public FoodItemData foodData; 
    
    // インスペクターから設定できる、この食材の重力スケール
    public float gravityScale = 1.0f; // デフォルト値を1に設定

    private GameDirector gameDirector; 

    void Start()
    {
        gameDirector = FindObjectOfType<GameDirector>();
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = gravityScale;
        }
    }

    // 他のオブジェクトと衝突したときに呼び出される
    void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーに当たったかチェック
        if (other.CompareTag("Player"))
        {
            GameDirector director = GameDirector.Instance;

            if (gameDirector != null)
            {
                gameDirector.EatFood(foodData.fullnessValue, foodData.scoreValue);
            }
            
            Destroy(gameObject); 
        }
        else
        {
            // プレイヤー以外のオブジェクトと衝突した場合のログ
            Debug.Log("プレイヤー以外のオブジェクトと衝突しました。タグ: " + other.tag);
        }
    }
}