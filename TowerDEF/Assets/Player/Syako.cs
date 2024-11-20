using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Syako : MonoBehaviour, IDamageable
{
    [Header("Syako Settings")]
    public int hp = 100; // Syako�̗̑́i���̗́j
    public int attackPower = 20; // Syako�̍U���́i���U���́j
    public float attackCooldown = 10f; // �U���N�[���_�E���i�����U���Ԋu�j
    private float attackTimer; // �U���^�C�}�[

    private void Start()
    {
        attackTimer = attackCooldown; // �^�C�}�[��������
    }

    private void Update()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);
            return;
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackCooldown; // �^�C�}�[�����Z�b�g
        }
    }

    private void Attack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f); // �U���͈͓��̓G�����o
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackPower);
                    Debug.Log("Syako attacks " + collider.name + " with " + attackPower + " damage!");
                }
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        Debug.Log(gameObject.name + " ���U�����󂯂܂����B�c��̗�: " + hp);

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}


