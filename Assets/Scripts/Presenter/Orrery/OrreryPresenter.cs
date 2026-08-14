using System.Collections.Generic;
using Core;
using Model;
using UnityEngine;
using View;

namespace Presenter
{
    public class OrreryPresenter : IUpdatable, ISaveDataReader, ISaveDataWriter
    {
        private OrreryModel _model;

        public OrreryPresenter(OrreryModel orreryModel)
        {
            _model = orreryModel;
        }
        
        public void OnUpdate(float deltaTime)
        {
            _model.Tick(deltaTime);
        }

        public void ReadFrom(SaveData data)
        {
            OrrerySaveData d = data.orrery;
            _model.Init(d.orreryCumulativeSeconds);
        }

        public void WriteTo(SaveData data)
        {
            OrrerySaveData d = new OrrerySaveData();
            d.orreryCumulativeSeconds = _model.OrreryCumulativeSeconds;
            data.orrery = d;
        }
    }
}