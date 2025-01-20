using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class FeedCombineUIManager : MonoBehaviour
{
    [SerializeField] private GameObject popupMenu;           // �����|�b�v�A�b�v���j���[
    [SerializeField] private Button feedAButton;             // �R�}�Z�{�^��
    [SerializeField] private Button feedBButton;             // �~�b�N�X�y���b�g�{�^��
    [SerializeField] private Button feedCButton;             // �o�C�I�t�B�[�h�{�^��
    [SerializeField] private TextMeshProUGUI resultText;     // �������ʂ�\������UI
    [SerializeField] private TextMeshProUGUI descriptionText; // �a�̐�����\������UI
    [SerializeField] private Button closeButton;             // �|�b�v�A�b�v�����{�^��

    public GameManager gm;
    private GameManager.ResourceType selectedFeedType;       // �I�����ꂽ�����a�̃^�C�v

    private void Start()
    {
        // �{�^���ɃN���b�N�C�x���g��ݒ�
        feedAButton.onClick.AddListener(() => SelectFeedToCreate(GameManager.ResourceType.FeedA));
        feedBButton.onClick.AddListener(() => SelectFeedToCreate(GameManager.ResourceType.FeedB));
        feedCButton.onClick.AddListener(() => SelectFeedToCreate(GameManager.ResourceType.FeedC));
        closeButton.onClick.AddListener(HidePopup);

        gm = GameManager.Instance;

        // �{�^���Ƀz�o�[�C�x���g��ǉ��i�X�N���v�g���Ő�������ݒ�j
        AddHoverEvents(feedAButton, "�R�}�Z: �����p�̉a�ł��B�I�L�A�~�ƃv�����N�g�����g���č쐬���܂��B");
        AddHoverEvents(feedBButton, "�~�b�N�X�y���b�g: �o�����X�̎�ꂽ�a�B�I�L�A�~�ƃx���g�X���g���č쐬���܂��B");
        AddHoverEvents(feedCButton, "�o�C�I�t�B�[�h: ����ȉa�B�v�����N�g���ƃx���g�X���g���č쐬���܂��B");

        // �|�b�v�A�b�v���ŏ��͔�\���ɂ���
        //popupMenu.SetActive(false);
    }

    // �����a��I�����郁�\�b�h
    private void SelectFeedToCreate(GameManager.ResourceType feedType)
    {
        selectedFeedType = feedType;
        CreateFeed(selectedFeedType);
    }

    // �����a���쐬���郁�\�b�h
    private void CreateFeed(GameManager.ResourceType feedType)
    {
        var inventory = GameManager.Instance.inventory;

        switch (feedType)
        {
            case GameManager.ResourceType.FeedA:
                if (inventory[GameManager.ResourceType.OkiaMi] > 0 && inventory[GameManager.ResourceType.Plankton] > 0)
                {
                    inventory[GameManager.ResourceType.OkiaMi]--;
                    inventory[GameManager.ResourceType.Plankton]--;
                    inventory[feedType]++;
                    gm.UpdateResourceUI();
                    resultText.text = "��������: �R�}�Z���쐬����܂����I";
                }
                else
                {
                    resultText.text = "�������s: �݌ɂ�����܂���";
                }
                break;

            case GameManager.ResourceType.FeedB:
                if (inventory[GameManager.ResourceType.OkiaMi] > 0 && inventory[GameManager.ResourceType.Benthos] > 0)
                {
                    inventory[GameManager.ResourceType.OkiaMi]--;
                    inventory[GameManager.ResourceType.Benthos]--;
                    inventory[feedType]++;
                    gm.UpdateResourceUI();
                    resultText.text = "��������: �~�b�N�X�y���b�g���쐬����܂����I";
                }
                else
                {
                    resultText.text = "�������s: �݌ɂ�����܂���";
                }
                break;

            case GameManager.ResourceType.FeedC:
                if (inventory[GameManager.ResourceType.Plankton] > 0 && inventory[GameManager.ResourceType.Benthos] > 0)
                {
                    inventory[GameManager.ResourceType.Plankton]--;
                    inventory[GameManager.ResourceType.Benthos]--;
                    inventory[feedType]++;
                    gm.UpdateResourceUI();
                    resultText.text = "��������: �o�C�I�t�B�[�h���쐬����܂����I";
                }
                else
                {
                    resultText.text = "�������s: �݌ɂ�����܂���";
                }
                break;

            default:
                resultText.text = "�����ȉa�^�C�v";
                break;
        }
    }

    // �|�b�v�A�b�v��\�����郁�\�b�h
    public void ShowPopup()
    {
        popupMenu.SetActive(true);
        resultText.text = "��肽���a��I��ł�������";
    }

    // �|�b�v�A�b�v����郁�\�b�h
    public void HidePopup()
    {
        popupMenu.SetActive(false);
        descriptionText.text = ""; // �������N���A
    }

    // �{�^���Ƀz�o�[�C�x���g��ǉ����郁�\�b�h
    private void AddHoverEvents(Button button, string description)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // �z�o�[���ɐ�����\��
        EventTrigger.Entry entryEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entryEnter.callback.AddListener((data) => { descriptionText.text = description; });
        trigger.triggers.Add(entryEnter);

        // �z�o�[���O�ꂽ�Ƃ��ɐ���������
        EventTrigger.Entry entryExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        entryExit.callback.AddListener((data) => { descriptionText.text = ""; });
        trigger.triggers.Add(entryExit);
    }
}
