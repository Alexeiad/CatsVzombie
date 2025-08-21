using UnityEngine;
using DG.Tweening;

public class UI_Animation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.9f;
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private Ease hoverEase = Ease.OutBack;
    [SerializeField] private Ease clickEase = Ease.OutCubic;
    [SerializeField] private Ease releaseEase = Ease.OutElastic;

    private Vector3 originalScale;
    private Tween currentTween;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    // Вызывать при наведении курсора
    public void OnHover()
    {
        KillCurrentTween();
        currentTween = transform.DOScale(originalScale * hoverScale, animationDuration)
            .SetEase(hoverEase)
            .SetUpdate(true);
    }

    // Вызывать при уходе курсора
    public void OnHoverExit()
    {
        KillCurrentTween();
        currentTween = transform.DOScale(originalScale, animationDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    // Вызывать при нажатии кнопки
    public void OnClick()
    {
        KillCurrentTween();

        // Последовательность: уменьшить -> увеличить с bounce эффектом
        Sequence clickSequence = DOTween.Sequence();

        clickSequence.Append(transform.DOScale(originalScale * clickScale, animationDuration * 0.3f)
            .SetEase(clickEase));

        clickSequence.Append(transform.DOScale(originalScale, animationDuration * 0.7f)
            .SetEase(releaseEase));

        currentTween = clickSequence;
    }

    // Вызывать при отпускании кнопки
    public void OnClickRelease()
    {
        KillCurrentTween();
        currentTween = transform.DOScale(originalScale, animationDuration)
            .SetEase(releaseEase)
            .SetUpdate(true);
    }

    // Принудительно сбросить анимацию
    public void ResetAnimation()
    {
        KillCurrentTween();
        transform.localScale = originalScale;
    }

    private void KillCurrentTween()
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }
    }

    private void OnDestroy()
    {
        KillCurrentTween();
    }
}