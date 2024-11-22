using UnityEngine;
using UnityEngine.AI;

public class UTUBO : MonoBehaviour, IDamageable
{
    public string primaryTargetTag = "koukaku"; // 優先ターゲットのタグを設定
    public string secondaryTargetTag = "Ally"; // 次に優先するターゲットのタグ
    public string fallbackTag = "Base"; // 最後に狙うターゲットのタグ

    private Transform target; // ターゲットのTransform
    public int health = 35; // UTUBOの体力
    public int attackDamage = 10; // 攻撃力
    public float attackRange = 4f; // 攻撃範囲
    public float attackCooldown = 3f; // 攻撃クールダウン時間
    public float biteChargeTime = 2f; // 連続噛みつきのチャージ時間

    private float lastAttackTime;
    private NavMeshAgent agent;
    private bool isChargingBite = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        FindTarget();
    }

    void Update()
    {
        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(secondaryTargetTag) && !target.CompareTag(fallbackTag)))
        {
            FindTarget();
        }

        if (target != null)
        {
            // ターゲットに向かって移動する
            agent.SetDestination(target.position);

            // 攻撃範囲内にターゲットがいる場合、攻撃の準備をする
            if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time > lastAttackTime + attackCooldown && !isChargingBite)
            {
                StartCoroutine(ChargeAndBite());
            }
        }
    }

    void FindTarget()
    {
        // 優先ターゲット（koukakuタグ）を探す
        GameObject koukakuTarget = GameObject.FindGameObjectWithTag(primaryTargetTag);
        if (koukakuTarget != null)
        {
            target = koukakuTarget.transform;
            return;
        }

        // 次に優先するターゲット（Allyタグ）を探す
        GameObject allyTarget = GameObject.FindGameObjectWithTag(secondaryTargetTag);
        if (allyTarget != null)
        {
            target = allyTarget.transform;
            return;
        }

        // それでも見つからない場合、Baseタグのターゲットを探す
        GameObject baseTarget = GameObject.FindGameObjectWithTag(fallbackTag);
        if (baseTarget != null)
        {
            target = baseTarget.transform;
        }
    }

    System.Collections.IEnumerator ChargeAndBite()
    {
        isChargingBite = true;
        yield return new WaitForSeconds(biteChargeTime);
        AttackTarget();
        lastAttackTime = Time.time;
        isChargingBite = false;
    }

    void AttackTarget()
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // UTUBOが倒れた際の処理（例えば破壊など）
        Destroy(gameObject);
    }
}
