using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaniHeisi : MonoBehaviour
{
    public int maxHP = 5;
    public int hp = 5;    // ���m��HP
    public int damage = 2; // ���m�̃_���[�W

    // �G�Ƀ_���[�W��^����֐�
    void Attack(Transform enemy)
    {
        // IDamageable �C���^�[�t�F�[�X�����G�Ƀ_���[�W��^����
        IDamageable damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }

    // �_���[�W���󂯂�֐�
    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        if (hp <= 0)
        {
            Destroy(gameObject); // HP��0�ɂȂ����畺�m��j��
        }
    }
}
