using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// MonoBehaviour attached to GameScreen prefab for UI logic.
/// </summary>

public class GameScreenView : MonoBehaviour, IGameScreenView
{
	[SerializeField] private RectTransform deckContainerParent;
	[SerializeField] private RectTransform dragContainer;
	[SerializeField] private GameObject deckAreaPrefab;
	[SerializeField] private GameObject cardPrefab;
	[SerializeField] private Button undoButton;
	private Dictionary<string, RectTransform> _deckAreas = new();
	public event Action<string, string, string> OnCardDropped;
	public event Action OnUndoRequested;
	public RectTransform DragContainer => dragContainer;
	public Button UndoButton => undoButton;

	private void Awake()
	{
		if (undoButton != null) undoButton.onClick.AddListener(() => OnUndoRequested?.Invoke());
	}

	public void SetPresenter(IScreenPresenter presenter)
	{
	}

	public void SetupDecks(IEnumerable<DeckModel> decks)
	{
		foreach (var deck in decks)
		{
			var go = Instantiate(deckAreaPrefab, deckContainerParent);
			go.name = deck.DeckId;
			var rt = go.GetComponent<RectTransform>();
			rt.localScale = Vector3.one;
			_deckAreas[deck.DeckId] = rt;
			ClearDeck(deck.DeckId);
			foreach (var c in deck.Cards) AddCardToDeck(deck.DeckId, c);
		}
	}

	public void ClearDeck(string deckId)
	{
		if (_deckAreas.TryGetValue(deckId, out var rt))
			foreach (Transform t in rt)
				Destroy(t.gameObject);
	}

	public void AddCardToDeck(string deckId, ICardModel card)
	{
		if (_deckAreas.TryGetValue(deckId, out var rt))
		{
			var cardGO = Instantiate(cardPrefab, rt);
			cardGO.name = card.CardId;
			var cv = cardGO.GetComponent<CardView>() ?? cardGO.AddComponent<CardView>();
			cv.Initialize(card.CardId, deckId, dragContainer);
			if (cardGO.TryGetComponent<Image>(out var img))
			{
				img.sprite = CardSpriteProvider.GetSprite(card.CardId);
				img.SetNativeSize();
			}
		}
	}

	public void CardDropped(string cvCardId, string prev, string deckId)
	{
		OnCardDropped?.Invoke(cvCardId, prev, deckId);
	}
}
