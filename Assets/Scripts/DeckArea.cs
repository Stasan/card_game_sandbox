using UnityEngine;
using UnityEngine.EventSystems;


public class DeckArea : MonoBehaviour, IDropHandler
{
	public string DeckId => gameObject.name;

	public void OnDrop(PointerEventData e)
	{
		var cardGO = e.pointerDrag;
		if (cardGO == null) return;
		var cv = cardGO.GetComponent<CardView>();
		if (cv == null) return;
		var rect = cardGO.GetComponent<RectTransform>();
		rect.SetParent(transform, false);
		rect.SetAsLastSibling();
		rect.anchoredPosition = Vector2.zero;
		var prev = cv.CurrentDeckId;
		cv.CurrentDeckId = DeckId;
		var sv = GameObject.FindObjectOfType<GameScreenView>();
		sv?.CardDropped(cv.CardId, prev, DeckId);
	}
}