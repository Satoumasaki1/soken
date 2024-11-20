using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OniDarumaOkoze : MonoBehaviour, IDamageable
{
    public float speed = 0.5f; // �ړ����x�i�x���j
    public int attackPower = 50; // �U���́i���ɍ����j
    public int health = 100; // �̗�

    private Transform targetBase;
    private Transform currentTarget;
    private bool isCountering = false;

    private void Start()
    {
        targetBase = GameObject.FindGameObjectWithTag("Base").transform;
        currentTarget = targetBase;
    }

    private void Update()
    {
        Debug.Log("Update ���\�b�h���Ăяo����܂���");
        if (currentTarget != null && !isCountering)
        {
            MoveTowardsTarget(currentTarget);
        }
    }

    private void MoveTowardsTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log(gameObject.name + " ���U�����󂯂܂����B�c��̗�: " + health);

        if (health <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(CounterAttack());
        }
    }

    private IEnumerator CounterAttack()
    {
        Debug.Log(gameObject.name + " ���J�E���^�[���J�n���܂����B");
        isCountering = true;
        yield return new WaitForSeconds(1.0f); // �J�E���^�[�̑ҋ@����

        // �J�E���^�[�U���̏����i��: �߂��̂��������^�O�܂���Ally�^�O�����I�u�W�F�N�g�Ƀ_���[�W��^����j
        bool counterSuccessful = false;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("koukaku") || hitCollider.CompareTag("Ally"))
            {
                Debug.Log("�J�E���^�[�͈͓��Ɍ��o: " + hitCollider.name + " (�^�O: " + hitCollider.tag + ")");
                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackPower);
                    Debug.Log(gameObject.name + " �� " + hitCollider.name + " �ɃJ�E���^�[�U�����s���܂����B");
                    currentTarget = hitCollider.transform; // �U�����ꂽ�I�u�W�F�N�g��D��^�[�Q�b�g�ɐݒ�
                    counterSuccessful = true;
                }
            }
        }

        if (!counterSuccessful)
        {
            Debug.Log(gameObject.name + " �̃J�E���^�[�͎��s���܂����B�ʏ��Ԃɖ߂�܂��B");
        }

        isCountering = false;
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " �͓|����܂����B");
        Destroy(gameObject);
    }
}
