using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kobanzame : MonoBehaviour, IDamageable
{
    [Header("Kobanzame Settings")]
    public int hp = 25; // Kobanzame��HP
    public int damage = 15; // Kobanzame�̗^����_���[�W
    public float detectionRange = 5f; // Manta�����o���鋗��

    private bool isMantaNearby = false; // Manta���߂��ɂ��邩�ǂ����̃t���O

    private void Update()
    {
        CheckMantaNearby();
    }

    // �G���U������֐�
    public void Attack(Transform enemy)
    {
        if (enemy.CompareTag("Enemy"))
        {
            int finalDamage = isMantaNearby ? damage * 2 : damage; // Manta���߂��ɂ���ꍇ�͍U���͂�2�{�ɂ���
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(finalDamage);
                Debug.Log(gameObject.name + " attacks " + enemy.name + " with " + finalDamage + " damage.");
            }
        }
    }

    // �_���[�W���󂯂�֐�
    public void TakeDamage(int damageAmount)
    {
        if (isMantaNearby)
        {
            Debug.Log(gameObject.name + " is protected by Manta and takes no damage.");
            return; // Manta���߂��ɂ���ꍇ�̓_���[�W���󂯂Ȃ�
        }

        hp -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Remaining HP: " + hp);

        if (hp <= 0)
        {
            Destroy(gameObject); // HP��0�ɂȂ�����j��
        }
    }

    // Manta���߂��ɂ��邩�m�F����֐�
    private void CheckMantaNearby()
    {
        GameObject manta = GameObject.FindWithTag("Manta"); // Manta�̃^�O�����I�u�W�F�N�g��T��
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
