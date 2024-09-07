using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class DataParser
{
    const string name = "Name", room = "Room", maxLevel = "MaxLevel", number = "Number", condition = "Condition",
    characterLevel = "CharacterLevel";
    const string character = "character", script = "script", addItem = "addItem", removeItem = "removeItem",
    changeObject = "changeObject", gameEvent = "gameEvent";
    const string UIName = "UIName", UIDesc = "UIDesc";
    public Dictionary<string, ObjectInfo> ObjectDataParser(string filePath, int roomNum) {
        List<Dictionary<string, object>> rawData = CSVReader.Read(filePath);
        Dictionary<string, ObjectInfo> objects = new Dictionary<string, ObjectInfo>();
        for (int i = 0; i < rawData.Count; i++) {
            if (int.Parse(rawData[i][room].ToString()) < roomNum) continue;
            if (int.Parse(rawData[i][room].ToString()) > roomNum) break;
            ObjectInfo objectInfo = new ObjectInfo();
            objects[rawData[i][name].ToString()] = objectInfo;
            objectInfo.room = int.Parse(rawData[i][room].ToString());
            objectInfo.number = int.Parse(rawData[i][number].ToString());
            objectInfo.maxLevel = int.Parse(rawData[i][maxLevel].ToString());
            objectInfo.characterLevel = int.Parse(rawData[i][characterLevel].ToString());
            objectInfo.condition = rawData[i][condition].ToString().Split("_").ToList().ConvertAll(int.Parse);
        }
        return objects;
    }
    public ItemInfo ItemDataParser(string filePath, int num) {
        Dictionary<string, object> rawData = CSVReader.ReadSingleData(filePath, num);
        ItemInfo itemInfo = new ItemInfo();
        itemInfo.name = rawData[name].ToString();
        itemInfo.number = int.Parse(rawData[number].ToString());
        itemInfo.character = int.Parse(rawData["Character"].ToString());
        itemInfo.UIName = rawData[UIName].ToString();
        itemInfo.UIDesc = rawData[UIDesc].ToString();
        return itemInfo;
    }
    public List<ScriptInfo> ScriptDataParser(string filePath, ObjectInfo objectInfo = null, int characterNum = -1) {
        List<ScriptInfo> scripts = new List<ScriptInfo>();
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        XmlNode nodelist;
        if (objectInfo == null) {
            nodelist = doc.SelectSingleNode("root/Default");
        }
        else if (characterNum != -1) {
            nodelist = doc.SelectSingleNode("root/Char" + characterNum + "/Case" + 
            (objectInfo.currentLevel - objectInfo.characterLevel).ToString());
        }
        else {
            nodelist = doc.SelectSingleNode("root/Map" + objectInfo.room.ToString()
            + objectInfo.number.ToString() + "/Level" + objectInfo.currentLevel.ToString());
        }
        foreach (XmlElement node in nodelist.ChildNodes) {
            ScriptInfo scriptInfo = new ScriptInfo
            {
                character = node.GetAttribute(character),
                script = node.GetAttribute(script)
            };
            if (objectInfo != null) scriptInfo.room = objectInfo.room;
            if (node.GetAttribute(addItem) != "") scriptInfo.addItem = int.Parse(node.GetAttribute(addItem));
            if (node.GetAttribute(removeItem) != "") scriptInfo.removeItem = int.Parse(node.GetAttribute(removeItem));
            if (node.GetAttribute(changeObject) != "") scriptInfo.changeObject = node.GetAttribute(changeObject);
            if (node.GetAttribute(gameEvent) != "") scriptInfo.gameEvent = node.GetAttribute(gameEvent);
            scripts.Add(scriptInfo);
        }
        return scripts;
    }
    public List<ScriptInfo> ScriptDataParser(string filePath, int eventNum, int detailedNum = -1) {
        List<ScriptInfo> scripts = new List<ScriptInfo>();
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        XmlNode nodelist;
        if (detailedNum != -1) nodelist = doc.SelectSingleNode("root/Event" + eventNum.ToString() + detailedNum.ToString());
        else nodelist = doc.SelectSingleNode("root/Event" + eventNum.ToString());
        foreach (XmlElement node in nodelist.ChildNodes) {
            ScriptInfo scriptInfo = new ScriptInfo {
                character = node.GetAttribute(character),
                script = node.GetAttribute(script)
            };
            scripts.Add(scriptInfo);
        }
        return scripts;
    }
    public string[] NoteDataParser(string filePath, int num) {
        string[] noteData = new string[2];
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        XmlNode node = doc.SelectSingleNode("root/Note" + num.ToString());
        noteData[0] = node.SelectSingleNode("Title").InnerText;
        noteData[1] = node.SelectSingleNode("Content").InnerText;
        return noteData;
    }
}
