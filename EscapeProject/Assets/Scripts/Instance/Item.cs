using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Item : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemInfo itemInfo;
    private Vector2 _firstPosition;
    private RectTransform _itemRect;
    void Start() {
        _firstPosition = gameObject.GetComponent<RectTransform>().anchoredPosition;
        _itemRect = gameObject.GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }
    void Update() {
        if (!InteractionController.IsDragging && _itemRect.anchoredPosition != _firstPosition) {
            _itemRect.anchoredPosition = _firstPosition;
        }
    }
    public void OnBeginDrag(PointerEventData pointerEventData) {
        ItemController.HoverItemInfo = null;
        InteractionController.DragBegin = true;
    }
    public void OnDrag(PointerEventData pointerEventData) {
        gameObject.transform.position = Input.mousePosition;
        ItemController.HoverItemInfo = null;
        InteractionController.IsDragging = true;
    }
    public void OnEndDrag(PointerEventData pointerEventData) {
        InteractionController.DragItemInfo = itemInfo;
        InteractionController.DragEnd = true;
        InteractionController.IsDragging = false;
        _itemRect.anchoredPosition = _firstPosition;
    }
    public void OnPointerEnter(PointerEventData pointerEventData) {
        if (InteractionController.DragBegin) return;
        ItemController.HoverItemInfo = itemInfo;
        ItemController.HoverItemPosition = gameObject.GetComponent<RectTransform>().anchoredPosition + new Vector2(-180f, 90f);
    }
    public void OnPointerExit(PointerEventData pointerEventData) {
        ItemController.HoverItemInfo = null;
    }
}
