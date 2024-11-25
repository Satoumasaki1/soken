using UnityEngine;

public class HanaminoKasago : MonoBehaviour, IDamageable
{
    // HanaminoKasagoの体力と最大体力
    public int health = 20;
    public int maxHealth = 20;

    // 麻痺毒関連の設定
    public float detectionRadius = 15f; // 麻痺毒の範囲
    public float poisonDamage = 1.0f;   // 継続ダメージ
    public float effectInterval = 1.0f; // 継続ダメージの間隔

    private float lastEffectTime;

    [SerializeField]
    private GameManager gm;

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
}
