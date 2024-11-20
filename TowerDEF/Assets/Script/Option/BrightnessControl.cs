using UnityEngine;
using UnityEngine.UI;

public class BrightnessControl : MonoBehaviour
{
    public Slider brightnessSlider;
    public Light sceneLight;

    void Start()
    {
        // ���邳�̏����l�����[�h�i�f�t�H���g�͌��݂̃��C�g�̋��x�j
        brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", sceneLight.intensity);

        // �X���C�_�[�̒l���ύX���ꂽ���Ɏ��s�����C�x���g
        brightnessSlider.onValueChanged.AddListener(delegate { AdjustBrightness(brightnessSlider.value); });
    }

    void AdjustBrightness(float value)
    {
        sceneLight.intensity = value;  // �X���C�_�[�̒l�ɉ����ă��C�g�̖��邳��ύX
        PlayerPrefs.SetFloat("Brightness", value);  // ���邳�̐ݒ��ۑ�
    }
}
