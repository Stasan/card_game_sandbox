using UnityEngine;

/// <summary>
/// MonoBehaviour to wire up core systems and show initial screen.
/// Attach to a "Bootstrap" GameObject in your startup scene.
/// </summary>
public class BootstrapperBehaviour : MonoBehaviour, IBootstrapper
{
	[Header("UI Setup")] [SerializeField]
	private RectTransform screenParent;

	private IConfigLoader _configLoader;
	private ISaveManager _saveManager;
	private ScreenManager _screenManager;

	public async void Bootstrap()
	{
		// 1) Instantiate core managers
		_configLoader = new ConfigLoader();
		_saveManager  = new SaveManager();
		_screenManager= new ScreenManager(screenParent);

		// 2) Initialize and load data
		_configLoader.Initialize();
		var progress = _saveManager.LoadProgress();

		// 3) Register screens
		var gameModel = new GameScreenModel { Score = progress.Score };
		_screenManager.RegisterScreen(
			alias: "Game",
			addressableKey: "GameScreen",
			modelInstance: gameModel,
			presenterFactory: view => new GameScreenPresenter(
				(IGameScreenView)view, gameModel, _saveManager)
		);

		// 4) Show initial screen
		await _screenManager.ShowScreenAsync("Game");
	}

	private void Awake()
	{
		Bootstrap();
	}
}