using UnityEngine;
using UnityEngine.UI;
using System;

namespace View
{
    public class TitleView : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button exitButton;

        public event Action OnStartClicked;
        public event Action OnLoadClicked;
        public event Action OnExitClicked;

        private void Awake()
        {
            startButton.onClick.AddListener(() => OnStartClicked?.Invoke());
            loadButton.onClick.AddListener(() => OnLoadClicked?.Invoke());
            exitButton.onClick.AddListener(() => OnExitClicked?.Invoke());
        }

        public void SetLoadButtonActive(bool active) => loadButton.interactable = active;
    }
}