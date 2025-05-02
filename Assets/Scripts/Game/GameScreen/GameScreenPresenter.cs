using System.Collections.Generic;
using Core.Architecture;
using Core.SaveManager;
using Game.GameScreen.Model;
using Game.GameScreen.View;
using Game.Saves;

namespace Game.GameScreen
{
	public class GameScreenPresenter : IScreenPresenter
	{
		private readonly IGameScreenView _view;
		private readonly GameScreenModel _model;
		private readonly ISaveManager<UserProgress> _saveManager;
		private Stack<(string card, string from, string to)> _history = new();

		public GameScreenPresenter(IGameScreenView v, GameScreenModel m, ISaveManager<UserProgress> s)
		{
			_view = v;
			_model = m;
			_saveManager = s;
		}

		public void Initialize()
		{
			_view.SetupDecks(_model.Decks);
			_view.OnCardDropped += OnDropped;
			_view.OnUndoRequested += OnUndo;
		}

		public void OnDestroy()
		{
			_view.OnCardDropped -= OnDropped;
			_view.OnUndoRequested -= OnUndo;
		}

		private void OnDropped(string card, string from, string to)
		{
			_history.Push((card, from, to));
			Swap(card, from, to);
		}

		private void OnUndo()
		{
			if (_history.Count == 0) return;
			var (card, from, to) = _history.Pop();
			Swap(card, to, from);
		}

		private void Swap(string cardId, string srcDeck, string dstDeck)
		{
			var src = _model.Decks.Find(d => d.DeckId == srcDeck);
			var dst = _model.Decks.Find(d => d.DeckId == dstDeck);
			var c = src.Cards.Find(x => x.CardId == cardId);
			if (c != null && dst != null)
			{
				src.Cards.Remove(c);
				dst.Cards.Add(c);
				Refresh(srcDeck);
				Refresh(dstDeck);
				_saveManager.SaveProgress(new UserProgress { Level = 1, Score = _model.Decks.Count });
			}
		}

		private void Refresh(string deckId)
		{
			_view.ClearDeck(deckId);
			foreach (var c in _model.Decks.Find(d => d.DeckId == deckId).Cards) _view.AddCardToDeck(deckId, c);
		}
	}
}