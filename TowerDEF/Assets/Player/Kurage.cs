using UnityEngine;
using System.Collections;

public class Kurage : MonoBehaviour, IDamageable
{
    public float floatSpeed = 2f; // ���㑬�x
    public float floatDuration = 2f; // ���シ�鎞��
    public int damage = 20; // �^����_���[�W
    public int health = 15; // �N���Q�̗̑�

    private void OnTriggerEnter2D(Collider2D other)
    {
        // IDamageable �C���^�[�t�F�[�X�����I�u�W�F�N�g�Ƀ_���[�W��^����
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage); // 20�_���[�W��^����
            StartCoroutine(FloatAndDestroy()); // �N���Q�𕂏コ���č폜
        }
    }

    private IEnumerator FloatAndDestroy()
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = transform.position;

        // �N���Q�𕂏コ����
        while (elapsedTime < floatDuration)
        {
            transform.position = new Vector3(initialPosition.x, initialPosition.y + (floatSpeed * Time.deltaTime), initialPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �N���Q���폜
        Destroy(gameObject);
    }

    // �_���[�W���󂯂鏈����ǉ�
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log(gameObject.name + " �̓_���[�W���󂯂܂����B�c��̗�: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " �͓|����܂���");
        Destroy(gameObject);
    }
}
