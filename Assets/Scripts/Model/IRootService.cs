using System;
using Core;

namespace Model
{
    public interface IDataService
    {
        public void SaveGame();
        public SceneType LoadGame();
        public void NewGame();
    }

    public interface ISceneService
    {
        public void LoadScene(SceneType sceneType);
        public event Action<SceneType> OnLoadStart;
        public event Action<SceneType> OnLoadComplete;
    }
    
    public interface IQuitService
    {
        public void QuitGame();
    }
}