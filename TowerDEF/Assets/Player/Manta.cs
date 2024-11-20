using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manta : MonoBehaviour, IDamageable
{
    public int hp = 200; // MantaのHP
    public int normalDamage = 1; // 通常のダメージ
    public int boostedDamage = 5; // Kobanzameが近くにいる時のダメージ
    public int additionalDamageWhenKobanzameNearby = 5; // Kobanzameが近くにいる時の追加ダメージ
    public float attackRange = 5f; // 攻撃範囲
    public float attackInterval = 2f; // 攻撃間隔
    private bool isKobanzameNearby = false; // Kobanzameが近くにいるかどうかのフラグ
    private float attackTimer = 0f;

    private void Update()
    {
        // Kobanzameの状態を確認
        CheckKobanzameNearby();

        // 攻撃範囲内の敵を確認して攻撃するかどうかを決定
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            if (CheckForEnemiesInRange())
            {
                Attack();
                attackTimer = attackInterval; // 攻撃間隔をリセット
            }
        }
    }

    // ダメージを与える関数
    public int GetDamage()
    {
        return isKobanzameNearby ? boostedDamage : normalDamage;
    }

    // ダメージを受ける関数
    public void TakeDamage(int damageAmount)
    {
        // Kobanzameが近くにいる場合はダメージに追加ダメージを加算
        if (isKobanzameNearby)
        {
            damageAmount += additionalDamageWhenKobanzameNearby;
        }

        hp -= damageAmount;
        Debug.Log(gameObject.name + " が攻撃を受けました。残り体力: " + hp);

        if (hp <= 0)
        {
            Die();
        }
    }

    // Kobanzameが近くにいるか確認する関数
    private void CheckKobanzameNearby()
    {
        GameObject kobanzame = GameObject.FindWithTag("Kobanzame"); // Kobanzameのタグを持つオブジェクトを探す
        if (kobanzame != null)
        {
            float distanceToKobanzame = Vector3.Distance(transform.position, kobanzame.transform.position);
            isKobanzameNearby = distanceToKobanzame <= 5f; // 距離が5以内であればKobanzameが近くにいると判断
        }
        else
        {
            isKobanzameNearby = false;
        }
    }

    // 敵が攻撃範囲内にいるか確認する関数
    private bool CheckForEnemiesInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                return true; // 敵が範囲内にいる
            }
        }
        return false; // 敵が範囲内にいない
    }

    // 敵を攻撃する関数
    private void Attack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(GetDamage());
                    Debug.Log(gameObject.name + " が " + hitCollider.name + " に攻撃を行いました。ダメージ: " + GetDamage());
                }
            }
        }
    }

    // 死亡時の処理
    private void Die()
    {
        Debug.Log(gameObject.name + " は倒されました。");
        Destroy(gameObject);
    }
}
