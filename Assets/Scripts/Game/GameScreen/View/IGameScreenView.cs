using System;
using System.Collections.Generic;
using Core.Architecture;
using Game.GameScreen.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameScreen.View
{
	public interface IGameScreenView : IScreenView
	{
		void SetupDecks(IEnumerable<DeckModel> decks);
		void ClearDeck(string deckId);
		void AddCardToDeck(string deckId, ICardModel card);
		event Action<string, string, string> OnCardDropped;
		event Action OnUndoRequested;
		RectTransform DragContainer { get; }
		Button UndoButton { get; }
	}
}