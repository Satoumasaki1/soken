using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Choucho : MonoBehaviour
{
    public float healRange = 10f;  // �q�[���[�̉񕜔͈�
    public int healerHP = 150;      // �q�[���[��HP
    public int healAmount = 50;     // �q�[����
    public float healCooldown = 3f; // �q�[���̃N�[���_�E��
    private float healTimer = 0f;   // �q�[���^�C�}�[

    void Update()
    {
        // �q�[���[��HP��0�Ȃ�j��
        if (healerHP <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // �q�[���^�C�}�[���X�V
        healTimer -= Time.deltaTime;
        if (healTimer <= 0f)
        {
            HealBarracksAndAllies();
            healTimer = healCooldown; // �q�[���^�C�}�[�����Z�b�g
        }
    }

    // ���ɂ□�����񕜂���֐�
    void HealBarracksAndAllies()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, healRange);
        foreach (Collider collider in colliders)
        {
            Kaniheisya barracks = collider.GetComponent<Kaniheisya>();

            if (barracks != null && barracks.hp < 20)  // ���ɂ�HP���ő�łȂ��ꍇ
            {
                barracks.hp += healAmount;
                Debug.Log(gameObject.name + " healed " + barracks.name + " for " + healAmount + " HP.");
            }
            else if (collider.CompareTag("Ally") || collider.CompareTag("koukaku"))  // Ally�܂���koukaku�^�O����������HP���ő�łȂ��ꍇ
            {
                Debug.Log(gameObject.name + " healed " + collider.name + " for " + healAmount + " HP.");
            }
        }
    }
}
