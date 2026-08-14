using System.Collections.Generic;
using Core;

namespace Service
{
    public interface IDataStorage
    {
        public List<SaveDataInfo> GetSaveDataInfoList();
        public bool ManualSave(SaveData saveData);
        public bool AutoSave(SaveData saveData);
        public bool TryLoad(int id, out SaveData saveData);
    }
}