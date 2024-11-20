using UnityEngine;
using UnityEngine.AI;

public class ONIDARUMA_OKOZE : MonoBehaviour, IDamageable
{
    public string primaryTargetTag = "koukaku"; // �D��^�[�Q�b�g�̃^�O��ݒ�
    public string secondaryTargetTag = "Ally"; // ���ɗD�悷��^�[�Q�b�g�̃^�O
    public string fallbackTag = "Base"; // �Ō�ɑ_���^�[�Q�b�g�̃^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 60; // ONIDARUMA_OKOZE�̗̑�
    public int attackDamage = 30; // �U����
    public float attackRange = 4f; // �U���͈�
    public float attackCooldown = 2f; // �U���N�[���_�E������
    public float moveSpeed = 2f; // �ړ����x���ƂĂ��x��
    public int thornDamage = 10; // ���̔����_���[�W
    public float poisonDamage = 2f; // �ł̃_���[�W
    public float poisonDuration = 5f; // �ł̎�������
    public float poisonInterval = 1f; // �ł̃_���[�W�Ԋu

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
        else
        {
            ThornCounterAttack();
        }
    }

    private void ThornCounterAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(primaryTargetTag) || collider.CompareTag(secondaryTargetTag))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(thornDamage);
                    Debug.Log("ONIDARUMA_OKOZE�����̔������s���܂���: " + collider.name);
                    StartCoroutine(ApplyPoison(damageable));
                }
            }
        }
    }

    private System.Collections.IEnumerator ApplyPoison(IDamageable damageable)
    {
        float elapsedTime = 0f;
        MonoBehaviour damageableObject = damageable as MonoBehaviour;

        while (elapsedTime < poisonDuration)
        {
            if (damageableObject == null || damageableObject.gameObject == null)
            {
                // �Ώۂ��j�󂳂�Ă�����R���[�`�����I������
                yield break;
            }

            damageable.TakeDamage((int)poisonDamage);
            elapsedTime += poisonInterval;
            yield return new WaitForSeconds(poisonInterval);
        }
    }

    private void Die()
    {
        // ONIDARUMA_OKOZE���|�ꂽ�ۂ̏����i�Ⴆ�Δj��Ȃǁj
        Destroy(gameObject);
    }
}
