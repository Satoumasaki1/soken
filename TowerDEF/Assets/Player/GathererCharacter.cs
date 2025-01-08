using UnityEngine;

public class GathererCharacter : MonoBehaviour
{
    public float attackRange = 2f;         // �U���͈�
    public float attackInterval = 1f;     // �U���Ԋu
    private float attackTimer = 0f;
    public float moveSpeed = 3f;          // �L�����N�^�[�̈ړ����x
    public int maxHP = 30;                // �L�����N�^�[�̍ő�HP
    private int currentHP;                // ���݂�HP
    private BreakableObject targetObject; // ���݂̃^�[�Q�b�g

    private void Start()
    {
        currentHP = maxHP; // HP��������
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;

        // �^�[�Q�b�g���ݒ肳��Ă��Ȃ��ꍇ�A�V�����^�[�Q�b�g��T��
        if (targetObject == null || targetObject.IsBroken)
        {
            FindClosestTarget();
        }

        // �^�[�Q�b�g�ɐڋ߂��čU��
        if (targetObject != null)
        {
            MoveTowardsTarget();

            // �U���͈͓��ɓ�������U��
            float distanceToTarget = Vector3.Distance(transform.position, targetObject.transform.position);
            if (distanceToTarget <= attackRange && attackTimer >= attackInterval)
            {
                Attack(targetObject);
                attackTimer = 0f;
            }
        }
    }

    private void FindClosestTarget()
    {
        // �^�O��"sozai"�̃I�u�W�F�N�g��T��
        GameObject[] candidates = GameObject.FindGameObjectsWithTag("sozai");
        BreakableObject closestObject = null;
        float closestDistance = float.MaxValue;

        foreach (var candidate in candidates)
        {
            BreakableObject breakable = candidate.GetComponent<BreakableObject>();
            if (breakable != null && !breakable.IsBroken)
            {
                float distance = Vector3.Distance(transform.position, candidate.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = breakable;
                }
            }
        }

        targetObject = closestObject; // �ł��߂��^�[�Q�b�g��ݒ�
    }

    private void MoveTowardsTarget()
    {
        if (targetObject != null)
        {
            // �^�[�Q�b�g�̕������v�Z
            Vector3 direction = (targetObject.transform.position - transform.position).normalized;

            // �L�����N�^�[���ړ�
            transform.position += direction * moveSpeed * Time.deltaTime;

            // �^�[�Q�b�g�Ɍ����ĉ�]
            transform.LookAt(new Vector3(targetObject.transform.position.x, transform.position.y, targetObject.transform.position.z));
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
}
