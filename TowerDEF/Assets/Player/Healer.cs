using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour, IDamageable
{
    [Header("Healer Settings")]
    public float healRange = 10f;  // �q�[���[�̉񕜔͈�
    public int healerHP = 50;       // �q�[���[��HP
    public int healAmount = 50;     // �q�[����
    public float healCooldown = 3f; // �q�[���̃N�[���_�E��
    private float healTimer = 0f;   // �q�[���^�C�}�[

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

    // Ally �� koukaku �^�O�����������񕜂���֐�
    void HealAllies()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, healRange);
        foreach (Collider collider in colliders)
        {
            if ((collider.CompareTag("Ally") || collider.CompareTag("koukaku")) && collider.GetComponent<IDamageable>() != null)
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                damageable.TakeDamage(healAmount * -1);  // �񕜂͕��̃_���[�W�Ƃ��Ĉ���
                Debug.Log(gameObject.name + " healed " + collider.name + " for " + healAmount + " HP.");
            }
        }
    }
}

