using UnityEngine;
using System.Collections.Generic;

namespace Model
{
    public class ClockProvider
    {
        private readonly List<IUpdatable> _tickables = new();
    
        // 実行中のリスト追加・削除による InvalidOperationException 回避用
        private readonly List<IUpdatable> _toAdd = new();
        private readonly List<IUpdatable> _toRemove = new();

        public void Register(IUpdatable tickable) => _toAdd.Add(tickable);
        public void Unregister(IUpdatable tickable) => _toRemove.Add(tickable);

        public ClockProvider()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            _toAdd.Clear();
            _toRemove.Clear();
        }

        private void OnUpdate()
        {
            ProcessPendingChanges();

            float dt = Time.deltaTime;
            foreach (var tickable in _tickables)
            {
                tickable.OnUpdate(dt);
            }
        }

        private void ProcessPendingChanges()
        {
            if (_toAdd.Count > 0)
            {
                _tickables.AddRange(_toAdd);
                _toAdd.Clear();
            }

            if (_toRemove.Count > 0)
            {
                foreach (var item in _toRemove) _tickables.Remove(item);
                _toRemove.Clear();
            }
        }
    }
}