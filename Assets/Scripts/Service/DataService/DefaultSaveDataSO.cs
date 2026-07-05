using UnityEngine;
using Core;

namespace Service
{
    [CreateAssetMenu(fileName = "DefaultSaveData", menuName = "DefaultSaveData")]
    public class DefaultSaveDataSo : ScriptableObject
    {
        public SaveData data;
    }
}