using UnityEngine;

public class FishPanelController : MonoBehaviour
{
    // FishPanel��UI�v�f��RectTransform���Q�Ƃ���
    public RectTransform fishPanel;

    // ToggleButton�i�ׂ̃{�^���j��RectTransform���Q�Ƃ���
    public RectTransform toggleButton;

    // �X���C�h�A�j���[�V�����̏��v���ԁi�b�P�ʁj
    public float slideDuration = 0.5f;

    // FishPanel���B���ۂ̈ʒu�i��ʊO��z��j
    public Vector2 hiddenPosition;

    // FishPanel���\�������ۂ̈ʒu�i��ʓ��j
    public Vector2 visiblePosition;

    // �{�^���̃I�t�Z�b�g�i�p�l���̉E�[����̋����j
    public Vector2 buttonOffset;

    // �p�l�������ݕ\������Ă��邩��ǐՂ���t���O
    private bool isPanelVisible = true;

    void Start()
    {
        // �����ʒu��ݒ�
        fishPanel.anchoredPosition = visiblePosition;

        // �{�^���̏����ʒu��FishPanel�̉E�[�ɍ��킹�āA�I�t�Z�b�g���l��
        toggleButton.anchoredPosition = new Vector2(
            visiblePosition.x + fishPanel.sizeDelta.x + buttonOffset.x,
            visiblePosition.y + buttonOffset.y
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
            StartCoroutine(SlidePanel(hiddenPosition));
        }
        else
        {
            // �p�l����\������A�j���[�V�������J�n
            StartCoroutine(SlidePanel(visiblePosition));
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

        // �{�^���̍ŏI�I�ȖڕW�ʒu���v�Z
        Vector2 targetButtonPos = new Vector2(
            targetPosition.x + fishPanel.sizeDelta.x + buttonOffset.x,
            targetPosition.y + buttonOffset.y
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

            yield return null; // ���̃t���[���܂őҋ@
        }

        // �ŏI�I�Ȉʒu�𐳊m�ɐݒ�
        fishPanel.anchoredPosition = targetPosition;
        toggleButton.anchoredPosition = targetButtonPos;
    }
}
