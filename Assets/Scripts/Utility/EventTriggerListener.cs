using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventTriggerListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action<PointerEventData> onEnter;
    public Action<PointerEventData> onExit;

    public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke(eventData);
    public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke(eventData);

    public static EventTriggerListener Get(GameObject obj)
    {
        EventTriggerListener listener = obj.GetComponent<EventTriggerListener>();
        if (listener == null)
        {
            listener = obj.AddComponent<EventTriggerListener>();
        }

        return listener;
    }
}