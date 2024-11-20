using System.Collections;
using UnityEngine;

public class Shark : MonoBehaviour, IDamageable
{
    [Header("Shark Stats")]
    public float speed = 5.0f; // �ړ����x�i�i�[�t�ς݁j
    public int attackPower = 50; // �U���́i�i�[�t�ς݁j
    public int health = 150; // �̗́i�����ɕύX�j

    [Header("Enrage Settings")]
    public float enragedSpeedMultiplier = 2.0f;
    public int enragedAttackMultiplier = 2;
    public float specialAttackPreparationTime = 5.0f;
    public float specialAttackCooldownTime = 15.0f;

    private Transform targetAlly;
    private Transform targetBase;
    private Transform currentTarget;
    private bool isStunned = false;
    private bool enraged = false;

    private void Start()
    {
        SetInitialTargets();
    }

    private void Update()
    {
        if (isStunned) return; // �X�^�����͓����Ȃ�

        UpdateTarget();

        if (health <= 0.15f * 150 && !enraged)
        {
            Enrage();
        }

        if (currentTarget != null)
        {
            MoveTowardsTarget(currentTarget);
        }
    }

    private void SetInitialTargets()
    {
        targetAlly = GameObject.FindGameObjectWithTag("Ally")?.transform;
        targetBase = GameObject.FindGameObjectWithTag("Base")?.transform;
        currentTarget = targetAlly != null ? targetAlly : targetBase;
    }

    private void UpdateTarget()
    {
        if (currentTarget == null || (!currentTarget.CompareTag("Ally") && !currentTarget.CompareTag("Base")))
        {
            SetInitialTargets();
        }
    }

    private void MoveTowardsTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void Enrage()
    {
        enraged = true;
        speed *= enragedSpeedMultiplier;
        attackPower *= enragedAttackMultiplier;
        Debug.Log($"{gameObject.name} �����{��ԂɂȂ�܂����I���x�ƍU���͂��㏸���܂����B");
        StartCoroutine(SpecialAttackCooldown());
    }

    private IEnumerator SpecialAttack()
    {
        Debug.Log($"{gameObject.name} ������U����������...");
        yield return new WaitForSeconds(specialAttackPreparationTime);

        if (currentTarget != null)
        {
            IDamageable damageable = currentTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                int specialAttackPower = attackPower * 3; // ����U���͒ʏ�U����3�{�̃_���[�W
                damageable.TakeDamage(specialAttackPower);
                Debug.Log($"{gameObject.name} �� {currentTarget.name} �ɓ���U�����s���܂����B�_���[�W: {specialAttackPower}");
            }
        }
    }

    private IEnumerator SpecialAttackCooldown()
    {
        yield return SpecialAttack();
        yield return new WaitForSeconds(specialAttackCooldownTime);
        Debug.Log($"{gameObject.name} ������U���̃N�[���_�E�����I�����܂����B");
        StartCoroutine(SpecialAttack());
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"{gameObject.name} ���U�����󂯂܂����B�c��̗�: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} �͓|����܂����B");
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Base") || other.CompareTag("Ally"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackPower);
                Debug.Log($"{gameObject.name} �� {other.name} �ɍU�����s���܂����B�_���[�W: {attackPower}");
            }
        }
    }
}
