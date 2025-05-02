using System;

namespace Game.GameScreen.Model
{
	[Serializable]
	public class CardModel : ICardModel
	{
		public string CardId;
		string ICardModel.CardId => CardId;
	}
}