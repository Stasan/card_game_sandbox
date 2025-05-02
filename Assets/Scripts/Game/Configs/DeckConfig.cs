using System;
using System.Collections.Generic;

namespace Game.Configs
{
	[Serializable]
	public class DeckConfig
	{
		public string DeckId;
		public List<string> CardIds;
	}
}