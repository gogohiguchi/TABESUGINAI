using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void OnClickNextScene()
    {
        SceneManager.LoadScene("aitemuScene");
        // 例: SceneManager.LoadScene("GameScene");
    }
}
