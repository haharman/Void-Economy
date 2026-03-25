using UnityEngine;

namespace Model
{
    [CreateAssetMenu(fileName = "DefaultSaveData", menuName = "DefaultSaveData")]
    public class DefaultSaveDataSO : ScriptableObject
    {
        public SaveData data;
    }
}