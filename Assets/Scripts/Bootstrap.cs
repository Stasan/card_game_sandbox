using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;

#region Core Interfaces

public interface IBootstrapper
{
	void Bootstrap();
}

public interface IConfigLoader
{
	void Initialize();
	T LoadConfig<T>(string configName);
}

public interface ISaveManager
{
	UserProgress LoadProgress();
	void SaveProgress(UserProgress progress);
}

public interface IScreenModel
{
}

public interface IScreenView
{
	void SetPresenter(IScreenPresenter presenter);
}

public interface IScreenPresenter
{
	void Initialize();
	void OnDestroy();
}

public interface ICardModel
{
	string CardId { get; }
}

public interface IDeckModel
{
	string DeckId { get; }
	List<ICardModel> Cards { get; }
}

#endregion

#region Data & Config Models

[Serializable]
public class UserProgress
{
	public int Level;
	public int Score;
}

[Serializable]
public class CardModel : ICardModel
{
	public string CardId;
	string ICardModel.CardId => CardId;
}

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

// Config container matching JSON structure
[Serializable]
public class DeckConfig
{
	public string DeckId;
	public List<string> CardIds;
}

[Serializable]
public class GameConfig
{
	public List<DeckConfig> Decks;
}

public class GameScreenModel : IScreenModel
{
	public List<DeckModel> Decks { get; private set; }

	public GameScreenModel(IEnumerable<DeckModel> decks)
	{
		Decks = new List<DeckModel>(decks);
	}
}

#endregion

#region Sprite Provider

public static class CardSpriteProvider
{
	private static SpriteAtlas _atlas;

	public static void Initialize(string atlasKey)
	{
		Addressables.LoadAssetAsync<SpriteAtlas>(atlasKey).Completed += h =>
		{
			if (h.Status == AsyncOperationStatus.Succeeded) _atlas = h.Result;
			else Debug.LogError($"Failed to load atlas '{h.DebugName}'");
		};
	}

	public static Sprite GetSprite(string cardId) => _atlas != null ? _atlas.GetSprite(cardId) : null;
}

#endregion

#region Config Loader & Save Manager

public class ConfigLoader : IConfigLoader
{
	public void Initialize() => Debug.Log("ConfigLoader initialized");

	public T LoadConfig<T>(string configName)
	{
		var asset = Resources.Load<TextAsset>($"Configs/{configName}");
		if (asset == null)
		{
			Debug.LogError($"Config '{configName}' not found in Resources/Configs");
			return default;
		}

		return JsonUtility.FromJson<T>(asset.text);
	}
}

public class SaveManager : ISaveManager
{
	private const string SaveKey = "USER_PROGRESS";

	public UserProgress LoadProgress()
	{
		if (PlayerPrefs.HasKey(SaveKey)) return JsonUtility.FromJson<UserProgress>(PlayerPrefs.GetString(SaveKey));
		return new UserProgress();
	}

	public void SaveProgress(UserProgress progress)
	{
		PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(progress));
		PlayerPrefs.Save();
	}
}

#endregion

#region MVP Screen System

public class ScreenManager
{
	private readonly RectTransform _screenParent;
	private readonly Dictionary<string, ScreenRegistration> _registry = new();
	private GameObject _currentGO;
	private IScreenPresenter _currentPresenter;

	public ScreenManager(RectTransform screenParent)
	{
		_screenParent = screenParent;
	}

	public void RegisterScreen(string alias, string addressableKey, IScreenModel model,
		Func<IScreenView, IScreenPresenter> factory)
	{
		_registry[alias] = new ScreenRegistration(addressableKey, model, factory);
	}

	public async Task ShowScreenAsync(string alias)
	{
		if (!_registry.TryGetValue(alias, out var reg)) throw new ArgumentException($"Alias '{alias}' not found.");
		_currentPresenter?.OnDestroy();
		if (_currentGO != null) GameObject.Destroy(_currentGO);
		var handle = Addressables.LoadAssetAsync<GameObject>(reg.AddressableKey);
		await handle.Task;
		var prefab = handle.Result;
		_currentGO = GameObject.Instantiate(prefab, _screenParent, false);
		if (_currentGO.TryGetComponent<RectTransform>(out var rt))
		{
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.anchoredPosition = Vector2.zero;
			rt.sizeDelta = Vector2.zero;
			rt.localScale = Vector3.one;
		}

		var view = _currentGO.GetComponent<IScreenView>();
		if (view == null) throw new InvalidOperationException("View must implement IScreenView");
		_currentPresenter = reg.PresenterFactory(view);
		view.SetPresenter(_currentPresenter);
		_currentPresenter.Initialize();
	}

	private class ScreenRegistration
	{
		public string AddressableKey;
		public IScreenModel Model;
		public Func<IScreenView, IScreenPresenter> PresenterFactory;

		public ScreenRegistration(string key, IScreenModel model, Func<IScreenView, IScreenPresenter> f)
		{
			AddressableKey = key;
			Model = model;
			PresenterFactory = f;
		}
	}
}

#endregion

#region GameScreen MVP Implementation with Undo & Config

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

public class GameScreenPresenter : IScreenPresenter
{
	private readonly IGameScreenView _view;
	private readonly GameScreenModel _model;
	private readonly ISaveManager _saveManager;
	private Stack<(string card, string from, string to)> _history = new();

	public GameScreenPresenter(IGameScreenView v, GameScreenModel m, ISaveManager s)
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

#endregion