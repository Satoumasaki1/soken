using UnityEngine;
using System.Collections;

public class HanaminoKasago : MonoBehaviour, IDamageable, ISeasonEffect
{
    // HanaminoKasagoの体力と最大体力
    public float buffMultiplier = 1.5f; // 攻撃バフの倍率
    public int health = 20;
    public int maxHealth = 20;

    // 麻痺毒関連の設定
    public float detectionRadius = 15f; // 麻痺毒の範囲
    public float poisonDamage = 1.0f;   // 継続ダメージ
    public float effectInterval = 1.0f; // 継続ダメージの間隔

    private float lastEffectTime;
    public bool isBuffActive = false; // バフが有効かどうかのフラグ
    private int originalAttackDamage = 10;
    public int attackDamage = 10; // 攻撃力

    [SerializeField]
    private GameManager gm;

    private bool seasonEffectApplied = false;

    void Start()
    {
        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        if (gm == null)
        {
            Debug.LogError("GameManagerの参照が見つかりません。GameManagerが正しく設定されているか確認してください。");
        }
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;
        ApplyParalyticPoisonEffect();
        ApplyIrukaBuff();
    }

    private void OnMouseDown()
    {
        TryHeal();
    }

    public void TryHeal()
    {
        if (!gm.SelectedFeedType.HasValue) { Debug.Log("餌が選択されていません。"); return; }

        var selectedFeed = gm.SelectedFeedType.Value;
        if (selectedFeed == GameManager.ResourceType.OkiaMi ||
            selectedFeed == GameManager.ResourceType.Benthos ||
            selectedFeed == GameManager.ResourceType.Plankton)
        {
            if (gm.inventory[selectedFeed] > 0 && health < maxHealth)
            {
                health = Mathf.Min(health + 2, maxHealth);
                gm.inventory[selectedFeed]--;
                gm.UpdateResourceUI();
                Debug.Log($"{selectedFeed} で体力を回復しました。残り在庫: {gm.inventory[selectedFeed]}");
            }
            else
            {
                Debug.Log(health >= maxHealth ? "体力は既に最大です。" : $"{selectedFeed} の在庫が不足しています。");
            }
        }
        else
        {
            Debug.Log("この餌では回復できません。");
        }
    }

    public void ApplyParalyticPoisonEffect()
    {
        if (Time.time > lastEffectTime + effectInterval)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            Debug.Log("麻痺毒の効果を適用中...");
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    IDamageable enemy = collider.GetComponent<IDamageable>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage((int)poisonDamage);
                        Debug.Log($"{collider.name} に麻痺毒の継続ダメージを与えました。");
                    }

                    // 麻痺毒処理を適用
                    if (collider.TryGetComponent(out Ikasan ikasan))
                    {
                        ikasan.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} に麻痺毒を適用しました。");
                    }
                    else if (collider.TryGetComponent(out OOKAMIUO ookamiuo))
                    {
                        ookamiuo.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} に麻痺毒を適用しました。");
                    }
                    else if (collider.TryGetComponent(out SAME same))
                    {
                        same.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} に麻痺毒を適用しました。");
                    }
                    else if (collider.TryGetComponent(out UTUBO utubo))
                    {
                        utubo.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} に麻痺毒を適用しました。");
                    }
                    else if (collider.TryGetComponent(out AKAEI akaei))
                    {
                        akaei.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} に麻痺毒を適用しました。");
                    }
                    else if (collider.TryGetComponent(out ISEEBI iseebi))
                    {
                        iseebi.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} に麻痺毒を適用しました。");
                    }
                    else if (collider.TryGetComponent(out KAZIKI kaziki))
                    {
                        kaziki.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} に麻痺毒を適用しました。");
                    }
                    else if (collider.TryGetComponent(out ONIDARUMA_OKOZE onidaruma_okoze))
                    {
                        onidaruma_okoze.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} に麻痺毒を適用しました。");
                    }
                    else if (collider.TryGetComponent(out ONIKAMASU onikamasu))
                    {
                        onikamasu.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} に麻痺毒を適用しました。");
                    }
                }
            }
            lastEffectTime = Time.time;
        }
    }

    // イルカのバフが有効か確認して適用する処理
    private void ApplyIrukaBuff()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool irukaNearby = false;
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out Iruka iruka))
            {
                irukaNearby = true;
                if (!isBuffActive)
                {
                    isBuffActive = true;
                    attackDamage = Mathf.RoundToInt(originalAttackDamage * buffMultiplier);
                    Debug.Log($"{name} の攻撃力が強化されました: {attackDamage}");
                }
                break;
            }
        }

        if (!irukaNearby && isBuffActive)
        {
            isBuffActive = false;
            attackDamage = originalAttackDamage;
            Debug.Log($"{name} の攻撃力強化が終了しました。元の攻撃力に戻りました: {attackDamage}");
        }
    }

    // ダメージを受けたときの処理
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    // HanaminoKasagoが倒れたときの処理
    private void Die()
    {
        Destroy(gameObject); // オブジェクトを破壊
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
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
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 1.1f);
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
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 1.2f);
                Debug.Log("秋のバフが適用されました: 体力と攻撃力が上昇");
                break;
            case GameManager.Season.Winter:
                maxHealth -= 10;
                health = Mathf.Min(health, maxHealth);
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 0.9f);
                Debug.Log("冬のデバフが適用されました: 体力と攻撃力が減少");
                break;
        }

        seasonEffectApplied = true;
    }

    // シーズンの効果をリセットするメソッド
    public void ResetSeasonEffect()
    {
        maxHealth = 20;
        health = Mathf.Min(health, maxHealth);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
        Debug.Log("シーズン効果がリセットされました。");
    }
}
