using System.Collections.Generic;
using Core;

namespace Presenter
{
    public interface IDataService
    {
        public void Save(bool auto);
        public SceneType Load(int id);
        public SceneType LoadNewGame();
        public List<SaveDataInfo> GetSaveSlots();
    }
}