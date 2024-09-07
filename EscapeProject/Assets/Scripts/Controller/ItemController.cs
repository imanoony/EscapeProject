using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemController : MonoBehaviour
{
    const string button = "Button";
    public static ItemInfo HoverItemInfo;
    public static Vector2 HoverItemPosition;
    [SerializeField] private GameObject _itemSlot, _toolTip;
    private GameObject[] _itemSlots = new GameObject[9];
    private float _slotOpenPositionY = -180f, _slotClosePositionY = 90f, _transitionSpeed = 500f;
    private RectTransform _itemSlotRect, _toolTipRect;
    private TextMeshProUGUI _toolTipUIName, _toolTipUIDesc;
    private ItemInfo _currentItemInfo;
    [HideInInspector] public bool SlotOpen = false, IsTransitioning = false;
    void Start() {
        for (int i = 1; i < _itemSlot.transform.childCount - 1; i++) {
            _itemSlots[i - 1] = _itemSlot.transform.GetChild(i).gameObject;
        }
        _itemSlotRect = _itemSlot.GetComponent<RectTransform>();
        _toolTipRect = _toolTip.GetComponent<RectTransform>();
        _toolTipUIName = _toolTip.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        _toolTipUIDesc = _toolTip.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
    }
    void Update() {
        if ((_currentItemInfo == null || _currentItemInfo != HoverItemInfo) && HoverItemInfo != null) {
            _currentItemInfo = HoverItemInfo;
            ToolTipOpen(HoverItemPosition, HoverItemInfo);
        }
        if (_currentItemInfo != null && HoverItemInfo == null) {
            _currentItemInfo = HoverItemInfo;
            ToolTipClose();
        }
    }
    public IEnumerator SlotOpenTransition() {
        if (IsTransitioning) yield break;
        IsTransitioning = true;
        SlotOpen = true;
        Vector2 itemSlotPosition = _itemSlotRect.anchoredPosition;
        while (itemSlotPosition.y > _slotOpenPositionY) {
            itemSlotPosition.y -= Time.deltaTime * _transitionSpeed;
            _itemSlotRect.anchoredPosition = itemSlotPosition;
            yield return null;
        }
        itemSlotPosition.y = _slotOpenPositionY;
        _itemSlotRect.anchoredPosition = itemSlotPosition;
        IsTransitioning = false;
        yield break;
    }
    public IEnumerator SlotCloseTransition() {
        if (IsTransitioning) yield break;
        IsTransitioning = true;
        SlotOpen = false;
        Vector2 itemSlotPosition = _itemSlotRect.anchoredPosition;
        while (itemSlotPosition.y < _slotClosePositionY) {
            itemSlotPosition.y += Time.deltaTime * _transitionSpeed;
            _itemSlotRect.anchoredPosition = itemSlotPosition;
            yield return null;
        }
        itemSlotPosition.y = _slotClosePositionY;
        _itemSlotRect.anchoredPosition = itemSlotPosition;
        IsTransitioning = false;
        yield break;
    }
    public void UpdateAddSlots(int index) {
        Item item = _itemSlots[index].GetComponent<Item>();
        Image image = _itemSlots[index].GetComponent<Image>();
        item.itemInfo = ObjectData.Instance.Inventory[index];
        image.sprite = Resources.Load<Sprite>("Sprites/Item/Item" + item.itemInfo.name);
        image.color = Color.white;
        _itemSlots[index].SetActive(true);
    }
    public void UpdateRemoveSlots(int index) {
        _itemSlots[index].SetActive(false);
    }
    public void ToolTipOpen(Vector2 position, ItemInfo itemInfo) {
        if (InteractionController.IsDragging) return;
        _toolTipRect.anchoredPosition = position;
        _toolTipUIName.text = itemInfo.UIName;
        _toolTipUIDesc.text = itemInfo.UIDesc.Replace("<n", "\n");
        _toolTip.SetActive(true);
    }
    public void ToolTipClose() {
        _toolTip.SetActive(false);
    }
}
