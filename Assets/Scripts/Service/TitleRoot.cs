using Core;
using UnityEngine;
using View;
using Presenter;
using Model;
using UnityEngine.SceneManagement;

namespace Service
{
    public class TitleRoot : MonoBehaviour
    {
        
        [SerializeField] private TitleView view;
        
        private TitlePresenter _presenter;

        public void Init(IUpdatableService updatableService)
        {
            Debug.Log("[TitleRoot] Init");
            _presenter = new TitlePresenter(view);
        }
    }
}