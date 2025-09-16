using UnityEngine;

[CreateAssetMenu(fileName = "New Food Item", menuName = "Game Data/Food Item")]
public class FoodItemData : ScriptableObject
{
    public string foodName; // 食材の名前
    public float fullnessValue; // 満腹ゲージの増加量
    public int scoreValue; // スコアの増加量
}