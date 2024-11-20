using UnityEngine;
using UnityEngine.UI;

public class BackGroundMover : MonoBehaviour
{
    private const float k_maxLength = 1f;
    private const string k_propName = "_MainTex"; // "_MainTex" �͒ʏ�̃e�N�X�`���I�t�Z�b�g�̃v���p�e�B���ł�

    [SerializeField]
    private Vector2 m_offsetSpeed = new Vector2(0.1f, 0.1f); // �e�N�X�`���̃I�t�Z�b�g�X�s�[�h

    private Material m_material;

    private void Start()
    {
        // �w�i��Image�R���|�[�l���g���擾���A�}�e���A�����Z�b�g
        Image imageComponent = GetComponent<Image>();
        if (imageComponent != null)
        {
            // �C���X�^���X�����ꂽ�}�e���A�����擾
            m_material = imageComponent.material;
        }
        else
        {
            Debug.LogWarning("Image�R���|�[�l���g��������܂���B");
        }
    }

    private void Update()
    {
        if (m_material != null)
        {
            // x��y�̒l��0 �` 1�Ń��s�[�g����悤�ɂ���
            float x = Mathf.Repeat(Time.time * m_offsetSpeed.x, k_maxLength);
            float y = Mathf.Repeat(Time.time * m_offsetSpeed.y, k_maxLength);
            Vector2 offset = new Vector2(x, y);

            // �e�N�X�`���I�t�Z�b�g��ݒ�
            m_material.SetTextureOffset(k_propName, offset);
        }
    }

    private void OnDestroy()
    {
        // �Q�[���I�����Ƀ}�e���A����Offset�����ɖ߂�
        if (m_material != null)
        {
            m_material.SetTextureOffset(k_propName, Vector2.zero);
        }
    }
}
