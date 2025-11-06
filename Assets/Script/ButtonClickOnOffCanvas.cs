using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickOnOffCanvas : MonoBehaviour
{
    private Button toggleButton;

    [SerializeField] private CanvasGroup panelCanvas; // 👈 요기 추가!
    public float fadeDuration = 0.5f;

    private bool isVisible = true;

    void Start()
    {
        toggleButton = GetComponent<Button>();

        // panelCanvas를 인스펙터에서 직접 지정하므로 Find 필요 없음
        if (panelCanvas == null)
        {
            Debug.LogError("패널(CanvasGroup)이 지정되지 않았습니다! 인스펙터에서 연결하세요.");
            return;
        }

        toggleButton.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        StopAllCoroutines();
        if (isVisible)
            StartCoroutine(FadeOut());
        else
            StartCoroutine(FadeIn());
        isVisible = !isVisible;
    }

    IEnumerator FadeOut()
    {
        float startAlpha = panelCanvas.alpha;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            panelCanvas.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            yield return null;
        }
        panelCanvas.alpha = 0f;
        panelCanvas.interactable = false;
        panelCanvas.blocksRaycasts = false;
    }

    IEnumerator FadeIn()
    {
        float startAlpha = panelCanvas.alpha;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            panelCanvas.alpha = Mathf.Lerp(startAlpha, 1f, t / fadeDuration);
            yield return null;
        }
        panelCanvas.alpha = 1f;
        panelCanvas.interactable = true;
        panelCanvas.blocksRaycasts = true;
    }
}
