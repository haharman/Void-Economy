using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace View
{
    public class Window : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private float heightRate;
        [SerializeField] private float aspect;
        [SerializeField] private float minHeightPixels;
        [SerializeField] private float minVisibleTopBarWidth;
        [SerializeField] private RectTransform windowRect;
        [SerializeField] private RectTransform topBarRect;

        private bool _isDragging;

        public event Action OnClosed;

        private void Start()
        {
            UpdateWindowSize();
        }

        public void UpdateWindowSize()
        {
            var parentRect = windowRect.parent as RectTransform;
            if (parentRect == null) return;

            float height = heightRate * parentRect.rect.height; // Mathf.Max(heightRate * parentRect.rect.height, minHeightPixels);
            float width = height * aspect;
            windowRect.sizeDelta = new Vector2(width, height);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(topBarRect, eventData.position, eventData.pressEventCamera))
            {
                _isDragging = true;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            windowRect.anchoredPosition += eventData.delta;
            ClampPosition();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;

            gameObject.SetActive(false);
            OnClosed?.Invoke();
        }

        private void ClampPosition()
        {
            var parentRect = windowRect.parent as RectTransform;
            if (parentRect == null) return;

            float halfParentWidth = parentRect.rect.width * 0.5f;
            float halfParentHeight = parentRect.rect.height * 0.5f;
            float halfWindowWidth = windowRect.rect.width * 0.5f;
            float halfWindowHeight = windowRect.rect.height * 0.5f;
            float topBarHeight = topBarRect.rect.height;

            Vector2 position = windowRect.anchoredPosition;

            // 横方向：TopBarの幅のうち、最低minVisibleTopBarWidth分は常に親領域内に収める
            float xMin = minVisibleTopBarWidth - halfWindowWidth - halfParentWidth;
            float xMax = halfParentWidth + halfWindowWidth - minVisibleTopBarWidth;
            position.x = Mathf.Clamp(position.x, xMin, xMax);

            // 縦方向：TopBarの高さ全体を常に親領域内に収める
            float yMin = -halfParentHeight - halfWindowHeight + topBarHeight;
            float yMax = halfParentHeight - halfWindowHeight;
            position.y = Mathf.Clamp(position.y, yMin, yMax);

            windowRect.anchoredPosition = position;
        }
    }
}
