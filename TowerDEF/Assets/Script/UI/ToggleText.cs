using UnityEngine;
using UnityEngine.UI;

public class ToggleText : MonoBehaviour
{
    public GameObject textObject;  // �\����\����؂�ւ�����Text�I�u�W�F�N�g���w��

    public void ToggleVisibility()
    {
        if (textObject != null)
        {
            textObject.SetActive(!textObject.activeSelf);  // ���݂̏�Ԃ𔽓]
        }
    }
}
