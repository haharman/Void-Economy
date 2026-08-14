using UnityEngine;
using System.Collections.Generic;
using Presenter;

namespace Service
{
    public class UpdateService : IUpdateService, IUpdatable
    {
        private readonly HashSet<IUpdatable> _updatables = new();
    
        // 実行中のリスト追加・削除による InvalidOperationException 回避用
        private readonly List<IUpdatable> _toAdd = new();
        private readonly List<IUpdatable> _toRemove = new();
        
        // IUpdatable
        public void OnUpdate(float deltaTime)
        {
            ProcessPendingChanges();
            foreach (var updatable in _updatables)
            {
                try
                {
                    updatable.OnUpdate(deltaTime);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[{updatable.GetType().Name}] OnUpdate中に例外: {e}");
                }
            }
        }

        // IUpdateService
        public void Register(IUpdatable updatable)
        {
            if (!_updatables.Contains(updatable)) _toAdd.Add(updatable);
        }

        public void Unregister(IUpdatable updatable)
        {
            if (_updatables.Contains(updatable)) _toRemove.Add(updatable);
        }
        
        public void ClearUpdatables()
        {
            foreach (var updatable in _updatables)
            {
                _toAdd.Clear();
                _toRemove.Add(updatable);
            }
        }

        private void ProcessPendingChanges()
        {
            
            if (_toAdd.Count > 0)
            {
                foreach (var updatable in _toAdd) _updatables.Add(updatable);
                _toAdd.Clear();
            }

            if (_toRemove.Count > 0)
            {
                foreach (var item in _toRemove) _updatables.Remove(item);
                _toRemove.Clear();
            }
        }
    }
}