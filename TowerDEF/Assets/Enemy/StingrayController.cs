using System.Collections;
using UnityEngine;

public class StingrayController : MonoBehaviour
{
    public int health = 20;
    public float speed = 1.5f; // 速度は遅い
    public float attackRange = 1.0f;
    public int attackDamage = 15;
    public float poisonDuration = 5.0f; // 毒の継続時間
    public int poisonDamagePerSecond = 2;
    public float attackCooldown = 2.0f;

    private GameObject target;
    private float attackCooldownTimer;

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

    private void FindNearestTarget()
    {
        // こくかくタグを持つ生物を優先してターゲットにする
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
            // こくかくタグの生物がいない場合はプレイヤーの拠点をターゲットにする
            target = GameObject.FindGameObjectWithTag("Base");
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void AttackTargetIfInRange()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange && attackCooldownTimer <= 0f)
        {
            if (target.CompareTag("Base"))
            {
                // プレイヤーの拠点に攻撃する
                target.GetComponent<PlayerBaseController>().TakeDamage(attackDamage);
                ApplyPoison(target);
            }
            else if (target.CompareTag("koukaku"))
            {
                // こくかくタグの生物に攻撃する
                target.GetComponent<Health>().TakeDamage(attackDamage);
                ApplyPoison(target);
            }
            attackCooldownTimer = attackCooldown;
        }

        // クールダウンを減らす
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    private void ApplyPoison(GameObject target)
    {
        PoisonEffect poison = target.GetComponent<PoisonEffect>();
        if (poison == null)
        {
            poison = target.AddComponent<PoisonEffect>();
        }
        poison.ApplyPoison(poisonDuration, poisonDamagePerSecond);
    }

    // ダメージを受けた際の処理
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " は倒されました");
        Destroy(gameObject);
    }
}

public class PoisonEffect : MonoBehaviour
{
    private float poisonTimer;
    private int poisonDamage;
    private bool isPoisoned;

    public void ApplyPoison(float duration, int damagePerSecond)
    {
        poisonTimer = duration;
        poisonDamage = damagePerSecond;
        if (!isPoisoned)
        {
            isPoisoned = true;
            StartCoroutine(ApplyPoisonDamage());
        }
    }

    private IEnumerator ApplyPoisonDamage()
    {
        while (poisonTimer > 0)
        {
            GetComponent<Health>().TakeDamage(poisonDamage);
            poisonTimer -= 1.0f;
            yield return new WaitForSeconds(1.0f);
        }
        isPoisoned = false;
    }
}
