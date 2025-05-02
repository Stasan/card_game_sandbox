using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoBehaviour to wire up core systems and show initial screen.
/// Attach to a "Bootstrap" GameObject in your startup scene.
/// </summary>
public class BootstrapperBehaviour : MonoBehaviour, IBootstrapper
{
	[SerializeField] private RectTransform screenParent;
	private IConfigLoader _configLoader;
	private ISaveManager _saveManager;
	private ScreenManager _screenManager;

	public async void Bootstrap()
	{
		CardSpriteProvider.Initialize("CardAtlas");
		_configLoader = new ConfigLoader();
		_saveManager = new SaveManager();
		_screenManager = new ScreenManager(screenParent);
		_configLoader.Initialize();
		// Load decks from config
		var config = _configLoader.LoadConfig<GameConfig>("GameConfig");
		var deckModels = new List<DeckModel>();
		if (config != null && config.Decks != null)
		{
			foreach (var dc in config.Decks)
			{
				var cards = new List<ICardModel>();
				foreach (var id in dc.CardIds) cards.Add(new CardModel { CardId = id });
				deckModels.Add(new DeckModel(dc.DeckId, cards));
			}
		}

		var gameModel = new GameScreenModel(deckModels);
		_screenManager.RegisterScreen("Game", "GameScreen", gameModel,
			v => new GameScreenPresenter((IGameScreenView)v, gameModel, _saveManager));
		await _screenManager.ShowScreenAsync("Game");
	}

	private void Awake() => Bootstrap();
}