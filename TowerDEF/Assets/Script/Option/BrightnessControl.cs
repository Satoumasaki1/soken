using UnityEngine;
using UnityEngine.UI;

public class BrightnessControl : MonoBehaviour
{
    public Slider brightnessSlider;
    public Light sceneLight;

    void Start()
    {
        // 明るさの初期値をロード（デフォルトは現在のライトの強度）
        brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", sceneLight.intensity);

        // スライダーの値が変更された時に実行されるイベント
        brightnessSlider.onValueChanged.AddListener(delegate { AdjustBrightness(brightnessSlider.value); });
    }

    void AdjustBrightness(float value)
    {
        sceneLight.intensity = value;  // スライダーの値に応じてライトの明るさを変更
        PlayerPrefs.SetFloat("Brightness", value);  // 明るさの設定を保存
    }
}
