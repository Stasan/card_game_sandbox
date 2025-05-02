using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.GameScreen.View
{
    public class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public string CardId { get; private set; }
        public string CurrentDeckId { get; set; }
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private RectTransform _originalParent;
        private int _originalIndex;
        private Vector2 _originalPosition;
        private RectTransform _dragContainer;
        private Vector2 _pointerOffset;

        public void Initialize(string cardId, string deckId, RectTransform dragContainer)
        {
            CardId = cardId;
            CurrentDeckId = deckId;
            _dragContainer = dragContainer;
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData e)
        {
            _originalParent = _rectTransform.parent as RectTransform;
            _originalIndex = _rectTransform.GetSiblingIndex();
            _originalPosition = _rectTransform.anchoredPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragContainer, e.position, e.pressEventCamera,
                out var local);
            _pointerOffset = Vector2.zero;
            _canvasGroup.blocksRaycasts = false;
            _rectTransform.SetParent(_dragContainer, false);
            _rectTransform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragContainer, e.position, e.pressEventCamera,
                out var local);
            _rectTransform.anchoredPosition = local + _pointerOffset;
        }

        public void OnEndDrag(PointerEventData e)
        {
            _canvasGroup.blocksRaycasts = true;
            bool dropped = false;
            foreach (var go in e.hovered)
                if (go.GetComponentInParent<DeckArea>() != null)
                {
                    dropped = true;
                    break;
                }

            if (!dropped)
            {
                _rectTransform.SetParent(_originalParent, false);
                _rectTransform.SetSiblingIndex(_originalIndex);
                _rectTransform.anchoredPosition = _originalPosition;
            }
        }
    }
}