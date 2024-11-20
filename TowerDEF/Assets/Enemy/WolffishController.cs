using System.Collections;
using UnityEngine;

public class WolffishController : MonoBehaviour, IDamageable
{
    public float speed = 1.0f; // 速度は遅い
    public float attackRange = 1.0f;
    public int attackDamage = 25; // 一撃が強い
    public float attackCooldown = 1.5f;
    public float frenzyAttackCooldown = 3.0f; // 連続噛みつきのクールダウンを長めに設定
    public float frenzyDuration = 3.0f; // 連続噛みつきの持続時間

    public GameObject target;
    private float attackCooldownTimer;
    private bool isInFrenzy;

    public int hp = 50; // Wolffishの体力

    private void Start()
    {
        // ターゲットを最初に設定
        FindNearestTarget();
    }

    private void Update()
    {
        if (target != null)
        {
            MoveTowardsTarget();
            AttackTargetIfInRange();
        }
        else
        {
            // ターゲットがなくなったら新しいターゲットを探す
            FindNearestTarget();
        }
    }

    public GameObject GetTarget()
    {
        return target;
    }

    private void FindNearestTarget()
    {
        // koukakuタグを持つ生物を優先してターゲットにする
        GameObject[] koukakuTargets = GameObject.FindGameObjectsWithTag("koukaku");
        float nearestDistance = Mathf.Infinity;
        GameObject nearestKoukaku = null;

        foreach (GameObject koukaku in koukakuTargets)
        {
            float distance = Vector3.Distance(transform.position, koukaku.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestKoukaku = koukaku;
            }
        }

        if (nearestKoukaku != null)
        {
            target = nearestKoukaku;
        }
        else
        {
            // koukakuタグの生物がいない場合はプレイヤーの拠点をターゲットにする
            target = GameObject.FindGameObjectWithTag("Base");
        }
    }

    private void MoveTowardsTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void AttackTargetIfInRange()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange && attackCooldownTimer <= 0f)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log(gameObject.name + " attacks " + target.name + " with " + attackDamage + " damage.");
            }

            if (Random.value < 0.3f) // 一定確率で連続噛みつきを発動
            {
                StartCoroutine(FrenzyAttack());
            }
            else
            {
                attackCooldownTimer = attackCooldown;
            }
        }

        // クールダウンを減らす
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    private IEnumerator FrenzyAttack()
    {
        isInFrenzy = true;
        float frenzyTimer = frenzyDuration;

        while (frenzyTimer > 0)
        {
            if (target != null && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
            {
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage);
                    Debug.Log(gameObject.name + " frenzy attacks " + target.name + " with " + attackDamage + " damage.");
                }
            }
            frenzyTimer -= frenzyAttackCooldown;
            yield return new WaitForSeconds(frenzyAttackCooldown);
        }

        isInFrenzy = false;
        attackCooldownTimer = attackCooldown;
    }

    // ダメージを受ける関数
    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Remaining HP: " + hp);

        if (hp <= 0)
        {
            Destroy(gameObject); // HPが0になったら破壊
        }
    }
}


