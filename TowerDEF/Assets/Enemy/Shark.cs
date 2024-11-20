using System.Collections;
using UnityEngine;

public class Shark : MonoBehaviour, IDamageable
{
    [Header("Shark Stats")]
    public float speed = 5.0f; // 移動速度（ナーフ済み）
    public int attackPower = 50; // 攻撃力（ナーフ済み）
    public int health = 150; // 体力（半分に変更）

    [Header("Enrage Settings")]
    public float enragedSpeedMultiplier = 2.0f;
    public int enragedAttackMultiplier = 2;
    public float specialAttackPreparationTime = 5.0f;
    public float specialAttackCooldownTime = 15.0f;

    private Transform targetAlly;
    private Transform targetBase;
    private Transform currentTarget;
    private bool isStunned = false;
    private bool enraged = false;

    private void Start()
    {
        SetInitialTargets();
    }

    private void Update()
    {
        if (isStunned) return; // スタン中は動かない

        UpdateTarget();

        if (health <= 0.15f * 150 && !enraged)
        {
            Enrage();
        }

        if (currentTarget != null)
        {
            MoveTowardsTarget(currentTarget);
        }
    }

    private void SetInitialTargets()
    {
        targetAlly = GameObject.FindGameObjectWithTag("Ally")?.transform;
        targetBase = GameObject.FindGameObjectWithTag("Base")?.transform;
        currentTarget = targetAlly != null ? targetAlly : targetBase;
    }

    private void UpdateTarget()
    {
        if (currentTarget == null || (!currentTarget.CompareTag("Ally") && !currentTarget.CompareTag("Base")))
        {
            SetInitialTargets();
        }
    }

    private void MoveTowardsTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void Enrage()
    {
        enraged = true;
        speed *= enragedSpeedMultiplier;
        attackPower *= enragedAttackMultiplier;
        Debug.Log($"{gameObject.name} が激怒状態になりました！速度と攻撃力が上昇しました。");
        StartCoroutine(SpecialAttackCooldown());
    }

    private IEnumerator SpecialAttack()
    {
        Debug.Log($"{gameObject.name} が特殊攻撃を準備中...");
        yield return new WaitForSeconds(specialAttackPreparationTime);

        if (currentTarget != null)
        {
            IDamageable damageable = currentTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                int specialAttackPower = attackPower * 3; // 特殊攻撃は通常攻撃の3倍のダメージ
                damageable.TakeDamage(specialAttackPower);
                Debug.Log($"{gameObject.name} が {currentTarget.name} に特殊攻撃を行いました。ダメージ: {specialAttackPower}");
            }
        }
    }

    private IEnumerator SpecialAttackCooldown()
    {
        yield return SpecialAttack();
        yield return new WaitForSeconds(specialAttackCooldownTime);
        Debug.Log($"{gameObject.name} が特殊攻撃のクールダウンを終了しました。");
        StartCoroutine(SpecialAttack());
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"{gameObject.name} が攻撃を受けました。残り体力: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} は倒されました。");
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Base") || other.CompareTag("Ally"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackPower);
                Debug.Log($"{gameObject.name} が {other.name} に攻撃を行いました。ダメージ: {attackPower}");
            }
        }
    }
}
