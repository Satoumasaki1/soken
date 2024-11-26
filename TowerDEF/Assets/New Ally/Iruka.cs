using UnityEngine;
using System.Collections;

public class Iruka : MonoBehaviour, IDamageable
{
    public int attackDamage = 10;
    // Iruka�̗̑͂Ɖ񕜊֘A�̐ݒ�
    public int health = 80;
    public int maxHealth = 80;
    public float detectionRadius = 15f;
    public int healAmount = 5;
    public float healInterval = 5f;
    public float buffDuration = 10f;
    public float attackBuffMultiplier = 1.2f;
    private bool isBuffActive = false;

    [SerializeField]
    private GameManager gm;

    void Start()
    {
        gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager")?.GetComponent<GameManager>();

        if (gm == null)
        {
            Debug.LogError("GameManager�̎Q�Ƃ�������܂���BGameManager���������ݒ肳��Ă��邩�m�F���Ă��������B");
        }

        StartCoroutine(HealAllies());
        StartCoroutine(BuffAllies());
    }

    IEnumerator HealAllies()
    {
        while (true)
        {
            yield return new WaitForSeconds(healInterval);
            ApplyHealEffect();
        }
    }

    IEnumerator BuffAllies()
    {
        while (true)
        {
            yield return new WaitForSeconds(healInterval);
            ApplyAttackBuff();
        }
    }

    void ApplyHealEffect()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Ally"))
            {
                HanaminoKasago ally = collider.GetComponent<HanaminoKasago>();
                if (ally != null)
                {
                    ally.TryHeal();
                }
            }
        }
    }

    void ApplyAttackBuff()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider collider in colliders)
        {
            HanaminoKasago ally = collider.GetComponent<HanaminoKasago>();
            if (ally != null && !ally.isBuffActive)
            {
                StartCoroutine(ApplyAttackBuffCoroutine(ally));
            }
        }
    }

    IEnumerator ApplyAttackBuffCoroutine(HanaminoKasago ally)
    {
        int originalAttackDamage = ally.attackDamage;
        ally.attackDamage = Mathf.RoundToInt(ally.attackDamage * attackBuffMultiplier);
        ally.isBuffActive = true;
        Debug.Log($"{ally.name} �̍U���͂���������܂���: {ally.attackDamage}");
        while (Vector3.Distance(transform.position, ally.transform.position) <= detectionRadius)
        {
            yield return null;
        }
        ally.attackDamage = originalAttackDamage;
        ally.isBuffActive = false;
        Debug.Log($"{ally.name} �̍U���͋������I�����܂����B���̍U���͂ɖ߂�܂���: {ally.attackDamage}");
    }

    private void OnMouseDown()
    {
        TryHealWithFeed();
    }

    public void TryHealWithFeed()
    {
        if (gm?.SelectedFeedType.HasValue == true)
        {
            var selectedFeed = gm.SelectedFeedType.Value;

            if (selectedFeed == GameManager.ResourceType.OkiaMi ||
                selectedFeed == GameManager.ResourceType.Benthos ||
                selectedFeed == GameManager.ResourceType.Plankton)
            {
                if (gm.inventory[selectedFeed] > 0)
                {
                    if (health < maxHealth)
                    {
                        health = Mathf.Min(health + 5, maxHealth);
                        gm.inventory[selectedFeed]--;
                        gm.UpdateResourceUI();
                        Debug.Log($"{selectedFeed} �ő̗͂��񕜂��܂����B�c��݌�: {gm.inventory[selectedFeed]}");
                    }
                    else
                    {
                        Debug.Log("�̗͂͊��ɍő�ł��B");
                    }
                }
                else
                {
                    Debug.Log($"{selectedFeed} �̍݌ɂ��s�����Ă��܂��B");
                }
            }
            else
            {
                Debug.Log("���̉a�ł͉񕜂ł��܂���B");
            }
        }
        else
        {
            Debug.Log("�a���I������Ă��܂���B");
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"{name} ���_���[�W���󂯂܂���: {damageAmount}, �c��̗�: {health}");
        if (health <= 0)
        {
            Die();
        }
    }

    public void TryHeal()
    {
        if (health < maxHealth)
        {
            health = Mathf.Min(health + healAmount, maxHealth);
            Debug.Log($"{name} �̗̑͂� {healAmount} �񕜂��܂����B���݂̗̑�: {health}");
        }
    }

    private void Die()
    {
        Debug.Log($"{name} ���|��܂����B");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
