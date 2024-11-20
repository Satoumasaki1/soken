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
        // BGMとSFXのトグル状態をロード
        bgmToggle.isOn = PlayerPrefs.GetInt("BGM", 1) == 1;  // デフォルトはオン
        sfxToggle.isOn = PlayerPrefs.GetInt("SFX", 1) == 1;  // デフォルトはオン

        // サウンド設定を初期化
        bgmSource.mute = !bgmToggle.isOn;
        sfxSource.mute = !sfxToggle.isOn;

        // トグルが変更された時の処理
        bgmToggle.onValueChanged.AddListener(delegate { ToggleBGM(bgmToggle.isOn); });
        sfxToggle.onValueChanged.AddListener(delegate { ToggleSFX(sfxToggle.isOn); });
    }

    void ToggleBGM(bool isOn)
    {
        bgmSource.mute = !isOn;  // トグルがオフの場合、BGMをミュート
        PlayerPrefs.SetInt("BGM", isOn ? 1 : 0);  // 設定を保存
    }

    void ToggleSFX(bool isOn)
    {
        sfxSource.mute = !isOn;  // トグルがオフの場合、効果音をミュート
        PlayerPrefs.SetInt("SFX", isOn ? 1 : 0);  // 設定を保存
    }
}
