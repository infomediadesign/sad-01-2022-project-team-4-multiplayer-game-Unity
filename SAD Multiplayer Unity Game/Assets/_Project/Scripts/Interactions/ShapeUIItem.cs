using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Interactions
{
    public class ShapeUIItem : MonoBehaviour, IBeginDragHandler, 
        IEndDragHandler, IDragHandler
    {
        [SerializeField] private Canvas _canvas;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        [SerializeField] private bool lockItem = false;
        [SerializeField] private Vector3 startAnchorPos;

        public ShapeType shapeType;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            startAnchorPos = _rectTransform.anchoredPosition;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (lockItem)
            {
                return;
            }
            _canvasGroup.alpha = .6f;
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (lockItem)
            {
                return;
            }
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (lockItem)
            {
                return;
            }
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }

        public void SetLockState(bool b)
        {
            lockItem = b;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = false;
        }

        public void ThrowToStartingPos()
        {
            _rectTransform.anchoredPosition = startAnchorPos;
        }
    }

    public enum ShapeType
    {
        Circle,
        Square,
        Diamond
    }
}
