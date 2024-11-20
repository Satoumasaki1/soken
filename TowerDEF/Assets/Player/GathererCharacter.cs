using UnityEngine;

public class GathererCharacter : MonoBehaviour
{
    public float attackRange = 2f;         // 攻撃範囲
    public float attackInterval = 1f;      // 攻撃間隔
    private float attackTimer = 0f;
    public InventoryManager inventory;     // インベントリ管理スクリプト
    public int maxHP = 30;                 // キャラクターの最大HP
    private int currentHP;                 // 現在のHP

    private void Start()
    {
        currentHP = maxHP; // HPを初期化
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;

        // 近くの破壊可能オブジェクトを探して攻撃
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            BreakableObject target = hitCollider.GetComponent<BreakableObject>();
            if (target != null && attackTimer >= attackInterval)
            {
                Attack(target);
                attackTimer = 0f;
            }
        }
    }

    private void Attack(BreakableObject target)
    {
        target.TakeDamage();
        currentHP--;

        // HPが0以下の場合、キャラクターを削除
        if (currentHP <= 0)
        {
            Debug.Log("キャラクターは力尽きました");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ドロップしたアイテムを検出してインベントリに追加
        if (other.CompareTag("Material"))
        {
            inventory.AddItem(other.gameObject);
            Destroy(other.gameObject);
        }
    }
}
