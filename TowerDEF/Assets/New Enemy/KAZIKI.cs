using UnityEngine;
using UnityEngine.AI;

public class KAZIKI : MonoBehaviour, IDamageable
{
    public string primaryTargetTag = "Ally"; // �D��^�[�Q�b�g�̃^�O��ݒ�
    public string fallbackTag = "Base"; // �Ō�ɑ_���^�[�Q�b�g�̃^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 60; // KAZIKI�̗̑�
    public int attackDamage = 40; // �ːi�U���̈З͂��グ��
    public float attackRange = 6f; // �U���͈�
    public float attackCooldown = 3f; // �U���N�[���_�E������
    public float moveSpeed = 8f; // �ʏ�̈ړ����x������

    private float lastAttackTime;
    private NavMeshAgent agent;
    public float dashChargeTime = 2.0f; // �ːi�U���̃`���[�W����
    public float dashSpeed = 20f; // �ːi�U���̑��x
    public float dashDistance = 15f; // �ːi�U���̋���

    private bool isCharging = false;
    private bool isDashing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed; // �ړ����x��ݒ�
        FindTarget();
    }

    void Update()
    {
        if (isCharging || isDashing)
        {
            return; // �`���[�W���܂��͓ːi���͓���𐧌䂷��
        }

        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(fallbackTag)))
        {
            FindTarget();
        }

        if (target != null)
        {
            // �^�[�Q�b�g�Ɍ������Ĉړ�����
            agent.SetDestination(target.position);

            // �U���͈͓��Ƀ^�[�Q�b�g������ꍇ�A�ːi�U������������
            if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                StartCoroutine(ChargeAndDashAttack());
                lastAttackTime = Time.time;
            }
        }
    }

    void FindTarget()
    {
        // �D��^�[�Q�b�g�iAlly�^�O�j��T��
        GameObject allyTarget = GameObject.FindGameObjectWithTag(primaryTargetTag);
        if (allyTarget != null)
        {
            target = allyTarget.transform;
            return;
        }

        // Ally��������Ȃ��ꍇ�ABase�^�O�̃^�[�Q�b�g��T��
        GameObject baseTarget = GameObject.FindGameObjectWithTag(fallbackTag);
        if (baseTarget != null)
        {
            target = baseTarget.transform;
        }
    }

    System.Collections.IEnumerator ChargeAndDashAttack()
    {
        isCharging = true;
        agent.isStopped = true; // �`���[�W���͈ړ����~����
        yield return new WaitForSeconds(dashChargeTime);

        isCharging = false;
        isDashing = true;
        agent.isStopped = false;

        Vector3 dashDirection = transform.forward; // �ːi�����͌��݂̑O��
        float dashStartTime = Time.time;

        while (Time.time < dashStartTime + (dashDistance / dashSpeed))
        {
            agent.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;

        // �U�������i�ђʍU���j
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 1.0f, dashDirection, dashDistance);
        foreach (RaycastHit hit in hits)
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null && (hit.collider.CompareTag(primaryTargetTag) || hit.collider.CompareTag(fallbackTag)))
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log("KAZIKI���ːi�U�����s���܂���: " + hit.collider.name);
            }
        }

        // �ːi��̔�����HP��10%����
        TakeDamage((int)(health * 0.1f));
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
        // KAZIKI���|�ꂽ�ۂ̏����i�Ⴆ�Δj��Ȃǁj
        Destroy(gameObject);
    }
}
