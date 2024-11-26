using UnityEngine;
using System.Collections;

public class Iruka : MonoBehaviour, IDamageable
{
    public int attackDamage = 10;
    // Irukaの体力と回復関連の設定
    public int health = 80;
    public int maxHealth = 80;
    public float detectionRadius = 15f;
    public int healAmount = 5;
    public float healInterval = 5f;
    public float buffDuration = 10f;
    public float attackBuffMultiplier = 1.2f;
    private bool isBuffActive = false;

    [SerializeField]
    private GameManager gm;

    void Start()
    {
        gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager")?.GetComponent<GameManager>();

        if (gm == null)
        {
            Debug.LogError("GameManagerの参照が見つかりません。GameManagerが正しく設定されているか確認してください。");
        }

        StartCoroutine(HealAllies());
        StartCoroutine(BuffAllies());
    }

    IEnumerator HealAllies()
    {
        while (true)
        {
            yield return new WaitForSeconds(healInterval);
            ApplyHealEffect();
        }
    }

    IEnumerator BuffAllies()
    {
        while (true)
        {
            yield return new WaitForSeconds(healInterval);
            ApplyAttackBuff();
        }
    }

    void ApplyHealEffect()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Ally"))
            {
                HanaminoKasago ally = collider.GetComponent<HanaminoKasago>();
                if (ally != null)
                {
                    ally.TryHeal();
                }
            }
        }
    }

    void ApplyAttackBuff()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider collider in colliders)
        {
            HanaminoKasago ally = collider.GetComponent<HanaminoKasago>();
            if (ally != null && !ally.isBuffActive)
            {
                StartCoroutine(ApplyAttackBuffCoroutine(ally));
            }
        }
    }

    IEnumerator ApplyAttackBuffCoroutine(HanaminoKasago ally)
    {
        int originalAttackDamage = ally.attackDamage;
        ally.attackDamage = Mathf.RoundToInt(ally.attackDamage * attackBuffMultiplier);
        ally.isBuffActive = true;
        Debug.Log($"{ally.name} の攻撃力が強化されました: {ally.attackDamage}");
        while (Vector3.Distance(transform.position, ally.transform.position) <= detectionRadius)
        {
            yield return null;
        }
        ally.attackDamage = originalAttackDamage;
        ally.isBuffActive = false;
        Debug.Log($"{ally.name} の攻撃力強化が終了しました。元の攻撃力に戻りました: {ally.attackDamage}");
    }

    private void OnMouseDown()
    {
        TryHealWithFeed();
    }

    public void TryHealWithFeed()
    {
        if (gm?.SelectedFeedType.HasValue == true)
        {
            var selectedFeed = gm.SelectedFeedType.Value;

            if (selectedFeed == GameManager.ResourceType.OkiaMi ||
                selectedFeed == GameManager.ResourceType.Benthos ||
                selectedFeed == GameManager.ResourceType.Plankton)
            {
                if (gm.inventory[selectedFeed] > 0)
                {
                    if (health < maxHealth)
                    {
                        health = Mathf.Min(health + 5, maxHealth);
                        gm.inventory[selectedFeed]--;
                        gm.UpdateResourceUI();
                        Debug.Log($"{selectedFeed} で体力を回復しました。残り在庫: {gm.inventory[selectedFeed]}");
                    }
                    else
                    {
                        Debug.Log("体力は既に最大です。");
                    }
                }
                else
                {
                    Debug.Log($"{selectedFeed} の在庫が不足しています。");
                }
            }
            else
            {
                Debug.Log("この餌では回復できません。");
            }
        }
        else
        {
            Debug.Log("餌が選択されていません。");
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"{name} がダメージを受けました: {damageAmount}, 残り体力: {health}");
        if (health <= 0)
        {
            Die();
        }
    }

    public void TryHeal()
    {
        if (health < maxHealth)
        {
            health = Mathf.Min(health + healAmount, maxHealth);
            Debug.Log($"{name} の体力が {healAmount} 回復しました。現在の体力: {health}");
        }
    }

    private void Die()
    {
        Debug.Log($"{name} が倒れました。");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
