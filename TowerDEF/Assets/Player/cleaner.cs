using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleaner : MonoBehaviour
{
    public float healRange = 10f;  // �q�[���[�̉񕜔͈�
    public int healAmount = 2;     // 1�b���Ƃ̉񕜗�
    public int healerHP = 10;      // �q�[���[��HP
    private float healInterval = 1f;  // 1�b��1��̉�
    private float healTimer = 0f;  // �񕜗p�̃^�C�}�[

    void Update()
    {
        healTimer -= Time.deltaTime;

        if (healTimer <= 0f)
        {
            HealSoldiers();  // �͈͓��̕��m����
            healTimer = healInterval;  // �^�C�}�[���Z�b�g
        }

        // �q�[���[��HP��0�ȉ��Ȃ�j��
        if (healerHP <= 0)
        {
            Destroy(gameObject);
        }
    }

    // �͈͓��̕��m���񕜂���֐�
    void HealSoldiers()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, healRange);
        foreach (Collider collider in colliders)
        {
            KaniHeisi soldier = collider.GetComponent<KaniHeisi>();
            if (soldier != null && soldier.hp < soldier.maxHP)
            {
                soldier.hp = Mathf.Min(soldier.hp + healAmount, soldier.maxHP);  // �J�j���m��HP����
            }

            Tpuebi soldier2 = collider.GetComponent<Tpuebi>();
            if (soldier != null && soldier.hp < soldier.maxHP)
            {
                soldier.hp = Mathf.Min(soldier.hp + healAmount, soldier.maxHP);  // �S�C�G�r��HP����
            }

            Uni soldier3 = collider.GetComponent<Uni>();
            if (soldier != null && soldier.hp < soldier.maxHP)
            {
                soldier.hp = Mathf.Min(soldier.hp + healAmount, soldier.maxHP);  // �E�j��HP����
            }
        }
    }

    // �_���[�W���󂯂�֐�
    public void TakeDamage(int damageAmount)
    {
        healerHP -= damageAmount;
    }
}
