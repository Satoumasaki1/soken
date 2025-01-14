using UnityEngine;
using UnityEngine.UI;

public class ShopSystem : MonoBehaviour
{
    public GameObject shopPanel; // �V���b�v�S�̂̃p�l��
    public GameObject itemPrefab; // �A�C�e���p�̃v���n�u
    public Transform gridParent; // Grid Layout Group �̐e�I�u�W�F�N�g
    public int gridSize = 5; // �O���b�h�̃T�C�Y (5x5)
    public ItemData[] items; // �A�C�e���f�[�^�̔z��

    private Button[] itemButtons; // ���������A�C�e���{�^�����Ǘ�
    private Image[] itemImages; // �e�A�C�e���̌����ڂ��Ǘ�
    private bool[] isPurchasable; // �e�A�C�e�����w���\���ǂ������Ǘ�
    private bool[] isPurchased; // �e�A�C�e�����w���ς݂��ǂ������Ǘ�

    private int pearls = 50; // �����^�쐔


    void Start()
    {
        shopPanel.SetActive(false); // �V���b�v���\���ɂ���

        int totalItems = gridSize * gridSize;
        itemButtons = new Button[totalItems];
        itemImages = new Image[totalItems];
        isPurchasable = new bool[totalItems];
        isPurchased = new bool[totalItems];

        GenerateShopItems();
        LoadPurchaseState();
    }

    // �V���b�v�̕\��/��\����؂�ւ���
    public void ToggleShop()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
    }

    // �V���b�v�A�C�e���𐶐�
    void GenerateShopItems()
    {
        for (int i = 0; i < gridSize * gridSize; i++)
        {
            GameObject item = Instantiate(itemPrefab, gridParent);
            Button itemButton = item.GetComponent<Button>();
            Image itemImage = item.GetComponent<Image>();

            itemButtons[i] = itemButton;
            itemImages[i] = itemImage;

            int itemIndex = i; // ���[�J���ϐ��ɃC���f�b�N�X��ۑ�

            if (itemIndex == gridSize * gridSize / 2)
            {
                // �����̃A�C�e�����w���\�ɂ���
                isPurchasable[itemIndex] = true;
                UpdateItemAppearance(itemIndex);
                itemButton.onClick.AddListener(() => OnItemClicked(itemIndex));
            }
            else
            {
                // ������Ԃł͍w���s��
                isPurchasable[itemIndex] = false;
                UpdateItemAppearance(itemIndex);
            }

            // �A�C�e���̃A�C�R����ݒ�
            if (i < items.Length)
            {
                itemImage.sprite = items[i].icon; // �A�C�e���ɐݒ肳�ꂽ�A�C�R��
                itemButton.GetComponentInChildren<Text>().text = items[i].cost.ToString(); // �A�C�e�����i��\��
            }
        }
    }

    // �A�C�e�����N���b�N���ꂽ�Ƃ��̏���
    void OnItemClicked(int index)
    {
        if (!isPurchasable[index] || isPurchased[index]) return; // �w���s�܂��͍w���ς݂̃A�C�e���͖���

        if (index < items.Length && pearls >= items[index].cost)
        {
            pearls -= items[index].cost; // �^�������
            isPurchased[index] = true; // �w���ς݂ɂ���
            Debug.Log($"Item {index} purchased for {items[index].cost} pearls!");

            // �A�C�e���̌��ʂ𔭓�
            items[index].ApplyEffect();

            // �㉺���E�̃A�C�e�����w���\�ɂ���
            UnlockAdjacentItems(index);
            UpdateItemAppearance(index);
        }
        else
        {
            Debug.Log("Not enough pearls!");
        }
    }

    // �㉺���E�̃A�C�e�����w���\�ɂ���
    void UnlockAdjacentItems(int index)
    {
        int[] adjacentOffsets = { -gridSize, gridSize, -1, 1 }; // �㉺���E�̃I�t�Z�b�g

        foreach (int offset in adjacentOffsets)
        {
            int adjacentIndex = index + offset;

            // �O���b�h�͈̔͊O�𖳎�
            if (adjacentIndex < 0 || adjacentIndex >= gridSize * gridSize)
                continue;

            // ���[�ƉE�[���ׂ��Ȃ��悤�ɂ���
            if ((index % gridSize == 0 && offset == -1) || (index % gridSize == gridSize - 1 && offset == 1))
                continue;

            if (!isPurchasable[adjacentIndex])
            {
                isPurchasable[adjacentIndex] = true;
                UpdateItemAppearance(adjacentIndex);
                itemButtons[adjacentIndex].onClick.AddListener(() => OnItemClicked(adjacentIndex));
            }
        }
    }

    // �A�C�e���̌����ڂ��X�V
    void UpdateItemAppearance(int index)
    {
        if (isPurchased[index])
        {
            itemButtons[index].interactable = false; // �N���b�N�s��
            itemImages[index].color = Color.gray; // �w���ς݂��D�F�ŕ\��
        }
        else if (isPurchasable[index])
        {
            itemButtons[index].interactable = true; // �N���b�N�\
            itemImages[index].color = Color.white; // �w���\�𔒐F�ŕ\��
        }
        else
        {
            itemButtons[index].interactable = false; // �N���b�N�s��
            itemImages[index].color = Color.black; // �w���s�����F�ŕ\��
        }
    }

    private void OnApplicationQuit()
    {
        SavePurchaseState();
    }

    private void SavePurchaseState()
    {
        for (int i = 0; i < isPurchased.Length; i++)
        {
            PlayerPrefs.SetInt($"ItemPurchased_{i}", isPurchased[i] ? 1 : 0); // �w����Ԃ�ۑ�
            PlayerPrefs.SetInt($"ItemPurchasable_{i}", isPurchasable[i] ? 1 : 0); // �����Ԃ�ۑ�
        }

        PlayerPrefs.Save(); // �f�[�^��ۑ�
        Debug.Log("Purchase and unlock states saved!");
    }

    private void LoadPurchaseState()
    {
        for (int i = 0; i < isPurchased.Length; i++)
        {
            // �w����Ԃ𕜌�
            if (PlayerPrefs.HasKey($"ItemPurchased_{i}"))
            {
                isPurchased[i] = PlayerPrefs.GetInt($"ItemPurchased_{i}") == 1;
            }

            // �����Ԃ𕜌�
            if (PlayerPrefs.HasKey($"ItemPurchasable_{i}"))
            {
                isPurchasable[i] = PlayerPrefs.GetInt($"ItemPurchasable_{i}") == 1;
            }
        }

        // �O�����X�V
        for (int i = 0; i < isPurchased.Length; i++)
        {
            UpdateItemAppearance(i);
        }

        Debug.Log("Purchase and unlock states loaded!");
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("All saved data reset!");
    }
}

