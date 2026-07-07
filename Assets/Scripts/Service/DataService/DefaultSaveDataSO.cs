using UnityEngine;
using Core;

namespace Service
{
    [CreateAssetMenu(fileName = "DefaultSaveData", menuName = "NewDefaultSaveData")]
    public class DefaultSaveDataSo : ScriptableObject
    {
        public SaveData data;
    }
}