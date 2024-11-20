using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kobanzame : MonoBehaviour, IDamageable
{
    [Header("Kobanzame Settings")]
    public int hp = 25; // KobanzameのHP
    public int damage = 15; // Kobanzameの与えるダメージ
    public float detectionRange = 5f; // Mantaを検出する距離

    private bool isMantaNearby = false; // Mantaが近くにいるかどうかのフラグ

    private void Update()
    {
        CheckMantaNearby();
    }

    // 敵を攻撃する関数
    public void Attack(Transform enemy)
    {
        if (enemy.CompareTag("Enemy"))
        {
            int finalDamage = isMantaNearby ? damage * 2 : damage; // Mantaが近くにいる場合は攻撃力を2倍にする
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(finalDamage);
                Debug.Log(gameObject.name + " attacks " + enemy.name + " with " + finalDamage + " damage.");
            }
        }
    }

    // ダメージを受ける関数
    public void TakeDamage(int damageAmount)
    {
        if (isMantaNearby)
        {
            Debug.Log(gameObject.name + " is protected by Manta and takes no damage.");
            return; // Mantaが近くにいる場合はダメージを受けない
        }

        hp -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Remaining HP: " + hp);

        if (hp <= 0)
        {
            Destroy(gameObject); // HPが0になったら破壊
        }
    }

    // Mantaが近くにいるか確認する関数
    private void CheckMantaNearby()
    {
        GameObject manta = GameObject.FindWithTag("Manta"); // Mantaのタグを持つオブジェクトを探す
        if (manta != null)
        {
            float distanceToManta = Vector3.Distance(transform.position, manta.transform.position);
            isMantaNearby = distanceToManta <= detectionRange;
        }
        else
        {
            isMantaNearby = false;
        }
    }
}
