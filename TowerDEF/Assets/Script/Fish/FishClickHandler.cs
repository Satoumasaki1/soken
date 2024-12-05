using UnityEngine;

public class FishClickHandler : MonoBehaviour
{
    [SerializeField] private GameManager.ResourceFishType fishType; // �C���X�y�N�^�[�Őݒ肷�鋛�̃^�C�v

    private void OnMouseDown()
    {
        AddFishToInventory(fishType);
    }

    private void AddFishToInventory(GameManager.ResourceFishType fishType)
    {
        // �݌ɂ𑝂₷����
        switch (fishType)
        {
            case GameManager.ResourceFishType.Kani:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Tyoutyou:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Kaisou:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Syako:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Koban:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Teppou:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Manta:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Uni:
                GameManager.Instance.finventory[fishType]++;
                break;
            default:
                Debug.LogWarning("�w�肳�ꂽ���^�C�v��������܂���B");
                break;
        }

        // �݌ɑ�����AUI���X�V���邽�߂�FishInventoryUI�ɒʒm
        FishInventoryUIManager fishInventoryUI = GameObject.Find("FishInventoryUIManager").GetComponent<FishInventoryUIManager>();
        if (fishInventoryUI != null)
        {
            fishInventoryUI.OnFishStockIncreased(fishType, 1); // ���������ʂ�n���i1�j; // �݌ɂ����������Ƃ�UI�ɓ`����
        }

        // �C���x���g���[���X�V������AUI�����t���b�V��
        GameManager.Instance.UpdateResourceUI();

        // �����N���b�N��ɍ폜����ꍇ
        Destroy(gameObject);
    }

}
