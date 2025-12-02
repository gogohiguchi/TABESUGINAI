using UnityEngine;

public class FoodItem : MonoBehaviour
{
    // ScriptableObjectへの参照を保持 (インスペクターで設定)
    public FoodItemData foodData;

    // インスペクターから設定できる、この食材の重力スケール
    [Tooltip("Rigidbody2Dに適用する重力スケール")]
    public float gravityScale = 1.0f;

    void Start()
    {
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

            if (director != null && foodData != null)
            {
                // 満腹度、スコア、特殊アイテムのタイプをすべて渡す
                director.EatFood(
                    foodData.fullnessValue,
                    foodData.scoreValue,
                    foodData.whatspecialItem
                );
            }

            // 食材を消費したので、このゲームオブジェクトを削除
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("プレイヤー以外のオブジェクトと衝突しました。タグ: " + other.tag);
        }
    }
}