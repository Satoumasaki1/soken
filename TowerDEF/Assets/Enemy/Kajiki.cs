using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kajiki : MonoBehaviour, IDamageable
{
    public float speed = 6.0f; // 移動速度（速い）
    public int attackPower = 30; // 攻撃力（高い）
    public int health = 150; // 体力（中）
    public float dashSpeedMultiplier = 10.0f; // 突進時の速度倍率
    public float dashInterval = 10.0f; // 突進を行う間隔（秒）
    public float stunDuration = 2.0f; // 突進後の停止時間（秒）
    public float dashDistance = 10.0f; // 突進する距離

    private Transform targetAlly;
    private Transform targetBase;
    private Transform currentTarget;
    private float dashTimer;
    private bool isStunned = false;
    private bool isDashing = false;

    private void Start()
    {
        targetAlly = GameObject.FindGameObjectWithTag("Ally")?.transform;
        targetBase = GameObject.FindGameObjectWithTag("Base").transform;
        currentTarget = targetAlly != null ? targetAlly : targetBase;
        dashTimer = dashInterval;
    }

    private void Update()
    {
        if (isStunned || isDashing)
        {
            return; // スタン中または突進中は動かない
        }

        if (currentTarget == null || (!currentTarget.CompareTag("Ally") && !currentTarget.CompareTag("Base")))
        {
            // 現在のターゲットがなくなった場合、再度ターゲットを探す
            targetAlly = GameObject.FindGameObjectWithTag("Ally")?.transform;
            currentTarget = targetAlly != null ? targetAlly : targetBase;
        }

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            StartCoroutine(DashTowardsTarget());
            dashTimer = dashInterval;
        }
        else if (currentTarget != null)
        {
            MoveTowardsTarget(currentTarget);
        }
    }

    private void MoveTowardsTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private IEnumerator DashTowardsTarget()
    {
        if (currentTarget != null)
        {
            isDashing = true;
            Vector3 startPosition = transform.position;
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            float distanceTraveled = 0f;

            while (distanceTraveled < dashDistance)
            {
                float step = speed * dashSpeedMultiplier * Time.deltaTime;
                transform.position += direction * step;
                distanceTraveled += step;
                yield return null;
            }

            health = Mathf.Max(0, health - (int)(150 * 0.1f)); // 最大体力の10%を減少させる
            Debug.Log(gameObject.name + " が高速突進を行いました。残り体力: " + health);

            isDashing = false;
            StartCoroutine(StunAfterDash());
        }
    }

    private IEnumerator StunAfterDash()
    {
        isStunned = true;
        Debug.Log(gameObject.name + " は突進後に停止しています。");
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        Debug.Log(gameObject.name + " が再び動き始めました。");
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log(gameObject.name + " が攻撃を受けました。残り体力: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " は倒されました。");
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
                Debug.Log(gameObject.name + " が " + other.name + " に攻撃を行いました。ダメージ: " + attackPower);
            }
        }
    }
}
