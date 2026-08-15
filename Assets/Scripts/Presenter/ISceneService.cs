using System;
using Core;
using R3;

namespace Presenter
{
    public interface ISceneLoader
    {
        public void LoadScene(SceneId sceneType);
    }
}