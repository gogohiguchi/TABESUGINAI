using UnityEngine;

[CreateAssetMenu(fileName = "New Food Item", menuName = "Game Data/Food Item")]
public class FoodItemData : ScriptableObject
{
    public string foodName;         // 食材の名前
    public float fullnessValue;      // 満腹ゲージの増加量
    public int scoreValue;          // スコアの増加量

    [Tooltip("特殊アイテムの場合、効果を識別する文字列を設定 ('TimeStop', 'Boost', 'MaxOut' など)")]
    public string whatspecialItem;  // 特殊アイテムか否か、またその種類

    [Header("ドロップ設定")]
    [Tooltip("このアイテムが出現する重み。値が大きいほど出やすい。")]
    public float dropWeight = 1f;
}