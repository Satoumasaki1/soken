using UnityEngine;
using System.Collections;

public class Iruka : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
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

    private bool seasonEffectApplied = false;

    public void OnApplicationQuit()　//追加
    {
        SaveState();
    }

    public void Upgrade(int additionalHp, int additionalDamage, int additionaRadius)//追加
    {
        health += additionalHp;
        attackDamage += additionalDamage;
        healAmount += additionaRadius;
        Debug.Log(gameObject.name + " upgraded! HP: " + health + ", Damage: " + attackDamage + ", Damage: " + healAmount);
    }

    public void SaveState()//追加
    {
        PlayerPrefs.SetInt($"{gameObject.name}_HP", health);
        PlayerPrefs.SetInt($"{gameObject.name}_Damage", attackDamage);
        PlayerPrefs.SetInt($"{gameObject.name}_Amount", healAmount);
        Debug.Log($"{gameObject.name} state saved!");
    }

    public void LoadState()//追加
    {
        if (PlayerPrefs.HasKey($"{gameObject.name}_HP"))
        {
            health = PlayerPrefs.GetInt($"{gameObject.name}_HP");
        }

        if (PlayerPrefs.HasKey($"{gameObject.name}_Damage"))
        {
            attackDamage = PlayerPrefs.GetInt($"{gameObject.name}_Damage");
        }

        if (PlayerPrefs.HasKey($"{gameObject.name}_Amount"))
        {
            healAmount = PlayerPrefs.GetInt($"{gameObject.name}_Amount");
        }

        Debug.Log($"{gameObject.name} state loaded! HP: {health}, Damage: {attackDamage}, Amount: {healAmount}");
    }

    void Start()
    {
        LoadState();//追加

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

    // シーズンの効果を適用するメソッド
    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                maxHealth += 10;
                health = Mathf.Min(health + 10, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.1f);
                Debug.Log("春のバフが適用されました: 体力と攻撃力が少し上昇");
                break;
            case GameManager.Season.Summer:
                maxHealth -= 5;
                health = Mathf.Min(health, maxHealth);
                Debug.Log("夏のデバフが適用されました: 体力が減少");
                break;
            case GameManager.Season.Autumn:
                maxHealth += 15;
                health = Mathf.Min(health + 15, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                Debug.Log("秋のバフが適用されました: 体力と攻撃力が上昇");
                break;
            case GameManager.Season.Winter:
                maxHealth -= 10;
                health = Mathf.Min(health, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                Debug.Log("冬のデバフが適用されました: 体力と攻撃力が減少");
                break;
        }

        seasonEffectApplied = true;
    }

    // シーズンの効果をリセットするメソッド
    public void ResetSeasonEffect()
    {
        maxHealth = 80;
        health = Mathf.Min(health, maxHealth);
        attackDamage = 10;
        seasonEffectApplied = false;
        Debug.Log("シーズン効果がリセットされました。");
    }
}
