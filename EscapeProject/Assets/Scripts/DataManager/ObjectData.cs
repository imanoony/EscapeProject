using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectInfo {
    public int maxLevel, room, number, currentLevel = -1, characterLevel;
    public bool scriptSeen = false;
    public List<int> condition;
}
public class ItemInfo {
    public string name, UIName, UIDesc;
    public int room, number, character;
}
public class ScriptInfo {
    public string script, character, changeObject = "", gameEvent = "";
    public int addItem = -1, removeItem = -1, room;
} 
public class ObjectData : MonoBehaviour // Singleton
{
    const string objectData = "ObjectData";
    [HideInInspector] public int InventoryMaxCount = 9;

    #region Map
    const int MaxMap = 3;
    public int UnlockedMap = 1;
    public int CurrentMap = 1; // changed when the map level changes.
    public bool CanMoveForward = false, CanMoveBackward = false;
    [SerializeField] public GameObject[] Maps;
    [SerializeField] public GameObject[] MoveButtons = new GameObject[2];
    #endregion

    public static ObjectData Instance {get; private set;}
    public Dictionary<string, ObjectInfo> Objects = new Dictionary<string, ObjectInfo>(); // updated when the map level changes.
    public List<ItemInfo> Inventory;
    [HideInInspector] public List<string> Characters = new List<string>(4) { "Man", "Woman", "Shota", "Uncle" };
    //
    private DataParser _dataParser;
    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }
        Instance = this;
        Inventory = Enumerable.Repeat<ItemInfo>(null, InventoryMaxCount).ToList();
    }
    void Start() {
        _dataParser = new DataParser();
        UpdateObjects();
        UpdateCanMove();
    }
    public int AddItem(ItemInfo itemInfo) {
        for (int i = 0; i < InventoryMaxCount; i++) {
            if (Inventory[i] == null) {
                Inventory[i] = itemInfo;
                return i;
            }
        }
        return -1;
    }
    public int RemoveItem(int number) {
        for (int i = 0; i < Inventory.Count; i++) {
            if (Inventory[i] == null || Inventory[i].number != number) continue;
            Inventory[i] = null;
            return i;
        }
        return -1;
    }
    public void UpdateObjects() {
        Objects.Clear();
        Objects = _dataParser.ObjectDataParser(objectData, CurrentMap);
    }
    public void UpdateCanMove() {
        if (CurrentMap - 1 > 0) CanMoveBackward = true;
        else CanMoveBackward = false;
        if (CurrentMap + 1 <= UnlockedMap) CanMoveForward = true;
        else CanMoveForward = false;
        MoveButtons[0].SetActive(CanMoveForward);
        MoveButtons[1].SetActive(CanMoveBackward);
    }
    public void UpdateMap(bool setActive) {
        Maps[CurrentMap - 1].SetActive(setActive);
    }
}