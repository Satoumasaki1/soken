using System.Collections.Generic;
using UnityEngine;

public class FeedManager : MonoBehaviour
{
    // �a�̎�ނ�񋓌^�Œ�`
    public enum FeedType { Plankton, Benthos, OkiaMi }

    // ���̗̑̓f�[�^��ێ�����N���X
    [System.Serializable]
    public class FishData
    {
        public int currentHealth; // ���݂̗̑�
        public int maxHealth;     // �ő�̗�
    }

    // ���I�u�W�F�N�g�Ƃ��̗̑̓f�[�^���֘A�t���鎫��
    private Dictionary<GameObject, FishData> fishDataMap;

    // �a���Ƃ̉񕜗ʂ�ێ�����f�B�N�V���i��
    private Dictionary<FeedType, int> feedHealAmounts;

    // ����������
    private void Start()
    {
        // ���̃f�[�^���Ǘ����邽�߂̎�����������
        fishDataMap = new Dictionary<GameObject, FishData>();
        // �a���Ƃ̉񕜗ʂ�������
        InitializeFeedHealAmounts();
    }

    // �a���Ƃ̉񕜗ʂ�ݒ�
    private void InitializeFeedHealAmounts()
    {
        feedHealAmounts = new Dictionary<FeedType, int>
        {
            { FeedType.Plankton, 2 }, // �v�����N�g���ŉ񕜂���ʂ�2
            { FeedType.Benthos, 5 },  // �x���g�X�ŉ񕜂���ʂ�5
            { FeedType.OkiaMi, 10 }   // �I�L�A�~�ŉ񕜂���ʂ�10
        };
    }

    // ���I�u�W�F�N�g�ɑΉ�����FishData���擾���郁�\�b�h
    public FishData GetFishData(GameObject fish)
    {
        // fishDataMap����w�肵�����̃f�[�^���擾
        if (fishDataMap.TryGetValue(fish, out FishData fishData))
        {
            return fishData; // ���������ꍇ�A���̃f�[�^��Ԃ�
        }

        // ����������Ȃ������ꍇ�A�f�t�H���g��FishData��Ԃ�
        return null;
    }

    // ���ɉa�������ĉ񕜏������s�����\�b�h
    public void FeedFish(GameObject fishObject, FeedType feedType)
    {
        // ���̃f�[�^���擾�܂��͏�����
        if (!fishDataMap.TryGetValue(fishObject, out FishData fishData))
        {
            // ���̏����̗͂�ݒ�
            fishData = new FishData { currentHealth = 10, maxHealth = 10 }; // �����l�ݒ�
            fishDataMap[fishObject] = fishData; // �����ɒǉ�
        }

        // �a���Ƃ̉񕜗ʂ��擾
        if (feedHealAmounts.TryGetValue(feedType, out int healAmount))
        {
            // ���݂̗̑͂��ő�̗͖����̏ꍇ�ɂ̂݉񕜂����s
            if (fishData.currentHealth < fishData.maxHealth)
            {
                // �񕜗ʂ�K�p���đ̗͂��X�V
                fishData.currentHealth = Mathf.Min(fishData.currentHealth + healAmount, fishData.maxHealth);
                Debug.Log($"{fishObject.name}��{feedType}�������āA�̗͂�{healAmount}�񕜂��܂����B");
            }
            else
            {
                Debug.Log($"{fishObject.name}�̗̑͂͊��ɍő�ł��B");
            }
        }
    }

    // �����N���b�N���ꂽ���ɌĂ΂�郁�\�b�h
    public void OnFishClicked(GameObject fishObject)
    {
        // ���ɉa�������鏈�����Ăяo���i��: �v�����N�g����^����j
        FeedFish(fishObject, FeedType.Plankton);
    }
}
