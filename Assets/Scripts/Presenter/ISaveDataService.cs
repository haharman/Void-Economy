using System.Collections.Generic;
using Core;

namespace Presenter
{
    public interface ISaveDataService
    {
        public void Save(bool auto);
        public void Load(int id, bool newGame);
        public List<SaveDataInfo> GetSaveSlots();
        public void RegisterWriter(ISaveDataWriter writer);
        public void RegisterReader(ISaveDataReader reader);
        public void UnregisterWriter(ISaveDataWriter writer);
        public void UnregisterReader(ISaveDataReader reader);
    }
}