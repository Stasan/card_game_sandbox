using System.Collections.Generic;
using Core.Architecture;

namespace Game.GameScreen.Model
{
	public class GameScreenModel : IScreenModel
	{
		public List<DeckModel> Decks { get; private set; }

		public GameScreenModel(IEnumerable<DeckModel> decks)
		{
			Decks = new List<DeckModel>(decks);
		}
	}
}