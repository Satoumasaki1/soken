using UnityEngine;
using UnityEngine.AI;

public class UTUBO : MonoBehaviour, IDamageable
{
    public string primaryTargetTag = "koukaku"; // �D��^�[�Q�b�g�̃^�O��ݒ�
    public string secondaryTargetTag = "Ally"; // ���ɗD�悷��^�[�Q�b�g�̃^�O
    public string fallbackTag = "Base"; // �Ō�ɑ_���^�[�Q�b�g�̃^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 35; // UTUBO�̗̑�
    public int attackDamage = 10; // �U����
    public float attackRange = 4f; // �U���͈�
    public float attackCooldown = 3f; // �U���N�[���_�E������
    public float biteChargeTime = 2f; // �A�����݂��̃`���[�W����

    private float lastAttackTime;
    private NavMeshAgent agent;
    private bool isChargingBite = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
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

            // �U���͈͓��Ƀ^�[�Q�b�g������ꍇ�A�U���̏���������
            if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time > lastAttackTime + attackCooldown && !isChargingBite)
            {
                StartCoroutine(ChargeAndBite());
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

    System.Collections.IEnumerator ChargeAndBite()
    {
        isChargingBite = true;
        yield return new WaitForSeconds(biteChargeTime);
        AttackTarget();
        lastAttackTime = Time.time;
        isChargingBite = false;
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
        // UTUBO���|�ꂽ�ۂ̏����i�Ⴆ�Δj��Ȃǁj
        Destroy(gameObject);
    }
}
