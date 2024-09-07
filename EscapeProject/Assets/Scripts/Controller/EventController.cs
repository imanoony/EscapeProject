using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum StoryEvent {
    Map1,
    Map2,
    Map3,
    Map4
}

public class EventController : MonoBehaviour // Move, Note, Pop-Up Event (etc.) // Event Catching
{
    private ObjectController _objectController;
    private ScriptController _scriptController;
    void Start() {
        _objectController = new ObjectController();
        _scriptController = GetComponent<ScriptController>();

        _note = _noteCanvas.transform.GetChild(1).gameObject;
        _noteRect = _note.GetComponent<RectTransform>();
        _noteTitleText = _note.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        _noteContentText = _note.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        _noteIconImage = _note.transform.GetChild(2).gameObject.GetComponent<Image>();

        switch (GameSystem.Language) {
            case 1:
                _noteDataPath = noteDataPathEng;
                break;
            default:
                _noteDataPath = noteDataPathKor;
                break;
        }
    }
    void Update() {
        if (NoteOn && !_noteCanvas.activeSelf) _noteCanvas.SetActive(true);
    }

    ///////////////////////////////////////////
    private int[] _storyEvents = {0, 0, 0, 0};
    public void ActivateStoryEvent(StoryEvent event) {
        if (_storyEvents[event] != 0) return;
        PrintStoryEvent(event);
        _storyEvents[event]++;
    }
    private void PrintStoryEvent(int eventNum) {

    }
    ///////////////////////////////////////////
    
    #region Pop-Up Event
    public bool PopUpAppear = false;
    [SerializeField] private GameObject[] PopUps;
    const string object0 = "Doorlock";
    const string password0 = "3210";
    private string _password0 = "";
    public void PopUp0Main() {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        string buttonNum = pointerEventData.selectedObject.name.Substring(6);
        if (_password0.Contains(buttonNum)) return;
        _password0 += buttonNum;
        if (_password0.Length != password0.Length) return;
        if (_password0 != password0) PopUp0Fail();
        else PopUp0Success();
        _password0 = "";
    }
    private void PopUp0Fail() {
        Debug.Log("Fail");
    }
    private void PopUp0Success() {
        Debug.Log("Success");
        ObjectInfo objectInfo = ObjectData.Instance.Objects[object0];
        _objectController.ObjectLevelUp(ref objectInfo);
        _scriptController.ScriptPrint(ref objectInfo);
    }
    public void PopUpOpen(int index) {
        if (index == 0) _password0 = "";
        PopUps[index].SetActive(true);
        PopUpAppear = true;
    }
    public void PopUpClose() {
        for (int i = 0; i < PopUps.Length; i++) {
            if (PopUps[i].activeSelf) {
                PopUps[i].SetActive(false);
                break;
            }
        }
        PopUpAppear = false;
    }
    #endregion
    #region Note Event
    public static bool NoteOn = false;
    const string noteDataPathKor = "Assets/Resources/NoteDataKor.xml";
    const string noteDataPathEng = "Assets/Resources/NoteDataEng.xml";
    private string _noteDataPath;
    public bool NoteOpen = false;
    private int _currentPage = 0;
    [SerializeField] private GameObject _noteCanvas;
    private GameObject _note;
    private RectTransform _noteRect;
    private TextMeshProUGUI _noteTitleText, _noteContentText;
    private Image _noteIconImage;
    private DataParser _dataParser = new DataParser();
    public void NoteAdd(int num) {
        for (int i = 0; i < _noteList.Length; i++) {
            if (_noteList[i] > num) {
                for (int j = _noteList.Length - 1; j > i; j--) {
                    _noteList[j] = _noteList[j - 1];
                }
                _noteList[i] = num;
                return;
            }
            if (_noteList[i] == -1) {
                _noteList[i] = num;
                return;
            }
        }
    }
    [SerializeField] private GameObject[] PageButtons = new GameObject[2];
    private int[] _noteList = new int[5] {-1, -1, -1, -1, -1};
    public void PageMove(int direction) {
        if (direction == 1 && PageButtons[0].activeSelf) {
            _currentPage++;
            StartCoroutine(NoteFadeOutTransition());
        }
        else if (direction == -1 && PageButtons[1].activeSelf) {
            _currentPage--;
            StartCoroutine(NoteFadeOutTransition());
        }
    }
    private void UpdatePage() {
        if (_currentPage + 1 < _noteList.Length) {
            if (_noteList[_currentPage + 1] != -1) {
                PageButtons[0].SetActive(true);
            }
            else PageButtons[0].SetActive(false);
        }
        else PageButtons[0].SetActive(false);
        if (_currentPage - 1 > -1) PageButtons[1].SetActive(true);
        else PageButtons[1].SetActive(false);
    }
    public void NoteSetting() {
        string[] noteTexts = _dataParser.NoteDataParser(_noteDataPath, _noteList[_currentPage]);
        _noteTitleText.text = noteTexts[0];
        _noteContentText.text = noteTexts[1];
        _noteIconImage.sprite = Resources.Load<Sprite>("Sprites/Item/ItemNote" + _noteList[_currentPage]);
    }
    #region Note Pop-Up Transition
    private float _noteOpenPositionY = 0f, _noteClosePositionY = -1080f;
    private float _transitionSpeed = 2300f, _fadingSpeed = 2f;
    public IEnumerator NoteOpenTransition() {
        _currentPage = 0;
        NoteSetting();
        UpdatePage();
        NoteOpen = true;
        Vector2 notePosition = _noteRect.anchoredPosition;
        while (notePosition.y < _noteOpenPositionY) {
            notePosition.y += Time.deltaTime * _transitionSpeed;
            _noteRect.anchoredPosition = notePosition;
            yield return null;
        }
        notePosition.y = _noteOpenPositionY;
        _noteRect.anchoredPosition = notePosition;
        yield break;
    }
    public IEnumerator NoteCloseTransition() {
        Vector2 notePosition = _noteRect.anchoredPosition;
        while (notePosition.y > _noteClosePositionY) {
            notePosition.y -= Time.deltaTime * _transitionSpeed;
            _noteRect.anchoredPosition = notePosition;
            yield return null;
        }
        notePosition.y = _noteClosePositionY;
        _noteRect.anchoredPosition = notePosition;
        NoteOpen = false;
        yield break;
    }
    public IEnumerator NoteFadeInTransition() {
        Color iconColor = _noteIconImage.color;
        Color titleColor = _noteTitleText.color;
        Color contentColor = _noteContentText.color;
        while (iconColor.a < 1f) {
            iconColor.a += Time.deltaTime * _fadingSpeed;
            titleColor.a += Time.deltaTime * _fadingSpeed;
            contentColor.a += Time.deltaTime * _fadingSpeed;
            _noteIconImage.color = iconColor;
            _noteTitleText.color = titleColor;
            _noteContentText.color = contentColor;
            yield return null;
        }
        iconColor.a = 1f;
        titleColor.a = 1f;
        contentColor.a = 1f;
        _noteIconImage.color = iconColor;
        _noteTitleText.color = titleColor;
        _noteContentText.color = contentColor;
        yield break;
    }
    public IEnumerator NoteFadeOutTransition() {
        Color iconColor = _noteIconImage.color;
        Color titleColor = _noteTitleText.color;
        Color contentColor = _noteContentText.color;
        while (iconColor.a > 0f) {
            iconColor.a -= Time.deltaTime * _fadingSpeed;
            titleColor.a -= Time.deltaTime * _fadingSpeed;
            contentColor.a -= Time.deltaTime * _fadingSpeed;
            _noteIconImage.color = iconColor;
            _noteTitleText.color = titleColor;
            _noteContentText.color = contentColor;
            yield return null;
        }
        iconColor.a = 0f;
        titleColor.a = 0f;
        contentColor.a = 0f;
        _noteIconImage.color = iconColor;
        _noteTitleText.color = titleColor;
        _noteContentText.color = contentColor;
        NoteSetting();
        UpdatePage();
        StartCoroutine(NoteFadeInTransition());
        yield break;
    }
    #endregion
    #endregion
    #region Story Event

    #endregion
}
