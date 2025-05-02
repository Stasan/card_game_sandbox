using System.Collections.Generic;
using Game.GameScreen.Model;

public class DeckModel : IDeckModel
{
	public string DeckId { get; private set; }
	public List<ICardModel> Cards { get; private set; }

	public DeckModel(string deckId, IEnumerable<ICardModel> cards)
	{
		DeckId = deckId;
		Cards = new List<ICardModel>(cards);
	}
}