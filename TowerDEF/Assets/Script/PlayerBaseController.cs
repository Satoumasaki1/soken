using UnityEngine;

public class PlayerBaseController : MonoBehaviour, IDamageable
{
    public int baseHealth = 100;
    public GameObject gameOverPanel;

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // プレイヤー拠点が攻撃された際のダメージ処理
    public void TakeDamage(int damage)
    {
        baseHealth -= damage;
        Debug.Log("プレイヤーの拠点が攻撃されました。残り体力: " + baseHealth);

        if (baseHealth <= 0)
        {
            GameOver();
        }
    }

    // ゲームオーバー時の処理
    private void GameOver()
    {
        Debug.Log("ゲームオーバー。プレイヤーの拠点が破壊されました。");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f; // ゲームを一時停止
        // 必要に応じてゲームをリセットする、または別のシーンに遷移する処理を追加できます。
    }
}
