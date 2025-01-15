using UnityEngine;
using System.Collections;

public class Iruka : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
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

    private bool seasonEffectApplied = false;

    public void OnApplicationQuit()�@//�ǉ�
    {
        SaveState();
    }

    public void Upgrade(int additionalHp, int additionalDamage, int additionaRadius)//�ǉ�
    {
        health += additionalHp;
        attackDamage += additionalDamage;
        healAmount += additionaRadius;
        Debug.Log(gameObject.name + " upgraded! HP: " + health + ", Damage: " + attackDamage + ", Damage: " + healAmount);
    }

    public void SaveState()//�ǉ�
    {
        PlayerPrefs.SetInt($"{gameObject.name}_HP", health);
        PlayerPrefs.SetInt($"{gameObject.name}_Damage", attackDamage);
        PlayerPrefs.SetInt($"{gameObject.name}_Amount", healAmount);
        Debug.Log($"{gameObject.name} state saved!");
    }

    public void LoadState()//�ǉ�
    {
        if (PlayerPrefs.HasKey($"{gameObject.name}_HP"))
        {
            health = PlayerPrefs.GetInt($"{gameObject.name}_HP");
        }

        if (PlayerPrefs.HasKey($"{gameObject.name}_Damage"))
        {
            attackDamage = PlayerPrefs.GetInt($"{gameObject.name}_Damage");
        }

        if (PlayerPrefs.HasKey($"{gameObject.name}_Amount"))
        {
            healAmount = PlayerPrefs.GetInt($"{gameObject.name}_Amount");
        }

        Debug.Log($"{gameObject.name} state loaded! HP: {health}, Damage: {attackDamage}, Amount: {healAmount}");
    }

    void Start()
    {
        LoadState();//�ǉ�

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

    // �V�[�Y���̌��ʂ�K�p���郁�\�b�h
    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                maxHealth += 10;
                health = Mathf.Min(health + 10, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.1f);
                Debug.Log("�t�̃o�t���K�p����܂���: �̗͂ƍU���͂������㏸");
                break;
            case GameManager.Season.Summer:
                maxHealth -= 5;
                health = Mathf.Min(health, maxHealth);
                Debug.Log("�Ẵf�o�t���K�p����܂���: �̗͂�����");
                break;
            case GameManager.Season.Autumn:
                maxHealth += 15;
                health = Mathf.Min(health + 15, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                Debug.Log("�H�̃o�t���K�p����܂���: �̗͂ƍU���͂��㏸");
                break;
            case GameManager.Season.Winter:
                maxHealth -= 10;
                health = Mathf.Min(health, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                Debug.Log("�~�̃f�o�t���K�p����܂���: �̗͂ƍU���͂�����");
                break;
        }

        seasonEffectApplied = true;
    }

    // �V�[�Y���̌��ʂ����Z�b�g���郁�\�b�h
    public void ResetSeasonEffect()
    {
        maxHealth = 80;
        health = Mathf.Min(health, maxHealth);
        attackDamage = 10;
        seasonEffectApplied = false;
        Debug.Log("�V�[�Y�����ʂ����Z�b�g����܂����B");
    }
}
