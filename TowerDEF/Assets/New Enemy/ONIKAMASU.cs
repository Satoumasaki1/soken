using UnityEngine;
using UnityEngine.AI;

public class ONIKAMASU : MonoBehaviour, IDamageable
{
    public string primaryTargetTag = "koukaku"; // �D��^�[�Q�b�g�̃^�O��ݒ�
    public string secondaryTargetTag = "Ally"; // ���ɗD�悷��^�[�Q�b�g�̃^�O
    public string fallbackTag = "Base"; // �Ō�ɑ_���^�[�Q�b�g�̃^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 45; // ONIKAMASU�̗̑�
    public int attackDamage = 10; // �U����
    public float attackRange = 4f; // �U���͈�
    public float attackCooldown = 1.5f; // �U���N�[���_�E������
    public float moveSpeed = 6f; // �ړ����x������

    private float lastAttackTime;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed; // �ړ����x��ݒ�
        FindTarget();
    }

    void Update()
    {
        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(secondaryTargetTag) && !target.CompareTag(fallbackTag)))
        {
            FindTarget();
        }

        if (target != null)
        {
            // �^�[�Q�b�g�Ɍ������Ĉړ�����
            agent.SetDestination(target.position);

            // �U���͈͓��Ƀ^�[�Q�b�g������ꍇ�A�U������
            if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                AttackTarget();
                lastAttackTime = Time.time;
            }
        }
    }

    void FindTarget()
    {
        // �D��^�[�Q�b�g�ikoukaku�^�O�j��T��
        GameObject koukakuTarget = GameObject.FindGameObjectWithTag(primaryTargetTag);
        if (koukakuTarget != null)
        {
            target = koukakuTarget.transform;
            return;
        }

        // ���ɗD�悷��^�[�Q�b�g�iAlly�^�O�j��T��
        GameObject allyTarget = GameObject.FindGameObjectWithTag(secondaryTargetTag);
        if (allyTarget != null)
        {
            target = allyTarget.transform;
            return;
        }

        // ����ł�������Ȃ��ꍇ�ABase�^�O�̃^�[�Q�b�g��T��
        GameObject baseTarget = GameObject.FindGameObjectWithTag(fallbackTag);
        if (baseTarget != null)
        {
            target = baseTarget.transform;
        }
    }

    void AttackTarget()
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // ONIKAMASU���|�ꂽ�ۂ̏����i�Ⴆ�Δj��Ȃǁj
        Destroy(gameObject);
    }
}
