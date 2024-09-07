using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractionController : MonoBehaviour
{   
    #region Variables
    const string character = "character", disappear = "Disappear";
    public static bool DragBegin = false, DragEnd = false, IsDragging = false;
    public static ItemInfo DragItemInfo = null;
    public float MaxDistance = 10f;
    private Camera _cam;
    private RaycastHit2D _objectRaycast;
    private Vector3 _tmpMousePosition;
    private ObjectController _objectController;
    private ScriptController _scriptController;
    private ItemController _itemController;
    private EventController _eventController;
    private KeyCode[] _chattingKeyCode = {KeyCode.Space, KeyCode.Return, KeyCode.Mouse0};
    private bool _isFading = false;
    #endregion
    void Start() {
        _cam = GetComponent<Camera>();
        _objectController = new ObjectController();
        _scriptController = GetComponent<ScriptController>();
        _itemController = GetComponent<ItemController>();
        _eventController = GetComponent<EventController>();

        _scrimImage = _scrim.GetComponent<Image>();
        _blackImage = _black.GetComponent<Image>();

        _moveTooltipRect = _moveTooltip.GetComponent<RectTransform>();
        _moveTooltipText = _moveTooltip.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
    }
    public static int ReservedPopUp = -1;
    void Update() {
        if (DragBegin) {
            DragBegin = false;
            SlotTransition();
            return;
        }
        if (_scriptController.IsChatting) {
            foreach (KeyCode keyCode in _chattingKeyCode) {
                if (Input.GetKeyDown(keyCode)) {
                    if (_scriptController.ChatMoveOn() == 0) {
                        if (ReservedPopUp != -1) PopUpTransition(ReservedPopUp);
                    }
                }
            }
        } 
        if (!_scriptController.IsChatting && !_itemController.SlotOpen && !_eventController.NoteOpen
        && !_eventController.PopUpAppear && !_isFading) CheckObject();
    }
    #region Interaction
    [SerializeField] private Sprite _transparency;
    private void CheckObject() {
        if (_characterInteracting && DragEnd) {
            ObjectInfo newObjectInfo = ObjectData.Instance.Objects[ObjectData.Instance.Characters[_characterNum]];
            ItemInteraction(ref newObjectInfo, DragItemInfo, true);
            return;
        }
        _tmpMousePosition = _cam.ScreenToWorldPoint(Input.mousePosition);
        _objectRaycast = Physics2D.Raycast(_tmpMousePosition, transform.forward, MaxDistance,
        LayerMask.GetMask("Default"));
        if (!_objectRaycast) return;
        if ((!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonUp(0)) || _isPointerOverUI) return;
        ObjectInfo currentObjectInfo = ObjectData.Instance.Objects[_objectRaycast.transform.name];
        bool isCharacter = false;
        if (_objectRaycast.transform.tag == disappear) return;
        if (_objectRaycast.transform.tag == character) {
            Sprite sprite = _objectRaycast.transform.gameObject.GetComponent<SpriteRenderer>().sprite;
            if (sprite != _transparency) isCharacter = true;
        }
        if (Input.GetMouseButtonDown(0) && !_characterInteracting) {
            if (isCharacter) ClickInteraction(currentObjectInfo, true);
            else ClickInteraction(currentObjectInfo);
        }
        else if (Input.GetMouseButtonUp(0)) {
            if (DragEnd) {
                if (isCharacter && DragItemInfo.character == 1) {
                    ItemInteraction(ref currentObjectInfo, DragItemInfo, true);
                }
                else ItemInteraction(ref currentObjectInfo, DragItemInfo);
            }
        }
    }
    private void ClickInteraction(ObjectInfo objectInfo, bool isCharacter = false) {
        if (isCharacter) { 
            if (objectInfo.currentLevel == -1) _objectController.ObjectLevelUp(ref objectInfo, true);
            StartCoroutine(CharacterFadeIn());
            _scriptController.ScriptPrint(ref objectInfo);
            StartCoroutine(ScrimFadeIn());
            return;
        }
        if (objectInfo.currentLevel == objectInfo.maxLevel) {
            if (objectInfo.scriptSeen) _scriptController.ReScriptPrint(null);
            else _scriptController.ScriptPrint(ref objectInfo);
            return;
        }
        if (objectInfo.currentLevel == -1) {
            _objectController.ObjectLevelUp(ref objectInfo, true);
            _scriptController.ScriptPrint(ref objectInfo);
            return;
        }
        if (objectInfo.scriptSeen) {
            _scriptController.ReScriptPrint(objectInfo);
        }
    }
    private void ItemInteraction(ref ObjectInfo objectInfo, ItemInfo itemInfo, bool isCharacter = false) {
        if (objectInfo.condition.Count == 0) return;
        if (!isCharacter && (itemInfo.number == objectInfo.condition[0] || 
        (objectInfo.currentLevel == -1 && itemInfo.number == objectInfo.condition[1]))) {
            _objectController.ObjectLevelUp(ref objectInfo);
            _scriptController.ScriptPrint(ref objectInfo);
        }
        if (isCharacter && objectInfo.condition.Contains(itemInfo.number)) {
            int beforeLevel = objectInfo.currentLevel;
            bool beforeSeen = objectInfo.scriptSeen;
            _objectController.ObjectLevelUp(ref objectInfo, false, objectInfo.condition.IndexOf(itemInfo.number) + beforeLevel);
            if (!_characterInteracting) {
                StartCoroutine(CharacterFadeIn());
                StartCoroutine(ScrimFadeIn());
            }
            _scriptController.ScriptPrint(ref objectInfo, _characterNum);
            _objectController.ObjectLevelUp(ref objectInfo, false, beforeLevel);
            objectInfo.scriptSeen = beforeSeen;
        }
        DragEnd = false;
        DragItemInfo = null;
    }
    private void PopUpInteraction() {
        
    }
    #endregion
    #region Character Interaction
    [SerializeField] private GameObject _character;
    private bool _characterInteracting = false;
    private int _characterNum = 0;
    private IEnumerator CharacterFadeIn() {
        StopCoroutine(CharacterFadeOut());
        _character.SetActive(true);
        _characterInteracting = true;
        Image characterImage = _character.GetComponent<Image>();
        Color characterColor = characterImage.color;
        while (characterColor.a < 1f) {
            characterColor.a += Time.deltaTime * _transitionSpeed * 2;
            characterImage.color = characterColor;
            yield return null;
        }
        characterColor.a = 1f;
        characterImage.color = characterColor;
        yield break;
    }
    private IEnumerator CharacterFadeOut() {
        StopCoroutine(CharacterFadeIn());
        Image characterImage = _character.GetComponent<Image>();
        Color characterColor = characterImage.color;
        while (characterColor.a > 0f) {
            characterColor.a -= Time.deltaTime * _transitionSpeed * 2;
            characterImage.color = characterColor;
            yield return null;
        }
        characterColor.a = 0f;
        characterImage.color = characterColor;
        _characterInteracting = false;
        _character.SetActive(false);
        yield break;
    }
    private void SetCharacter() {
        
    }
    #endregion
    #region Map Move
    [SerializeField] private GameObject _moveTooltip;
    private RectTransform _moveTooltipRect;
    private TextMeshProUGUI _moveTooltipText;
    public void MoveForward() {
        StartCoroutine(ScreenFadeOut(1));
    }
    public void MoveBackward() {
        StartCoroutine(ScreenFadeOut(-1));
    }
    public bool MapChange(int direction) {
        if (direction == 1 && ObjectData.Instance.CanMoveForward) {
            ObjectData.Instance.UpdateMap(false);
            ObjectData.Instance.CurrentMap++;
            ObjectData.Instance.UpdateMap(true);
            ObjectData.Instance.UpdateObjects();
            ObjectData.Instance.UpdateCanMove();
            return true;
        }
        else if (direction == -1 && ObjectData.Instance.CanMoveBackward) {
            ObjectData.Instance.UpdateMap(false);
            ObjectData.Instance.CurrentMap--;   
            ObjectData.Instance.UpdateMap(true);
            ObjectData.Instance.UpdateObjects();
            ObjectData.Instance.UpdateCanMove();
            return true;
        }
        return false;
    }
    private Vector2 _moveForwardPosition = new Vector2(-210f, -170f), _moveBackwardPosition = new Vector2(-210f, -275f);
    private const string moveMessage = "<color=#FFC807>맵이름</color>로 이동한다", mapName = "맵이름";
    private string[] _mapNames = new string[4] {"맵1", "맵2", "맵3", "맵4"};
    public void MoveForwardTooltipOn() {
        _moveTooltipRect.anchoredPosition = _moveForwardPosition;
        _moveTooltipText.text = moveMessage.Replace(mapName, _mapNames[ObjectData.Instance.CurrentMap]);
        _moveTooltip.SetActive(true);
    }
    public void MoveBackwardTooltipOn() {
        _moveTooltipRect.anchoredPosition = _moveBackwardPosition;
        _moveTooltipText.text = moveMessage.Replace(mapName, _mapNames[ObjectData.Instance.CurrentMap - 2]);
        _moveTooltip.SetActive(true);
    }
    public void MoveTooltipOff() { _moveTooltip.SetActive(false); }
    #endregion
    #region Utils
    [SerializeField] private GameObject _scrim;
    private Image _scrimImage;
    private float _transitionSpeed = 1f;
    private IEnumerator ScrimFadeIn() {
        StopCoroutine(ScrimFadeOut());
        _scrim.SetActive(true);
        Color scrimColor = _scrimImage.color;
        while (scrimColor.a < 0.5f) {
            scrimColor.a += Time.deltaTime * _transitionSpeed;
            _scrimImage.color = scrimColor;
            yield return null;
        }
        scrimColor.a = 0.5f;
        _scrimImage.color = scrimColor;
        yield break;
    }
    private IEnumerator ScrimFadeOut() {
        StopCoroutine(ScrimFadeIn());
        Color scrimColor = _scrimImage.color;
        while (scrimColor.a > 0f) {
            scrimColor.a -= Time.deltaTime * _transitionSpeed;
            _scrimImage.color = scrimColor;
            yield return null;
        }
        scrimColor.a = 0f;
        _scrimImage.color = scrimColor;
        _scrim.SetActive(false);
        yield break;
    }
    [SerializeField] private GameObject _black;
    private Image _blackImage;
    private IEnumerator ScreenFadeIn() {
        Color blackColor = _blackImage.color;
        while (blackColor.a > 0f) {
            blackColor.a -= Time.deltaTime * _transitionSpeed * 2;
            _blackImage.color = blackColor;
            yield return null;
        }
        blackColor.a = 0f;
        _blackImage.color = blackColor;
        _isFading = false;
        _black.SetActive(false);
        yield break;
    }
    private IEnumerator ScreenFadeOut(int direction) {
        Color blackColor = _blackImage.color;
        _isFading = true;
        _black.SetActive(true);
        while (blackColor.a < 1f) {
            blackColor.a += Time.deltaTime * _transitionSpeed * 2;
            _blackImage.color = blackColor;
            yield return null;
        }
        blackColor.a = 1f;
        _blackImage.color = blackColor;
        MapChange(direction);
        yield return new WaitForSecondsRealtime(0.2f);
        StartCoroutine(ScreenFadeIn());
        yield break;
    }
    public void ScrimClickUtil() {
        if (_scrimImage.color.a < 0.5f) return;
        if (_scriptController.IsChatting) return;
        if (_eventController.NoteOpen) {
            NoteTransition();
            return;
        }
        if (_characterInteracting) {
            StartCoroutine(CharacterFadeOut());
            StartCoroutine(ScrimFadeOut());
            if (_itemController.SlotOpen) SlotTransition();
            return;
        }
        if (_itemController.SlotOpen) SlotTransition();
        if (_eventController.PopUpAppear) PopUpTransition();
    }
    public void SlotTransition() {
        if (_scriptController.IsChatting || _itemController.IsTransitioning || _eventController.NoteOpen ||
        _eventController.PopUpAppear || _isFading) return;
        if (_itemController.SlotOpen) {
            _itemController.StartCoroutine(_itemController.SlotCloseTransition());
            if (!_characterInteracting) StartCoroutine(ScrimFadeOut());
        }
        else {
            _itemController.StartCoroutine(_itemController.SlotOpenTransition());
            StartCoroutine(ScrimFadeIn());
        }
    }
    public void NoteTransition() {
        if (_scriptController.IsChatting || _itemController.IsTransitioning || _itemController.SlotOpen ||
        _eventController.PopUpAppear || _isFading) return;
        if (_eventController.NoteOpen) {
            _eventController.StartCoroutine(_eventController.NoteCloseTransition());
            if (!_characterInteracting) StartCoroutine(ScrimFadeOut());
        }
        else {
            _eventController.StartCoroutine(_eventController.NoteOpenTransition());
            StartCoroutine(ScrimFadeIn());
        }
    }
    public void NoteRightTurn() {
        _eventController.PageMove(1);
    }
    public void NoteLeftTurn() {
        _eventController.PageMove(-1);
    }
    public void PopUpTransition(int index = -1) {
        if (_eventController.PopUpAppear && index == -1) {
            _eventController.PopUpClose();
            StartCoroutine(ScrimFadeOut());
            ReservedPopUp = -1;
        }
        else if (!_eventController.PopUpAppear && index != -1) {
            _eventController.PopUpOpen(index);
            StartCoroutine(ScrimFadeIn());
        }
    }
    private bool _isPointerOverUI = false;
    public void IsPointerOverUIOn() { _isPointerOverUI = true; }
    public void IsPointerOverUIOff() { _isPointerOverUI = false; }
    #endregion
}
