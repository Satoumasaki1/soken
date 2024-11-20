using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour, IDamageable
{
    [Header("Healer Settings")]
    public float healRange = 10f;  // ヒーラーの回復範囲
    public int healerHP = 50;       // ヒーラーのHP
    public int healAmount = 50;     // ヒール量
    public float healCooldown = 3f; // ヒールのクールダウン
    private float healTimer = 0f;   // ヒールタイマー

    public void TakeDamage(int damageAmount)
    {
        healerHP -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Remaining HP: " + healerHP);
    }

    void Update()
    {
        if (healerHP <= 0)
        {
            Destroy(gameObject);
            return;
        }

        healTimer -= Time.deltaTime;
        if (healTimer <= 0f)
        {
            HealAllies();
            healTimer = healCooldown;
        }
    }

    // Ally と koukaku タグを持つ味方を回復する関数
    void HealAllies()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, healRange);
        foreach (Collider collider in colliders)
        {
            if ((collider.CompareTag("Ally") || collider.CompareTag("koukaku")) && collider.GetComponent<IDamageable>() != null)
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                damageable.TakeDamage(healAmount * -1);  // 回復は負のダメージとして扱う
                Debug.Log(gameObject.name + " healed " + collider.name + " for " + healAmount + " HP.");
            }
        }
    }
}

