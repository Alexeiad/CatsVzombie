using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

public class SimpleUIAnimator : MonoBehaviour
{
    [SerializeField] private List<EventTrigger> _triggers;
    [SerializeField] private float clickScale = 0.9f;
    [SerializeField] private float animationDuration = 0.2f;

    private Dictionary<GameObject, Vector3> _originalScales = new Dictionary<GameObject, Vector3>();

    private void Awake()
    {
        // Устанавливаем частоту обновления для всех твинов
        DOTween.SetTweensCapacity(500, 50); // Опционально: увеличиваем capacity
        DOTween.defaultUpdateType = UpdateType.Normal;

        // Основная настройка FPS
        DOTween.timeScale = 1f; // Убеждаемся, что timeScale = 1
        Application.targetFrameRate = 120;

        foreach (var trigger in _triggers)
        {
            // Сохраняем оригинальный scale для каждого объекта
            if (trigger.gameObject != null && !_originalScales.ContainsKey(trigger.gameObject))
            {
                _originalScales[trigger.gameObject] = trigger.gameObject.transform.localScale;
            }

            AddEventTriggerListener(trigger, EventTriggerType.PointerClick, OnClick);
        }
    }

    private void OnClick(BaseEventData data)
    {
        // Получаем объект, на котором произошел клик
        GameObject clickedObject = ((PointerEventData)data).pointerCurrentRaycast.gameObject;
        if (clickedObject != null && _originalScales.ContainsKey(clickedObject))
        {
            // Останавливаем все предыдущие анимации
            clickedObject.transform.DOKill();

            // Сохраняем оригинальный scale если его еще нет
            if (!_originalScales.ContainsKey(clickedObject))
            {
                _originalScales[clickedObject] = clickedObject.transform.localScale;
            }

            Vector3 originalScale = _originalScales[clickedObject];

            // Анимация клика: уменьшить -> вернуться к оригиналу
            Sequence clickSequence = DOTween.Sequence();

            clickSequence.Append(clickedObject.transform.DOScale(originalScale * clickScale, animationDuration * 0.3f)
                .SetEase(Ease.OutCubic));

            clickSequence.Append(clickedObject.transform.DOScale(originalScale, animationDuration * 0.7f)
                .SetEase(Ease.OutElastic));
        }
    }

    private void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    // Метод для ручного добавления объекта (на случай динамического создания)
    public void AddTrigger(EventTrigger trigger)
    {
        if (trigger != null && !_triggers.Contains(trigger))
        {
            _triggers.Add(trigger);
            _originalScales[trigger.gameObject] = trigger.gameObject.transform.localScale;
            AddEventTriggerListener(trigger, EventTriggerType.PointerClick, OnClick);
        }
    }

    private void OnDestroy()
    {
        // Восстанавливаем оригинальные scale при уничтожении
        foreach (var kvp in _originalScales)
        {
            if (kvp.Key != null)
            {
                kvp.Key.transform.DOKill();
                kvp.Key.transform.localScale = kvp.Value;
            }
        }
    }
}