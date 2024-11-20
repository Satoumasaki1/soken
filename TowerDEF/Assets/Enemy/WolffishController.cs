using System.Collections;
using UnityEngine;

public class WolffishController : MonoBehaviour, IDamageable
{
    public float speed = 1.0f; // ���x�͒x��
    public float attackRange = 1.0f;
    public int attackDamage = 25; // �ꌂ������
    public float attackCooldown = 1.5f;
    public float frenzyAttackCooldown = 3.0f; // �A�����݂��̃N�[���_�E���𒷂߂ɐݒ�
    public float frenzyDuration = 3.0f; // �A�����݂��̎�������

    public GameObject target;
    private float attackCooldownTimer;
    private bool isInFrenzy;

    public int hp = 50; // Wolffish�̗̑�

    private void Start()
    {
        // �^�[�Q�b�g���ŏ��ɐݒ�
        FindNearestTarget();
    }

    private void Update()
    {
        if (target != null)
        {
            MoveTowardsTarget();
            AttackTargetIfInRange();
        }
        else
        {
            // �^�[�Q�b�g���Ȃ��Ȃ�����V�����^�[�Q�b�g��T��
            FindNearestTarget();
        }
    }

    public GameObject GetTarget()
    {
        return target;
    }

    private void FindNearestTarget()
    {
        // koukaku�^�O����������D�悵�ă^�[�Q�b�g�ɂ���
        GameObject[] koukakuTargets = GameObject.FindGameObjectsWithTag("koukaku");
        float nearestDistance = Mathf.Infinity;
        GameObject nearestKoukaku = null;

        foreach (GameObject koukaku in koukakuTargets)
        {
            float distance = Vector3.Distance(transform.position, koukaku.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestKoukaku = koukaku;
            }
        }

        if (nearestKoukaku != null)
        {
            target = nearestKoukaku;
        }
        else
        {
            // koukaku�^�O�̐��������Ȃ��ꍇ�̓v���C���[�̋��_���^�[�Q�b�g�ɂ���
            target = GameObject.FindGameObjectWithTag("Base");
        }
    }

    private void MoveTowardsTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void AttackTargetIfInRange()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange && attackCooldownTimer <= 0f)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log(gameObject.name + " attacks " + target.name + " with " + attackDamage + " damage.");
            }

            if (Random.value < 0.3f) // ���m���ŘA�����݂��𔭓�
            {
                StartCoroutine(FrenzyAttack());
            }
            else
            {
                attackCooldownTimer = attackCooldown;
            }
        }

        // �N�[���_�E�������炷
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    private IEnumerator FrenzyAttack()
    {
        isInFrenzy = true;
        float frenzyTimer = frenzyDuration;

        while (frenzyTimer > 0)
        {
            if (target != null && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
            {
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage);
                    Debug.Log(gameObject.name + " frenzy attacks " + target.name + " with " + attackDamage + " damage.");
                }
            }
            frenzyTimer -= frenzyAttackCooldown;
            yield return new WaitForSeconds(frenzyAttackCooldown);
        }

        isInFrenzy = false;
        attackCooldownTimer = attackCooldown;
    }

    // �_���[�W���󂯂�֐�
    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Remaining HP: " + hp);

        if (hp <= 0)
        {
            Destroy(gameObject); // HP��0�ɂȂ�����j��
        }
    }
}


