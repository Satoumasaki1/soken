using UnityEngine;
using System.Collections;

public class HanaminoKasago : MonoBehaviour, IDamageable, ISeasonEffect
{
    // HanaminoKasago�̗̑͂ƍő�̗�
    public float buffMultiplier = 1.5f; // �U���o�t�̔{��
    public int health = 20;
    public int maxHealth = 20;

    // ��დŊ֘A�̐ݒ�
    public float detectionRadius = 15f; // ��დł͈̔�
    public float poisonDamage = 1.0f;   // �p���_���[�W
    public float effectInterval = 1.0f; // �p���_���[�W�̊Ԋu

    private float lastEffectTime;
    public bool isBuffActive = false; // �o�t���L�����ǂ����̃t���O
    private int originalAttackDamage = 10;
    public int attackDamage = 10; // �U����

    [SerializeField]
    private GameManager gm;

    private bool seasonEffectApplied = false;

    void Start()
    {
        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        if (gm == null)
        {
            Debug.LogError("GameManager�̎Q�Ƃ�������܂���BGameManager���������ݒ肳��Ă��邩�m�F���Ă��������B");
        }
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;
        ApplyParalyticPoisonEffect();
        ApplyIrukaBuff();
    }

    private void OnMouseDown()
    {
        TryHeal();
    }

    public void TryHeal()
    {
        if (!gm.SelectedFeedType.HasValue) { Debug.Log("�a���I������Ă��܂���B"); return; }

        var selectedFeed = gm.SelectedFeedType.Value;
        if (selectedFeed == GameManager.ResourceType.OkiaMi ||
            selectedFeed == GameManager.ResourceType.Benthos ||
            selectedFeed == GameManager.ResourceType.Plankton)
        {
            if (gm.inventory[selectedFeed] > 0 && health < maxHealth)
            {
                health = Mathf.Min(health + 2, maxHealth);
                gm.inventory[selectedFeed]--;
                gm.UpdateResourceUI();
                Debug.Log($"{selectedFeed} �ő̗͂��񕜂��܂����B�c��݌�: {gm.inventory[selectedFeed]}");
            }
            else
            {
                Debug.Log(health >= maxHealth ? "�̗͂͊��ɍő�ł��B" : $"{selectedFeed} �̍݌ɂ��s�����Ă��܂��B");
            }
        }
        else
        {
            Debug.Log("���̉a�ł͉񕜂ł��܂���B");
        }
    }

    public void ApplyParalyticPoisonEffect()
    {
        if (Time.time > lastEffectTime + effectInterval)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            Debug.Log("��დł̌��ʂ�K�p��...");
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    IDamageable enemy = collider.GetComponent<IDamageable>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage((int)poisonDamage);
                        Debug.Log($"{collider.name} �ɖ�დł̌p���_���[�W��^���܂����B");
                    }

                    // ��დŏ�����K�p
                    if (collider.TryGetComponent(out Ikasan ikasan))
                    {
                        ikasan.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} �ɖ�დł�K�p���܂����B");
                    }
                    else if (collider.TryGetComponent(out OOKAMIUO ookamiuo))
                    {
                        ookamiuo.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} �ɖ�დł�K�p���܂����B");
                    }
                    else if (collider.TryGetComponent(out SAME same))
                    {
                        same.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} �ɖ�დł�K�p���܂����B");
                    }
                    else if (collider.TryGetComponent(out UTUBO utubo))
                    {
                        utubo.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} �ɖ�დł�K�p���܂����B");
                    }
                    else if (collider.TryGetComponent(out AKAEI akaei))
                    {
                        akaei.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} �ɖ�დł�K�p���܂����B");
                    }
                    else if (collider.TryGetComponent(out ISEEBI iseebi))
                    {
                        iseebi.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} �ɖ�დł�K�p���܂����B");
                    }
                    else if (collider.TryGetComponent(out KAZIKI kaziki))
                    {
                        kaziki.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} �ɖ�დł�K�p���܂����B");
                    }
                    else if (collider.TryGetComponent(out ONIDARUMA_OKOZE onidaruma_okoze))
                    {
                        onidaruma_okoze.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} �ɖ�დł�K�p���܂����B");
                    }
                    else if (collider.TryGetComponent(out ONIKAMASU onikamasu))
                    {
                        onikamasu.ApplyPoison(effectInterval, poisonDamage);
                        Debug.Log($"{collider.name} �ɖ�დł�K�p���܂����B");
                    }
                }
            }
            lastEffectTime = Time.time;
        }
    }

    // �C���J�̃o�t���L�����m�F���ēK�p���鏈��
    private void ApplyIrukaBuff()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool irukaNearby = false;
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out Iruka iruka))
            {
                irukaNearby = true;
                if (!isBuffActive)
                {
                    isBuffActive = true;
                    attackDamage = Mathf.RoundToInt(originalAttackDamage * buffMultiplier);
                    Debug.Log($"{name} �̍U���͂���������܂���: {attackDamage}");
                }
                break;
            }
        }

        if (!irukaNearby && isBuffActive)
        {
            isBuffActive = false;
            attackDamage = originalAttackDamage;
            Debug.Log($"{name} �̍U���͋������I�����܂����B���̍U���͂ɖ߂�܂���: {attackDamage}");
        }
    }

    // �_���[�W���󂯂��Ƃ��̏���
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    // HanaminoKasago���|�ꂽ�Ƃ��̏���
    private void Die()
    {
        Destroy(gameObject); // �I�u�W�F�N�g��j��
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
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
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 1.1f);
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
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 1.2f);
                Debug.Log("�H�̃o�t���K�p����܂���: �̗͂ƍU���͂��㏸");
                break;
            case GameManager.Season.Winter:
                maxHealth -= 10;
                health = Mathf.Min(health, maxHealth);
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 0.9f);
                Debug.Log("�~�̃f�o�t���K�p����܂���: �̗͂ƍU���͂�����");
                break;
        }

        seasonEffectApplied = true;
    }

    // �V�[�Y���̌��ʂ����Z�b�g���郁�\�b�h
    public void ResetSeasonEffect()
    {
        maxHealth = 20;
        health = Mathf.Min(health, maxHealth);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
        Debug.Log("�V�[�Y�����ʂ����Z�b�g����܂����B");
    }
}
