using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class QuestModel
    {
        public List<MainQuest> MainQuests;
        public List<TradeQuest> TradeQuests;
    }
    public class MainQuestManager
    {
    }

    /// <summary>
    /// メインクエストの生成を担当するクラス
    /// 将来的にはScriptableObjectのシナリオをもとに生成する
    /// </summary>
    public class MainQuestFactory
    {
        
    }

    
    /// <summary>
    /// 取引クエストの生成を担当するクラス
    /// </summary>
    public class TradeQuestFactory
    {
        
    }
    
    [System.Serializable]
    public class MainQuest
    {
        public string id;
        public string title;
        public string description;
    }
    
    [System.Serializable]
    public class TradeQuest
    {
        public string id;
        public string title;
        public string description;
        
        [Header("依頼主の情報")]
        public string requesterName;
        public Sprite requesterIcon;

        [Header("要求される品物")]
        public Item targetItem;
        public int requiredAmount;
        
        [Header("報酬")]
        public int rewardYen;
        public Item rewardItem;
        public int rewardItemAmount;
    }
}