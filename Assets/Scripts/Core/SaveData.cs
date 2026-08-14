using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{


    [Serializable]
    public class SaveData
    {
        public SaveDataInfo info;
        public GlobalStateSaveData global;
        public OrrerySaveData orrery;
        //public EconomySaveData economy;
        //public InventorySaveData inventory;
        public PlayerSaveData player;
        public List<YarnVariable> yarn;

        public SaveData()
        {
            info = new SaveDataInfo();
            global = new GlobalStateSaveData();
            orrery = new OrrerySaveData();
            //economy = new EconomySaveData();
            //inventory = new InventorySaveData();
            player = new PlayerSaveData();
            yarn = new List<YarnVariable>();
        }
        
        public SaveData DeepCopy()
        {
            string json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<SaveData>(json);
        }
    }

    [Serializable]
    public struct SaveDataInfo
    {
        public int id;
        public string name;
        public string savedAt;
        public bool autoSave;
        public long savedAtUnixMs;
        public string screenshotFileName;
        public string version;
        public string sceneName;
    }

    [Serializable]
    public struct PlayerSaveData
    {
        public CoordSystemId coordSystem;
        public Vector2 position; // 座標
        public Vector2 velocity; // 速度
        public Vector2 forward; // 向き
    }

    [Serializable]
    public struct OrrerySaveData
    {
        public double orreryCumulativeSeconds;
    }

    [Serializable]
    public struct EconomySaveData
    {
        
    }

    [Serializable]
    public struct EntitySaveData
    {
        
    }

    [Serializable]
    public struct GlobalStateSaveData
    {
        public SceneType sceneType;
    }

    [Serializable]
    public struct InventorySaveData
    {
        
    }

    [Serializable]
    public struct YarnVariable
    {
        public string key;
        public string value;
        public string type; // "Float", "String", "Bool" などの識別用
    }
    
    [Serializable]
    public class ItemInstanceSaveData
    {
        public string InstanceId;
        public string DefinitionId; // ItemDefinition.Idの文字列
        public int Count;
        // Memory系拡張データはpolymorphicにシリアライズが必要になるので注意
    }
}