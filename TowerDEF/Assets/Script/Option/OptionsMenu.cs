using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Toggle bgmToggle;
    public Toggle sfxToggle;
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    void Start()
    {
        // BGM��SFX�̃g�O����Ԃ����[�h
        bgmToggle.isOn = PlayerPrefs.GetInt("BGM", 1) == 1;  // �f�t�H���g�̓I��
        sfxToggle.isOn = PlayerPrefs.GetInt("SFX", 1) == 1;  // �f�t�H���g�̓I��

        // �T�E���h�ݒ��������
        bgmSource.mute = !bgmToggle.isOn;
        sfxSource.mute = !sfxToggle.isOn;

        // �g�O�����ύX���ꂽ���̏���
        bgmToggle.onValueChanged.AddListener(delegate { ToggleBGM(bgmToggle.isOn); });
        sfxToggle.onValueChanged.AddListener(delegate { ToggleSFX(sfxToggle.isOn); });
    }

    void ToggleBGM(bool isOn)
    {
        bgmSource.mute = !isOn;  // �g�O�����I�t�̏ꍇ�ABGM���~���[�g
        PlayerPrefs.SetInt("BGM", isOn ? 1 : 0);  // �ݒ��ۑ�
    }

    void ToggleSFX(bool isOn)
    {
        sfxSource.mute = !isOn;  // �g�O�����I�t�̏ꍇ�A���ʉ����~���[�g
        PlayerPrefs.SetInt("SFX", isOn ? 1 : 0);  // �ݒ��ۑ�
    }
}
