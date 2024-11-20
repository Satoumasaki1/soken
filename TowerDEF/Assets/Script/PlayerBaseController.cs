using UnityEngine;

public class PlayerBaseController : MonoBehaviour, IDamageable
{
    public int baseHealth = 100;
    public GameObject gameOverPanel;

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // �v���C���[���_���U�����ꂽ�ۂ̃_���[�W����
    public void TakeDamage(int damage)
    {
        baseHealth -= damage;
        Debug.Log("�v���C���[�̋��_���U������܂����B�c��̗�: " + baseHealth);

        if (baseHealth <= 0)
        {
            GameOver();
        }
    }

    // �Q�[���I�[�o�[���̏���
    private void GameOver()
    {
        Debug.Log("�Q�[���I�[�o�[�B�v���C���[�̋��_���j�󂳂�܂����B");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f; // �Q�[�����ꎞ��~
        // �K�v�ɉ����ăQ�[�������Z�b�g����A�܂��͕ʂ̃V�[���ɑJ�ڂ��鏈����ǉ��ł��܂��B
    }
}
