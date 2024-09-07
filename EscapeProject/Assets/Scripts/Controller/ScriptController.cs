using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScriptController : MonoBehaviour
{
    #region variables
    #region file path
    const string scriptDataPathKor = "Assets/Resources/ScriptDataKor.xml";
    const string scriptDataPathEng = "Assets/Resources/ScriptDataEng.xml";
    const string itemDataPathKor = "ItemDataKor";
    const string itemDataPathEng = "ItemDataEng";
    private string _scriptDataPath;
    private string _itemDataPath;
    private Dictionary<char, string> _nameDic;
    #endregion
    public bool IsChatting = false;
    [SerializeField] private GameObject _chatWindow;
    [SerializeField] private Sprite[] _chatWindowSprites;
    private TextMeshProUGUI _chatWindowText, _characterNameText;
    private RectTransform _chatWindowTextRect;
    private Image _chatWindowImage, _faceImage;
    private DataParser _dataParser = new DataParser();
    private List<ScriptInfo> _currentScripts;
    private int index = 0;
    private ItemController _itemController;
    private ObjectController _objectController;
    private EventController _eventController;
    #endregion
    void Start() {
        _chatWindowImage = _chatWindow.GetComponent<Image>();
        _faceImage = _chatWindow.transform.GetChild(1).gameObject.GetComponent<Image>();
        _chatWindowText = _chatWindow.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        _characterNameText = _chatWindow.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
        _chatWindowTextRect = _chatWindow.transform.GetChild(0).gameObject.GetComponent<RectTransform>();
        _chatWindow.SetActive(false);
        _itemController = GetComponent<ItemController>();
        _objectController = new ObjectController();
        _eventController = GetComponent<EventController>();

        switch (GameSystem.Language) {
            case 1:
                _scriptDataPath = scriptDataPathEng;
                _nameDic = _nameDicEng;
                _itemDataPath = itemDataPathEng;
                break;
            default:
                _scriptDataPath = scriptDataPathKor;
                _nameDic = _nameDicKor;
                _itemDataPath = itemDataPathKor;
                break;
        }
    }
    void Update() {
        // for test
        if (Input.GetKeyDown(KeyCode.A)) _itemController.UpdateAddSlots(ObjectData.Instance.AddItem(_dataParser.ItemDataParser(_itemDataPath, 5)));
        if (Input.GetKeyDown(KeyCode.B)) _itemController.UpdateAddSlots(ObjectData.Instance.AddItem(_dataParser.ItemDataParser(_itemDataPath, 9)));
        if (Input.GetKeyDown(KeyCode.C)) _itemController.UpdateAddSlots(ObjectData.Instance.AddItem(_dataParser.ItemDataParser(_itemDataPath, 6)));
    }
    public void ScriptPrint(ref ObjectInfo objectInfo, int characterNum = -1) {
        _currentScripts = (characterNum != -1) ? _dataParser.ScriptDataParser(_scriptDataPath, objectInfo, characterNum) : 
        _dataParser.ScriptDataParser(_scriptDataPath, objectInfo);
        objectInfo.scriptSeen = true;
        SetChatWindow();
    }
    public void ReScriptPrint(ObjectInfo objectInfo = null) {
        _currentScripts = _dataParser.ScriptDataParser(_scriptDataPath, objectInfo);
        if (_currentScripts.Count == 0) return;
        ScriptInfo currentScript = _currentScripts[_currentScripts.Count - 1];
        _currentScripts.Clear();
        _currentScripts.Add(currentScript);
        SetChatWindow();
    }
    public int ChatMoveOn() {
        index++;
        if (_currentScripts.Count == index) {
            _currentScripts.Clear();
            _chatWindow.SetActive(false);
            return 0;
        }
        else if (_currentScripts.Count < index) {
            IsChatting = false;
            index = 0;
            return 1;
        }
        SetChatWindow();
        return 1;
    }
    private Vector2 _noFaceSize = new Vector2(900f, 94f), _faceSize = new Vector2(630f, 90f);
    private Vector2 _noFacePosition = new Vector2(-4f, 24f), _facePosition = new Vector2(220f, 0f);
    private Dictionary<char, string> _nameDicKor = new Dictionary<char, string>() {
        {'A', "주인공"}, {'M', "남자"}, {'W', "여자"}, {'S', "쇼타"}, {'U', "오지상"}
    };
    private Dictionary<char, string> _nameDicEng = new Dictionary<char, string>() {
        {'A', "Protago"}, {'M', "Man"}, {'W', "Woman"}, {'S', "Shota"}, {'U', "Uncle"}
    };
    public static bool Disappear = false;
    private void SetChatWindow() { // if character is not in party, call 'ChatMoveOn'
        if (!IsChatting) IsChatting = true;
        if (_currentScripts.Count == 0) return;
        _chatWindowText.text = _currentScripts[index].script;
        if (_currentScripts[index].character == "N") {
            _characterNameText.text = "";
            _chatWindowImage.sprite = _chatWindowSprites[0];
            _chatWindowTextRect.sizeDelta = _noFaceSize;
            _chatWindowTextRect.anchoredPosition = _noFacePosition;
            _faceImage.color = new Color(1f, 1f, 1f, 0f);
        }
        else {
            _characterNameText.text = _nameDic[_currentScripts[index].character[0]];
            _chatWindowImage.sprite = _chatWindowSprites[1];
            _chatWindowTextRect.sizeDelta = _faceSize;
            _chatWindowTextRect.anchoredPosition = _facePosition;
            _faceImage.sprite = Resources.Load<Sprite>("Sprites/Face/" + _currentScripts[index].character);
            _faceImage.color = new Color(1f, 1f, 1f, 1f);
        }
        if (_currentScripts[index].removeItem != -1) {
            int slotIndex = ObjectData.Instance.RemoveItem(_currentScripts[index].removeItem);
            if (slotIndex != -1) _itemController.UpdateRemoveSlots(slotIndex);
        }
        if (_currentScripts[index].addItem != -1) {
            ItemInfo itemInfo = _dataParser.ItemDataParser(_itemDataPath, _currentScripts[index].addItem);
            int slotIndex = ObjectData.Instance.AddItem(itemInfo);
            if (slotIndex != -1) _itemController.UpdateAddSlots(slotIndex);
        }
        if (_currentScripts[index].changeObject != "") {
            ObjectInfo objectInfo;
            string str = _currentScripts[index].changeObject;
            if (str.Substring(0, 1) == "0") { // Max Level with Disappear
                objectInfo = ObjectData.Instance.Objects[str.Substring(1, str.Length - 1)];
                _objectController.ObjectLevelUp(ref objectInfo, false, -2, true);
            }
            else {
                objectInfo = ObjectData.Instance.Objects[_currentScripts[index].changeObject];
                _objectController.ObjectLevelUp(ref objectInfo);
            }
        }
        if (_currentScripts[index].gameEvent != "") {
            string eventName = _currentScripts[index].gameEvent;
            switch (eventName.Substring(0, eventName.Length - 1)) {
                case "Move":
                    ObjectData.Instance.UnlockedMap = ObjectData.Instance.CurrentMap + 1;
                    ObjectData.Instance.UpdateCanMove();
                    _currentScripts.AddRange(_dataParser.ScriptDataParser(_scriptDataPath, 1, 
                    int.Parse(eventName[eventName.Length - 1].ToString())));
                    break;
                case "PopUp":
                    InteractionController.ReservedPopUp = int.Parse(eventName[eventName.Length - 1].ToString());
                    break;
                case "Diary":
                    if (!EventController.NoteOn) {
                        EventController.NoteOn = true;
                        _currentScripts.AddRange(_dataParser.ScriptDataParser(_scriptDataPath, 0));
                    }
                    _eventController.NoteAdd(int.Parse(eventName[eventName.Length - 1].ToString()));
                    break;
                default:
                    break;
            }
        }
        _chatWindow.SetActive(true);
    }
}
