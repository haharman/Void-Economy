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
            // 削除待ちの再登録は削除を取り消す（取り消さないと次回の処理で外れてしまう）
            if (_updatables.Contains(updatable)) _toRemove.Remove(updatable);
            else if (!_toAdd.Contains(updatable)) _toAdd.Add(updatable);
        }

        public void Unregister(IUpdatable updatable)
        {
            // 追加待ち（次回 OnUpdate で反映予定）のものも取り消す
            _toAdd.Remove(updatable);
            if (_updatables.Contains(updatable) && !_toRemove.Contains(updatable)) _toRemove.Add(updatable);
        }
        
        // 削除を検討中（インターフェースからはいったん削除している）
        public void ClearUpdatables()
        {
            _toAdd.Clear();
            foreach (var updatable in _updatables)
            {
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