using UnityEngine;

public class FishPanelController : MonoBehaviour
{
    // FishPanel��UI�v�f��RectTransform���Q�Ƃ���
    public RectTransform fishPanel;

    // ToggleButton�i�ׂ̃{�^���j��RectTransform���Q�Ƃ���
    public RectTransform toggleButton,toggleButton2;

    // �X���C�h�A�j���[�V�����̏��v���ԁi�b�P�ʁj
    public float slideDuration = 0.5f;

    // FishPanel���B���ۂ̈ʒu�i��ʊO��z��j
    public Vector2 hiddenPosition;

    // FishPanel���\�������ۂ̈ʒu�i��ʓ��j
    public Vector2 visiblePosition;

    // �{�^���̃I�t�Z�b�g�i�p�l���̉E�[����̋����j
    public Vector2 buttonOffset,buttonOffset2;

    // �p�l�������ݕ\������Ă��邩��ǐՂ���t���O
    private bool isPanelVisible = true;

    void Start()
    {
        // �����ʒu��ݒ�
        fishPanel.anchoredPosition = hiddenPosition;

        // �{�^���̏����ʒu��FishPanel�̉E�[�ɍ��킹�āA�I�t�Z�b�g���l��
        toggleButton.anchoredPosition = new Vector2(
            hiddenPosition.x + fishPanel.sizeDelta.x + buttonOffset.x,
            hiddenPosition.y + buttonOffset.y
            );

        toggleButton2.anchoredPosition = new Vector2(
            hiddenPosition.x + fishPanel.sizeDelta.x + buttonOffset2.x,
            hiddenPosition.y + buttonOffset2.y
        );
    }

    // FishPanel�ƃ{�^�����g�O������i�\��/��\����؂�ւ���j
    public void TogglePanel()
    {
        // ���̃A�j���[�V�������i�s���̏ꍇ�̓L�����Z�����ĐV���ɊJ�n����
        StopAllCoroutines();

        if (isPanelVisible)
        {
            // �p�l�����B���A�j���[�V�������J�n
            StartCoroutine(SlidePanel(visiblePosition));
        }
        else
        {
            // �p�l����\������A�j���[�V�������J�n
            StartCoroutine(SlidePanel(hiddenPosition));
        }
        // �\����Ԃ��g�O��
        isPanelVisible = !isPanelVisible;
    }

    // FishPanel��ToggleButton���X���C�h������A�j���[�V��������
    private System.Collections.IEnumerator SlidePanel(Vector2 targetPosition)
    {
        // FishPanel�̌��݂̈ʒu���L�^
        Vector2 startPanelPos = fishPanel.anchoredPosition;

        // ToggleButton�̌��݂̈ʒu���L�^
        Vector2 startButtonPos = toggleButton.anchoredPosition;

        // ToggleButton�̌��݂̈ʒu���L�^
        Vector2 startButtonPos2 = toggleButton2.anchoredPosition;

        // �{�^���̍ŏI�I�ȖڕW�ʒu���v�Z
        Vector2 targetButtonPos = new Vector2(
            targetPosition.x + fishPanel.sizeDelta.x + buttonOffset.x,
            targetPosition.y + buttonOffset.y
            );

        Vector2 targetButtonPos2 = new Vector2(
            targetPosition.x + fishPanel.sizeDelta.x + buttonOffset2.x,
            targetPosition.y + buttonOffset2.y
        );

        float elapsedTime = 0f; // �A�j���[�V�����o�ߎ��Ԃ̏�����

        while (elapsedTime < slideDuration) // �A�j���[�V�������I������܂Ń��[�v
        {
            // �o�ߎ��Ԃ��X�V
            elapsedTime += Time.deltaTime;

            // ���݂̐i�s�x�i0�`1�j���v�Z
            float t = Mathf.Clamp01(elapsedTime / slideDuration);

            // FishPanel���X���C�h
            fishPanel.anchoredPosition = Vector2.Lerp(startPanelPos, targetPosition, t);

            // ToggleButton���X���C�h
            toggleButton.anchoredPosition = Vector2.Lerp(startButtonPos, targetButtonPos, t);

            // ToggleButton���X���C�h
            toggleButton2.anchoredPosition = Vector2.Lerp(startButtonPos, targetButtonPos2, t);

            yield return null; // ���̃t���[���܂őҋ@
        }

        // �ŏI�I�Ȉʒu�𐳊m�ɐݒ�
        fishPanel.anchoredPosition = targetPosition;
        toggleButton.anchoredPosition = targetButtonPos;
        toggleButton2.anchoredPosition = targetButtonPos2;
    }
}
