using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.AI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // �Q�[���}�l�[�W���̃C���X�^���X
    public int baseHealth = 100; // ���_�̏����̗�
    public GameObject gameOverPanel; // �Q�[���I�[�o�[�p�l��
    public GameObject nextwavebutton; // ���̃E�F�[�u�̊J�n�{�^��

    public int totalWaves = 50; // ���E�F�[�u��
    public float waveDuration = 10f; // �E�F�[�u�̎�������
    public Text waveText; // �E�F�[�u���̃e�L�X�g
    public int currentWave = 0; // ���݂̃E�F�[�u��
    private float waveTimer; // �E�F�[�u�̃J�E���g�_�E���^�C�}�[

    public bool isPaused = false; // �Q�[�����ꎞ��~�����ǂ����������t���O

    public bool IsPaused // �Q�[���̈ꎞ��~��Ԃ��O������擾���邽�߂̃v���p�e�B
    {
        get { return isPaused; }
    }

    public delegate void OnWaveStart(); // �E�F�[�u�J�n���ɔ��΂���C�x���g
    public static event OnWaveStart WaveStarted; // �C�x���g

    public enum ResourceType { OkiaMi, Benthos, Plankton, FeedA, FeedB, FeedC, Uroko, Pearl } // ���\�[�X�̎��

    public enum ResourceFishType { Kani, Tyoutyou, Kaisou, Syako, Koban, Teppou, Manta, Uni } // ���\�[�X�̎��
    // �e���\�[�X�̍݌ɂ��Ǘ�����Dictionary
    public Dictionary<ResourceType, int> inventory = new Dictionary<ResourceType, int>();

    public Dictionary<ResourceFishType, int> finventory = new Dictionary<ResourceFishType, int>();
    public ResourceType? SelectedFeedType { get; set; } = ResourceType.OkiaMi; // �f�t�H���g��OkiaMi��ݒ�
    public ResourceFishType? SelectedFishType { get; set; } = null;

    // �e���\�[�X��UI�e�L�X�g
    public TextMeshProUGUI okiaMiText, benthosText, planktonText, feedAText, feedBText, feedCText, urokoText, pearlText, Kani, Tyoutyou, Kaisou, Syako, Koban, Teppou, Manta, Uni;

    public int currentDay = 0; // ���݂̓���
    public List<int> enemyDays = new List<int> { 4, 8, 12, 16, 20 }; // �G���U�߂Ă������̓�

    [SerializeField]
    public TextMeshProUGUI WaveText; // TextMeshPro�ŕ\������E�F�[�u���

    private void Awake()
    {
        // Singleton�p�^�[���i�C���X�^���X�����łɑ��݂��Ȃ��ꍇ�̂ݐV�����쐬�j
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // ���łɃC���X�^���X�����݂���ꍇ�͔j��
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject); // �V�[�����ς���Ă��j�����Ȃ�
        gameOverPanel.SetActive(false); // �Q�[���I�[�o�[�p�l�����\��
        nextwavebutton.SetActive(false); // ���̃E�F�[�u�{�^�����\��
        ForceResumeGame();  // ����E�F�[�u�J�n���̈ꎞ��~����������
        StartWave();  // �ŏ��̃E�F�[�u�J�n

        // �a�̏����݌ɂ�0�ɐݒ�
        foreach (ResourceType resource in System.Enum.GetValues(typeof(ResourceType)))
        {
            inventory[resource] = 0;
        }

        // ���̏����݌ɂ�0�ɐݒ�
        foreach (ResourceFishType resource in System.Enum.GetValues(typeof(ResourceFishType)))
        {
            finventory[resource] = 0;
        }

        // �I�L�A�~�A�x���g�X�A�v�����N�g���̍݌ɂ�3�ɐݒ�
        inventory[ResourceType.OkiaMi] = 3;
        inventory[ResourceType.Benthos] = 3;
        inventory[ResourceType.Plankton] = 3;

        //����m�F�̂��߂ɃJ�j�̍݌ɂ�3�ɐݒ�
        finventory[ResourceFishType.Kani] = 3;

        UpdateResourceUI(); // ���\�[�X�̏���UI���X�V
    }

    private void Update()
    {
        // �uE�v�L�[�ŃQ�[���̈ꎞ��~�E�ĊJ��؂�ւ�
        if (Input.GetKeyDown(KeyCode.E))
        {
            TogglePause();
        }

        // �Q�[�����ꎞ��~���łȂ��ꍇ�A�E�F�[�u�̃^�C�}�[���X�V
        if (!isPaused && currentWave < totalWaves)
        {
            waveTimer -= Time.deltaTime; // ���Ԃ̌o�߂Ń^�C�}�[�����炷
            if (waveTimer <= 0)
            {
                ShowNextWaveButton();  // �E�F�[�u�I����Ɏ��̃E�F�[�u�{�^����\��
            }
        }

        // ����̓��ɓG���U�߂Ă���
        if (enemyDays.Contains(currentDay))
        {
            Debug.Log("�G���U�߂Ă�����ł�: Day " + currentDay);
        }
    }

    // �ꎞ��~�ƍĊJ��؂�ւ��郁�\�b�h
    public void TogglePause()
    {
        isPaused = !isPaused; // ���݂̈ꎞ��~��Ԃ𔽓]
        ApplyPauseState(); // ��Ԃɉ�����������K�p
        Debug.Log("�Q�[���� " + (isPaused ? "�ꎞ��~��" : "�ĊJ��"));
    }

    // �ꎞ��~�������I�ɉ������郁�\�b�h
    private void ForceResumeGame()
    {
        isPaused = false; // �ꎞ��~��Ԃ�����
        ApplyPauseState(); // �ĊJ��ԂɕύX
    }

    // �ꎞ��~�̏�ԂɊ�Â��ăQ�[���I�u�W�F�N�g���ꎞ��~�E�ĊJ���鏈��
    private void ApplyPauseState()
    {
        // �G�̏���
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            // Enemy�X�N���v�g�𖳌���
            MonoBehaviour enemyScript = enemy.GetComponent<MonoBehaviour>();
            if (enemyScript != null)
            {
                enemyScript.enabled = !isPaused; // �ꎞ��~���͖�����
            }

            // NavMeshAgent�̈ړ����ꎞ��~�E�ĊJ
            NavMeshAgent enemyAgent = enemy.GetComponent<NavMeshAgent>();
            if (enemyAgent != null)
            {
                enemyAgent.isStopped = isPaused; // �ꎞ��~���͈ړ����~

                // �ꎞ��~���ɑ��x�����Z�b�g
                if (isPaused)
                {
                    enemyAgent.velocity = Vector3.zero;
                }
            }
        }

        // ���_�̏����i�G�Ɠ��l�Ɉړ����ꎞ��~�j
        GameObject[] bases = GameObject.FindGameObjectsWithTag("Base");
        foreach (GameObject baseObj in bases)
        {
            // Base�X�N���v�g�𖳌���
            MonoBehaviour baseScript = baseObj.GetComponent<MonoBehaviour>();
            if (baseScript != null)
            {
                baseScript.enabled = !isPaused;
            }

            // NavMeshAgent�̈ړ����ꎞ��~�E�ĊJ
            NavMeshAgent baseAgent = baseObj.GetComponent<NavMeshAgent>();
            if (baseAgent != null)
            {
                baseAgent.isStopped = isPaused; // �ꎞ��~���͈ړ����~

                // �ꎞ��~���ɑ��x�����Z�b�g
                if (isPaused)
                {
                    baseAgent.velocity = Vector3.zero;
                }
            }
        }

        // �����L�����N�^�[�̏����i�ړ��ƍU���𐧌�j
        GameObject[] allys = GameObject.FindGameObjectsWithTag("Ally");
        foreach (GameObject ally in allys)
        {
            // Kanisan�X�N���v�g�𖳌���
            /*Kanisan kanisanScript = ally.GetComponent<Kanisan>();
            if (kanisanScript != null)
            {
                kanisanScript.enabled = !isPaused; // �ꎞ��~���͖�����
            }*/

            // NavMeshAgent�̈ړ����ꎞ��~�E�ĊJ
            NavMeshAgent allyAgent = ally.GetComponent<NavMeshAgent>();
            if (allyAgent != null)
            {
                allyAgent.isStopped = isPaused; // �ꎞ��~���͈ړ����~

                // �ꎞ��~���ɑ��x�����Z�b�g
                if (isPaused)
                {
                    allyAgent.velocity = Vector3.zero;
                }
            }
        }



        Debug.Log("�Q�[���� " + (isPaused ? "�ꎞ��~��" : "�ĊJ��"));
    }

    // �V�����E�F�[�u���J�n���郁�\�b�h
    public void StartWave()
    {
        ForceResumeGame();  // �E�F�[�u���X�^�[�g������ꎞ��~����
        nextwavebutton.SetActive(false); // ���̃E�F�[�u�{�^�����\��
        currentWave++; // ���݂̃E�F�[�u���𑝉�
        waveTimer = waveDuration; // �E�F�[�u�̃^�C�}�[�����Z�b�g
        Debug.Log("�E�F�[�u " + currentWave + " ���J�n����܂���");

        WaveStarted?.Invoke(); // �E�F�[�u�J�n�C�x���g�𔭉�

        // ����̃E�F�[�u�ԍ��œG���U�߂Ă��邱�Ƃ�\��
        if (currentWave % 3 == 0 || currentWave % 4 == 0)
        {
            Debug.Log("�E�F�[�u " + currentWave + " �œG���U�߂Ă��܂�");
        }
    }

    // ���̃E�F�[�u�{�^����\�����郁�\�b�h
    public void ShowNextWaveButton()
    {
        isPaused = true; // �ꎞ��~��Ԃɂ���
        ApplyPauseState(); // �Q�[���̏�Ԃ��X�V
        nextwavebutton.SetActive(true); // ���̃E�F�[�u�{�^����\��
    }

    // ���\�[�X��ǉ����郁�\�b�h
    public void AddResource(GameManager.ResourceType resourceType, int amount)
    {
        if (inventory.ContainsKey(resourceType))
        {
            inventory[resourceType] += amount;
            Debug.Log($"{resourceType} �̍݌�: {inventory[resourceType]}");
            UpdateResourceUI(); // UI���X�V
        }
    }
    // FeedUIManager����Ăяo����A�I�����ꂽ�a�^�C�v��ݒ肷�郁�\�b�h
    public void SetSelectedFeedType(ResourceType feedType)
    {
        SelectedFeedType = feedType;
        Debug.Log("GameManager�őI�����ꂽ�a�^�C�v: " + SelectedFeedType);
    }

    //FishUIManager����Ăяo����A�I���������^�C�v��ݒ肷�郁�\�b�h
    public void SetSelectedFishType(ResourceFishType fishType)
    {
        SelectedFishType = fishType;
    }

    // ���\�[�XUI���X�V���郁�\�b�h
    public void UpdateResourceUI()
    {
        okiaMiText.text = "OkiaMi: " + inventory[ResourceType.OkiaMi];
        benthosText.text = "Benthos: " + inventory[ResourceType.Benthos];
        planktonText.text = "Plankton: " + inventory[ResourceType.Plankton];
        feedAText.text = "Feed A: " + inventory[ResourceType.FeedA];
        feedBText.text = "Feed B: " + inventory[ResourceType.FeedB];
        feedCText.text = "Feed C: " + inventory[ResourceType.FeedC];
        urokoText.text = "Uroko: " + inventory[ResourceType.Uroko];
        pearlText.text = "Pearl: " + inventory[ResourceType.Pearl];
        Kani.text ="�~" + finventory[ResourceFishType.Kani].ToString();
        Tyoutyou.text = "�~" + finventory[ResourceFishType.Tyoutyou].ToString();
        Kaisou.text = "�~" + finventory[ResourceFishType.Kaisou].ToString();
        Syako.text = "�~" + finventory[ResourceFishType.Syako].ToString();
        Koban.text = "�~" + finventory[ResourceFishType.Koban].ToString();
        Teppou.text = "�~" + finventory[ResourceFishType.Teppou].ToString();
        Manta.text = "�~" + finventory[ResourceFishType.Manta].ToString();
        Uni.text = "�~" + finventory[ResourceFishType.Uni].ToString();
    }

    // �Q�[���I�[�o�[����
    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        Debug.Log("�Q�[���I�[�o�[");
    }
}
