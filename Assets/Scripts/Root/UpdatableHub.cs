using UnityEngine;
using System.Collections.Generic;
using Presenter;

namespace Root
{
    public class UpdatableHub : IUpdatableService
    {
        private readonly HashSet<IUpdatable> _globalUpdatables = new();
        private readonly HashSet<IUpdatable> _updatables = new();
    
        // 実行中のリスト追加・削除による InvalidOperationException 回避用
        private readonly List<IUpdatable> _toAddGlobal = new();
        private readonly List<IUpdatable> _toRemoveGlobal = new();
        private readonly List<IUpdatable> _toAdd = new();
        private readonly List<IUpdatable> _toRemove = new();

        public void RegisterGlobal(IUpdatable updatable)
        {
            if (!_globalUpdatables.Contains(updatable)) _toAddGlobal.Add(updatable);
        }

        public void UnregisterGlobal(IUpdatable updatable)
        {
            if (_globalUpdatables.Contains(updatable)) _toRemoveGlobal.Add(updatable);
        }

        public void Register(IUpdatable updatable)
        {
            if (!_updatables.Contains(updatable)) _toAdd.Add(updatable);
        }

        public void Unregister(IUpdatable updatable)
        {
            if (_updatables.Contains(updatable)) _toRemove.Add(updatable);
        }

        public void OnUpdate()
        {
            ProcessPendingChanges();

            float dt = Time.deltaTime;
            foreach (var updatable in _globalUpdatables)
            {
                try
                {
                    updatable.OnUpdate(dt);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
            foreach (var updatable in _updatables)
            {
                try
                {
                    updatable.OnUpdate(dt);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void ClearUpdatables()
        {
            _updatables.Clear();
        }

        private void ProcessPendingChanges()
        {
            if (_toAddGlobal.Count > 0)
            {
                foreach (var updatable in _toAddGlobal) _globalUpdatables.Add(updatable);
                _toAddGlobal.Clear();
            }

            if (_toRemoveGlobal.Count > 0)
            {
                foreach (var item in _toRemoveGlobal) _globalUpdatables.Remove(item);
                _toRemoveGlobal.Clear();
            }
            
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