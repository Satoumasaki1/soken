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
    public GameObject CanvasPanel;
    public EsaSpawner es;

    public int totalWaves = 52; // ���E�F�[�u��
    public float waveDuration = 10f; // �E�F�[�u�̎�������
    public Text waveText; // �E�F�[�u���̃e�L�X�g
    public int currentWave = 0; // ���݂̃E�F�[�u��
    private float waveTimer; // �E�F�[�u�̃J�E���g�_�E���^�C�}�[

    public bool isPaused = false; // �Q�[�����ꎞ��~�����ǂ����������t���O

    // �{�^���̎Q�Ƃ�ݒ�
    public Button playButton;
    public Button pauseButton;

    public bool IsPaused // �Q�[���̈ꎞ��~��Ԃ��O������擾���邽�߂̃v���p�e�B
    {
        get { return isPaused; }
    }

    public delegate void OnWaveStart(); // �E�F�[�u�J�n���ɔ��΂���C�x���g
    public static event OnWaveStart WaveStarted; // �C�x���g

    public delegate void SeasonChangeHandler(GameManager.Season newSeason);
    public static event SeasonChangeHandler SeasonChanged;

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
    public List<int> enemyDays = new List<int> { 5, 9, 13, 18, 22, 26, 31, 35, 39, 44, 48, 52}; // �G���U�߂Ă������̓�

    [SerializeField]
    public TextMeshProUGUI WaveText; // TextMeshPro�ŕ\������E�F�[�u���

    // �����_���ȋ���Prefab���X�g
    [SerializeField] private List<GameObject> fishPrefabs;

    // �G�߂̗񋓌^
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    public Season currentSeason = Season.Spring; // �����͏t�̐ݒ�
    private Season? previousSeason = null; // �O�̃V�[�Y����ێ�����ϐ�

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
        CanvasPanel.SetActive(true);
        // �{�^���̃N���b�N�C�x���g�Ɋ֐���ݒ�
        playButton.onClick.AddListener(OnPlayButtonClick);
        pauseButton.onClick.AddListener(OnPauseButtonClick);

        // �L�̏����݌ɂ�0�ɐݒ�
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
        //finventory[ResourceFishType.Kani] = 3;
        //finventory[ResourceFishType.Tyoutyou] = 2;

        UpdateResourceUI(); // ���\�[�X�̏���UI���X�V
    }

    private void Update()
    {

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
        waveText.text = currentWave.ToString() + "�^�C�h";
    }

    // �ꎞ��~�ƍĊJ��؂�ւ��郁�\�b�h
    public void TogglePause()
    {
        isPaused = !isPaused; // ���݂̈ꎞ��~��Ԃ𔽓]
        ApplyPauseState(); // ��Ԃɉ�����������K�p
        Debug.Log("Game " + (isPaused ? "Paused" : "Resumed"));
    }
    public void OnPlayButtonClick()
    {
        if (isPaused)  // �ꎞ��~��ԂȂ����
        {
            TogglePause();
        }
    }

    // �ꎞ��~�{�^�����N���b�N���ꂽ�Ƃ�
    public void OnPauseButtonClick()
    {
        if (!isPaused)  // �Q�[�����Đ����Ȃ�ꎞ��~
        {
            TogglePause();
        }
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

        // ����L�����̏����i�ړ��ƍU���𐧌�j
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

        Debug.Log("Game " + (isPaused ? "Paused" : "Resumed"));
    }

    // �V�����E�F�[�u���J�n���郁�\�b�h
    public void StartWave()
    {
        ForceResumeGame();  // �E�F�[�u���X�^�[�g������ꎞ��~����
        nextwavebutton.SetActive(false); // ���̃E�F�[�u�{�^�����\��
        currentWave++; // ���݂̃E�F�[�u���𑝉�
        Debug.Log("Current Season: " + currentSeason); // ���݂̃V�[�Y�����f�o�b�O���O�ɕ\��
        waveTimer = waveDuration; // �E�F�[�u�̃^�C�}�[�����Z�b�g
        Debug.Log("Wave " + currentWave + " started");
        

        WaveStarted?.Invoke(); // �E�F�[�u�J�n�C�x���g�𔭉�

        // ����̃E�F�[�u�ԍ��œG���U�߂Ă��邱�Ƃ�\��
        /*if (currentWave % 3 == 0 || currentWave % 4 == 0)
        {
            Debug.Log("Wave " + currentWave + " has enemies attacking");
        }*/

        ConvertFeedToFish();

        // 13�E�F�[�u���ƂɋG�߂�ς���
        if (currentWave % 13 == 1)
        {
            ChangeSeason();
        }

        ResetSeasonalEffects(); // �G�߂̃o�t�ƃf�o�t�����Z�b�g
        ApplySeasonalEffects(); // �E�F�[�u�J�n���ɋG�߂̌��ʂ�K�p
        es.currentEsaCount = 0; //�a��̐��������Z�b�g
    }

    // �G�߂�ς��郁�\�b�h
    private void ChangeSeason()
    {
        if (currentSeason != previousSeason)
        {
            previousSeason = currentSeason;
            currentSeason = (Season)(((int)currentSeason + 1) % System.Enum.GetValues(typeof(Season)).Length);
            Debug.Log("Season changed to " + currentSeason);
            // �G�ߕύX�C�x���g�𔭉�
            SeasonChanged?.Invoke(currentSeason);
        }
    }

    // �G�߂̃o�t�ƃf�o�t�����Z�b�g���郁�\�b�h
    private void ResetSeasonalEffects()
    {
        GameObject[] allCreatures = GameObject.FindGameObjectsWithTag("Ally");
        foreach (GameObject creature in allCreatures)
        {
            ISeasonEffect seasonEffect = creature.GetComponent<ISeasonEffect>();
            if (seasonEffect != null)
            {
                seasonEffect.ResetSeasonEffect();
            }
        }
    }

    // �G�߂̌��ʂ�S�����ɓK�p���郁�\�b�h
    private void ApplySeasonalEffects()
    {
        GameObject[] allCreatures = GameObject.FindGameObjectsWithTag("Ally");
        foreach (GameObject creature in allCreatures)
        {
            ISeasonEffect seasonEffect = creature.GetComponent<ISeasonEffect>();
            if (seasonEffect != null)
            {
                seasonEffect.ApplySeasonEffect(currentSeason);
            }
        }
    }

    void ConvertFeedToFish()
    {
        // "esa" �^�O�������ׂẴI�u�W�F�N�g���擾
        GameObject[] feeds = GameObject.FindGameObjectsWithTag("esa");

        foreach (GameObject feed in feeds)
        {
            // �L�I�u�W�F�N�g�������_���ȋ��ɕϊ�
            int randomIndex = Random.Range(0, fishPrefabs.Count);
            GameObject fish = Instantiate(fishPrefabs[randomIndex], feed.transform.position, feed.transform.rotation);

            // �L�I�u�W�F�N�g���폜�i�K�v�ł���΁j
            Destroy(feed);
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
    // FeedUIManager����Ăяo����A�I�����ꂽ�L�^�C�v��ݒ肷�郁�\�b�h
    public void SetSelectedFeedType(ResourceType feedType)
    {
        SelectedFeedType = feedType;
        Debug.Log("Selected Feed Type: " + SelectedFeedType);
    }

    //FishUIManager����Ăяo����A�I���������^�C�v��ݒ肷�郁�\�b�h
    public void SetSelectedFishType(ResourceFishType fishType)
    {
        SelectedFishType = fishType;
    }

    // ���\�[�XUI���X�V���郁�\�b�h
    public void UpdateResourceUI()
    {
        okiaMiText.text = ": " + inventory[ResourceType.OkiaMi].ToString();
        benthosText.text = " :" + inventory[ResourceType.Benthos].ToString();
        planktonText.text = ": " + inventory[ResourceType.Plankton].ToString();
        feedAText.text = ": " + inventory[ResourceType.FeedA].ToString();
        feedBText.text = ": " + inventory[ResourceType.FeedB].ToString();
        feedCText.text = ": " + inventory[ResourceType.FeedC].ToString();
        urokoText.text = ": " + inventory[ResourceType.Uroko].ToString();
        pearlText.text = ": " + inventory[ResourceType.Pearl].ToString();
        Kani.text = "x" + finventory[ResourceFishType.Kani].ToString();
        Tyoutyou.text = "x" + finventory[ResourceFishType.Tyoutyou].ToString();
        Kaisou.text = "x" + finventory[ResourceFishType.Kaisou].ToString();
        Syako.text = "x" + finventory[ResourceFishType.Syako].ToString();
        Koban.text = "x" + finventory[ResourceFishType.Koban].ToString();
        Teppou.text = "x" + finventory[ResourceFishType.Teppou].ToString();
        Manta.text = "x" + finventory[ResourceFishType.Manta].ToString();
        Uni.text = "x" + finventory[ResourceFishType.Uni].ToString();
    }

    // �Q�[���I�[�o�[����
    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        Debug.Log("Game Over");
    }
}
