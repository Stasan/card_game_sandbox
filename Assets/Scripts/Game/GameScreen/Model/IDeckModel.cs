using System.Collections.Generic;

namespace Game.GameScreen.Model
{
	public interface IDeckModel
	{
		string DeckId { get; }
		List<ICardModel> Cards { get; }
	}
}