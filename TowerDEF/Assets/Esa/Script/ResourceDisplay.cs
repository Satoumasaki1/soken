using TMPro;  // TextMeshPro���g�p���邽�߂̖��O���
using UnityEngine;

public class ResourceDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text okiaMiText;  // OkiaMi�̍݌ɕ\���p
    [SerializeField] private TMP_Text benthosText;  // Benthos�̍݌ɕ\���p
    [SerializeField] private TMP_Text planktonText;  // Plankton�̍݌ɕ\���p

    void Update()
    {
        UpdateResourceDisplay();  // ���\�[�X�̕\�����X�V
    }

    void UpdateResourceDisplay()
    {
        // GameManager���烊�\�[�X�̍݌ɂ��擾���ATextMeshPro�ɕ\��
        //okiaMiText.text = "OkiaMi: " + GameManager.Instance.resourceSlots[(int)GameManager.ResourceType.OkiaMi];
        //benthosText.text = "Benthos: " + GameManager.Instance.resourceSlots[(int)GameManager.ResourceType.Benthos];
        //planktonText.text = "Plankton: " + GameManager.Instance.resourceSlots[(int)GameManager.ResourceType.Plankton];
    }
}
