using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectController
{
    private GameObject _object;
    public void ObjectLevelUp(ref ObjectInfo objectInfo, bool byClick = false, int level = -2, bool disappear = false) {
        if (level != -2) {
            objectInfo.currentLevel = level;
            ObjectSpriteChange(objectInfo, disappear);
            return;
        }
        if (objectInfo.currentLevel == objectInfo.maxLevel) return;
        if (!byClick && objectInfo.currentLevel == -1) {
            objectInfo.currentLevel += 2;
            objectInfo.condition.RemoveAt(0);
            objectInfo.condition.RemoveAt(0);
        }
        else {
            objectInfo.currentLevel++;
            objectInfo.condition.RemoveAt(0);
        }
        objectInfo.scriptSeen = false;
        ObjectSpriteChange(objectInfo, disappear);
    }
    private void ObjectSpriteChange(ObjectInfo objectInfo, bool disappear) {
        _object = ObjectData.Instance.Maps[objectInfo.room - 1];
        _object = _object.transform.GetChild(objectInfo.number).gameObject;
        string address = "Sprites/Object/Map" + objectInfo.room.ToString() + _object.name + objectInfo.currentLevel.ToString();
        Sprite sprite = Resources.Load<Sprite>(address);
        if (sprite == null) return;
        _object.GetComponent<SpriteRenderer>().sprite = sprite;
        if (disappear) {
            _object.tag = "Disappear";
            _object.GetComponent<EventTrigger>().enabled = false;
        }
    }
}
