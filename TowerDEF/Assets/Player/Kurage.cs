using UnityEngine;
using System.Collections;

public class Kurage : MonoBehaviour, IDamageable
{
    public float floatSpeed = 2f; // 浮上速度
    public float floatDuration = 2f; // 浮上する時間
    public int damage = 20; // 与えるダメージ
    public int health = 15; // クラゲの体力

    private void OnTriggerEnter2D(Collider2D other)
    {
        // IDamageable インターフェースを持つオブジェクトにダメージを与える
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage); // 20ダメージを与える
            StartCoroutine(FloatAndDestroy()); // クラゲを浮上させて削除
        }
    }

    private IEnumerator FloatAndDestroy()
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = transform.position;

        // クラゲを浮上させる
        while (elapsedTime < floatDuration)
        {
            transform.position = new Vector3(initialPosition.x, initialPosition.y + (floatSpeed * Time.deltaTime), initialPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // クラゲを削除
        Destroy(gameObject);
    }

    // ダメージを受ける処理を追加
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log(gameObject.name + " はダメージを受けました。残り体力: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " は倒されました");
        Destroy(gameObject);
    }
}
