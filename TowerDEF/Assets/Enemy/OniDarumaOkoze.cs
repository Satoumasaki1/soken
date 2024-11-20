using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OniDarumaOkoze : MonoBehaviour, IDamageable
{
    public float speed = 0.5f; // 移動速度（遅い）
    public int attackPower = 50; // 攻撃力（非常に高い）
    public int health = 100; // 体力

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
        Debug.Log("Update メソッドが呼び出されました");
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
        Debug.Log(gameObject.name + " が攻撃を受けました。残り体力: " + health);

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
        Debug.Log(gameObject.name + " がカウンターを開始しました。");
        isCountering = true;
        yield return new WaitForSeconds(1.0f); // カウンターの待機時間

        // カウンター攻撃の処理（例: 近くのこうかくタグまたはAllyタグを持つオブジェクトにダメージを与える）
        bool counterSuccessful = false;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("koukaku") || hitCollider.CompareTag("Ally"))
            {
                Debug.Log("カウンター範囲内に検出: " + hitCollider.name + " (タグ: " + hitCollider.tag + ")");
                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackPower);
                    Debug.Log(gameObject.name + " が " + hitCollider.name + " にカウンター攻撃を行いました。");
                    currentTarget = hitCollider.transform; // 攻撃されたオブジェクトを優先ターゲットに設定
                    counterSuccessful = true;
                }
            }
        }

        if (!counterSuccessful)
        {
            Debug.Log(gameObject.name + " のカウンターは失敗しました。通常状態に戻ります。");
        }

        isCountering = false;
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " は倒されました。");
        Destroy(gameObject);
    }
}
