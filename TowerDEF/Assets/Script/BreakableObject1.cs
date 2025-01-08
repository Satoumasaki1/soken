using UnityEngine;

public class BreakableObject1 : MonoBehaviour
{
    public int hp = 5; // �q�b�g�|�C���g

    // IsBroken�v���p�e�B�̒ǉ�
    public bool IsBroken
    {
        get { return hp <= 0; } // hp��0�ȉ��Ȃ�j��ς݂Ɣ���
    }

    public void TakeDamage()
    {
        hp--; // �_���[�W���󂯂��hp������
        if (hp <= 0)
        {
            Debug.Log($"{gameObject.name} ���j�󂳂�܂����I");
            Destroy(gameObject); // �I�u�W�F�N�g��j��
        }
    }
}