using UnityEngine;

public class HanaminoKasago : MonoBehaviour, IDamageable
{
    // HanaminoKasago�̗̑͂ƍő�̗�
    public int health = 20;
    public int maxHealth = 20;

    // ��დŊ֘A�̐ݒ�
    public float detectionRadius = 15f; // ��დł͈̔�
    public float poisonDamage = 1.0f;   // �p���_���[�W
    public float effectInterval = 1.0f; // �p���_���[�W�̊Ԋu

    private float lastEffectTime;

    [SerializeField]
    private GameManager gm;

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
}
