using UnityEngine;

public class SeasonObjectManager : MonoBehaviour
{
    // �Ή�����^�O��
    private readonly string[] seasonTags = { "Spring", "Summer", "Autumn", "Winter" };

    private GameManager.Season currentSeasonCache;

    private void Start()
    {
        // ������Ԃ�ݒ�
        UpdateSeasonObjects(GameManager.Instance.currentSeason);
    }

    private void Update()
    {
        // GameManager �̃C���X�^���X�����݂��邩�m�F
        if (GameManager.Instance != null)
        {
            GameManager.Season currentSeason = GameManager.Instance.currentSeason;

            // ���݂̋G�߂��ύX���ꂽ�ꍇ�̂ݏ������s��
            if (currentSeason != currentSeasonCache)
            {
                currentSeasonCache = currentSeason;
                UpdateSeasonObjects(currentSeason);
            }
        }
    }

    /// <summary>
    /// ���݂̋G�߂ɑΉ�����I�u�W�F�N�g��L�������A����ȊO�𖳌�������
    /// </summary>
    /// <param name="currentSeason">���݂̋G��</param>
    private void UpdateSeasonObjects(GameManager.Season currentSeason)
    {
        // �S�Ẵ^�O�ɂ��ď������s��
        foreach (string tag in seasonTags)
        {
            // ���݂̃V�[�Y���ƈ�v����^�O�͗L�����A��v���Ȃ��^�O�͖�����
            bool shouldActivate = tag == currentSeason.ToString();
            SetObjectsActiveByTag(tag, shouldActivate);
        }
    }

    /// <summary>
    /// �w�肳�ꂽ�^�O�����I�u�W�F�N�g��L�����܂��͖���������
    /// </summary>
    /// <param name="tag">����Ώۂ̃^�O</param>
    /// <param name="isActive">�L��������ꍇ�� true�A����������ꍇ�� false</param>
    private void SetObjectsActiveByTag(string tag, bool isActive)
    {
        // �w�肳�ꂽ�^�O�������ׂẴI�u�W�F�N�g���擾
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

        // �e�I�u�W�F�N�g�̏�Ԃ�ݒ�
        foreach (GameObject obj in objects)
        {
            obj.SetActive(isActive);
        }
    }
}
