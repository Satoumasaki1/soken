using UnityEngine;

public class GathererCharacter : MonoBehaviour
{
    public float attackRange = 2f;         // �U���͈�
    public float attackInterval = 1f;      // �U���Ԋu
    private float attackTimer = 0f;
    public InventoryManager inventory;     // �C���x���g���Ǘ��X�N���v�g
    public int maxHP = 30;                 // �L�����N�^�[�̍ő�HP
    private int currentHP;                 // ���݂�HP

    private void Start()
    {
        currentHP = maxHP; // HP��������
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;

        // �߂��̔j��\�I�u�W�F�N�g��T���čU��
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            BreakableObject target = hitCollider.GetComponent<BreakableObject>();
            if (target != null && attackTimer >= attackInterval)
            {
                Attack(target);
                attackTimer = 0f;
            }
        }
    }

    private void Attack(BreakableObject target)
    {
        target.TakeDamage();
        currentHP--;

        // HP��0�ȉ��̏ꍇ�A�L�����N�^�[���폜
        if (currentHP <= 0)
        {
            Debug.Log("�L�����N�^�[�͗͐s���܂���");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // �h���b�v�����A�C�e�������o���ăC���x���g���ɒǉ�
        if (other.CompareTag("Material"))
        {
            inventory.AddItem(other.gameObject);
            Destroy(other.gameObject);
        }
    }
}
