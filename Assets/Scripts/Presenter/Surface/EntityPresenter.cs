using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using View;
using Model;
using R3;

namespace Presenter
{
    public class EntityPresenter
    {
        // Model
        private List<Entity> _modelList;
        
        // View
        private List<EntityView> _viewList;
        
        public EntityPresenter(List<Entity> modelList, List<EntityView> viewList, CancellationToken cancellationToken)
        {
            _modelList = modelList;
            _viewList = viewList;
            if (_modelList.Count != _viewList.Count)
                Debug.LogError("[EntityPresenter] ModelとViewの数が一致しません");
            for (int i = 0; i < _modelList.Count; i++)
            {
                _modelList[i].Position
                    .Subscribe(pos => _viewList[i].OnUpdatePosition(pos))
                    .RegisterTo(cancellationToken);
            }
        }
    }
}