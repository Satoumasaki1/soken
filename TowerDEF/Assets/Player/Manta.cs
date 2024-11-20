using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manta : MonoBehaviour, IDamageable
{
    public int hp = 200; // Manta��HP
    public int normalDamage = 1; // �ʏ�̃_���[�W
    public int boostedDamage = 5; // Kobanzame���߂��ɂ��鎞�̃_���[�W
    public int additionalDamageWhenKobanzameNearby = 5; // Kobanzame���߂��ɂ��鎞�̒ǉ��_���[�W
    public float attackRange = 5f; // �U���͈�
    public float attackInterval = 2f; // �U���Ԋu
    private bool isKobanzameNearby = false; // Kobanzame���߂��ɂ��邩�ǂ����̃t���O
    private float attackTimer = 0f;

    private void Update()
    {
        // Kobanzame�̏�Ԃ��m�F
        CheckKobanzameNearby();

        // �U���͈͓��̓G���m�F���čU�����邩�ǂ���������
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            if (CheckForEnemiesInRange())
            {
                Attack();
                attackTimer = attackInterval; // �U���Ԋu�����Z�b�g
            }
        }
    }

    // �_���[�W��^����֐�
    public int GetDamage()
    {
        return isKobanzameNearby ? boostedDamage : normalDamage;
    }

    // �_���[�W���󂯂�֐�
    public void TakeDamage(int damageAmount)
    {
        // Kobanzame���߂��ɂ���ꍇ�̓_���[�W�ɒǉ��_���[�W�����Z
        if (isKobanzameNearby)
        {
            damageAmount += additionalDamageWhenKobanzameNearby;
        }

        hp -= damageAmount;
        Debug.Log(gameObject.name + " ���U�����󂯂܂����B�c��̗�: " + hp);

        if (hp <= 0)
        {
            Die();
        }
    }

    // Kobanzame���߂��ɂ��邩�m�F����֐�
    private void CheckKobanzameNearby()
    {
        GameObject kobanzame = GameObject.FindWithTag("Kobanzame"); // Kobanzame�̃^�O�����I�u�W�F�N�g��T��
        if (kobanzame != null)
        {
            float distanceToKobanzame = Vector3.Distance(transform.position, kobanzame.transform.position);
            isKobanzameNearby = distanceToKobanzame <= 5f; // ������5�ȓ��ł����Kobanzame���߂��ɂ���Ɣ��f
        }
        else
        {
            isKobanzameNearby = false;
        }
    }

    // �G���U���͈͓��ɂ��邩�m�F����֐�
    private bool CheckForEnemiesInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                return true; // �G���͈͓��ɂ���
            }
        }
        return false; // �G���͈͓��ɂ��Ȃ�
    }

    // �G���U������֐�
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
                    Debug.Log(gameObject.name + " �� " + hitCollider.name + " �ɍU�����s���܂����B�_���[�W: " + GetDamage());
                }
            }
        }
    }

    // ���S���̏���
    private void Die()
    {
        Debug.Log(gameObject.name + " �͓|����܂����B");
        Destroy(gameObject);
    }
}
