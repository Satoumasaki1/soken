using UnityEngine;
using UnityEngine.UI;

public class ToggleText : MonoBehaviour
{
    public GameObject textObject;  // �\����\����؂�ւ�����Text�I�u�W�F�N�g���w��


    // �ꎞ��~�{�^�����N���b�N���ꂽ���̏���

    public void PButton()
    {
        if(textObject != null)
        {
            textObject.SetActive(true);// �e�L�X�g��\��
        }
    }

    // �Đ��{�^�����N���b�N���ꂽ���̏���
    public void PlayButton()
    {
        if (textObject != null)
        {
            textObject.SetActive(false);  // �e�L�X�g���\��
        }
    }
}
