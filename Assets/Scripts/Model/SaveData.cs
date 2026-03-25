using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using Core;

namespace Model
{


    [Serializable]
    public class SaveData
    {
        public GlobalStateSaveData global;
        public PhysicsSaveData physics;
        public EconomySaveData economy;
        public InventorySaveData inventory;
        public PlayerSaveData player;
        public List<YarnVariable> yarn;

        public SaveData(GlobalStateSaveData gl, PhysicsSaveData ph,  EconomySaveData ec, InventorySaveData inv, PlayerSaveData pl, List<YarnVariable> ya)
        {
            global = gl;
            physics = ph;
            economy = ec;
            inventory = inv;
            player = pl;
            yarn = ya;
        }
        
        public SaveData DeepCopy()
        {
            string json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<SaveData>(json);
        }
    }

    [Serializable]
    public class PlayerSaveData
    {
        public float scale = 1f;
        public Vector2 position; // 座標
        public Vector2 velocity; // 速度
        public Vector2 acceleration; // 加速度
        public Vector2 forward; // 向き
        public Vector2 velocityForward; // 速度の向き
    }

    [Serializable]
    public class PhysicsSaveData
    {
        
    }

    [Serializable]
    public class EconomySaveData
    {
        
    }

    [Serializable]
    public class GlobalStateSaveData
    {
        public SceneType sceneType;
    }

    [Serializable]
    public class InventorySaveData
    {
        
    }

    [Serializable]
    public struct YarnVariable
    {
        public string key;
        public string value;
        public string type; // "Float", "String", "Bool" などの識別用
    }
}