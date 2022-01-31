using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Interactions
{
    public class CardSwipeItem : MonoBehaviour, IBeginDragHandler, 
        IEndDragHandler, IDragHandler
    {
        [SerializeField] private Canvas _canvas;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform startAnchorPoint;
        [SerializeField] private RectTransform targetAnchorPoint;
        [SerializeField] private TextMeshProUGUI extraMessage;

        [SerializeField] private float targetGapTolerance = 100f;
        [SerializeField] private int attemptsRequired = 0;
        [SerializeField] private float swipeSpeed = 0;
        
        [SerializeField] private CardSwipeTask cardSwipeTask;

        private List<string> extraMessagesListText = new List<string>()
        {
            "Try again slowly...",
            "Too fast!!",
            "Easy Buddy!",
            "Slowly Swipe it."
        };

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform.anchoredPosition = startAnchorPoint.anchoredPosition;
            attemptsRequired = Random.Range(2, 6);
            extraMessage.gameObject.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _canvasGroup.alpha = .6f;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.alpha = 1f;
            
            if (Vector2.Distance(_rectTransform.anchoredPosition, 
                targetAnchorPoint.anchoredPosition) < targetGapTolerance)
            {
                attemptsRequired--;

                if (attemptsRequired <= 0)
                {
                    _rectTransform.anchoredPosition = targetAnchorPoint.anchoredPosition;
                    cardSwipeTask.TaskFinish();
                }
                else
                {
                    DisplayExtraMessage(extraMessagesListText[Random.Range(0, extraMessagesListText.Count)]);
                    ThrowToStartingPos();
                }
            }
            else
            {
                ThrowToStartingPos();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 updatedAnchoredPosition = eventData.delta / _canvas.scaleFactor;
            updatedAnchoredPosition = new Vector2(updatedAnchoredPosition.x, _rectTransform.anchoredPosition.y);
            _rectTransform.anchoredPosition += updatedAnchoredPosition;

            swipeSpeed = (eventData.delta / _canvas.scaleFactor).x;
        }

        private void ThrowToStartingPos()
        {
            _rectTransform.anchoredPosition = startAnchorPoint.anchoredPosition;
        }

        private void DisplayExtraMessage(string message)
        {
            StopCoroutine(DisplayExtraMessageRoutine(message));
            StartCoroutine(DisplayExtraMessageRoutine(message));
        }

        private IEnumerator DisplayExtraMessageRoutine(string message)
        {
            extraMessage.SetText(message);
            extraMessage.gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(1f);

            extraMessage.gameObject.SetActive(false);
        }
    }
}
