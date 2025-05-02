// -----------------------------
// Integrated Game Architecture with MVP Screen System (TextMeshPro)
// -----------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// TextMeshPro

#region Core Interfaces

/// <summary>
/// Entry point to initialize core systems.
/// </summary>
public interface IBootstrapper
{
    void Bootstrap();
}

/// <summary>
/// Loads configuration assets by name and deserializes them.
/// </summary>
public interface IConfigLoader
{
    void Initialize();
    T LoadConfig<T>(string configName);
}

/// <summary>
/// Saves and loads persistent user progress.
/// </summary>
public interface ISaveManager
{
    UserProgress LoadProgress();
    void SaveProgress(UserProgress progress);
}

/// <summary>
/// Marker for a screen's data/state model.
/// </summary>
public interface IScreenModel { }

/// <summary>
/// Defines contract for view components on screen prefabs.
/// </summary>
public interface IScreenView
{
    void SetPresenter(IScreenPresenter presenter);
}

/// <summary>
/// Presenter logic for a screen: handles events and updates.
/// </summary>
public interface IScreenPresenter
{
    void Initialize();
    void OnDestroy();
}

#endregion

#region Data Models

/// <summary>
/// Serializable user progress data.
/// </summary>
[Serializable]
public class UserProgress
{
    public int Level;
    public int Score;
}

#endregion

#region Config Loader Implementation

/// <summary>
/// Example config loader using Resources and JsonUtility.
/// </summary>
public class ConfigLoader : IConfigLoader
{
    public void Initialize()
    {
        Debug.Log("ConfigLoader: Initialization complete.");
    }

    public T LoadConfig<T>(string configName)
    {
        var textAsset = Resources.Load<TextAsset>($"Configs/{configName}");
        if (textAsset == null)
        {
            Debug.LogError($"Config '{configName}' not found in Resources/Configs.");
            return default;
        }
        return JsonUtility.FromJson<T>(textAsset.text);
    }
}

#endregion

#region Save Manager Implementation

/// <summary>
/// Simple JSON-based save system using PlayerPrefs.
/// </summary>
public class SaveManager : ISaveManager
{
    private const string SaveKey = "USER_PROGRESS";

    public UserProgress LoadProgress()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            return JsonUtility.FromJson<UserProgress>(json);
        }
        return new UserProgress();
    }

    public void SaveProgress(UserProgress progress)
    {
        string json = JsonUtility.ToJson(progress);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log("SaveManager: Progress saved.");
    }
}

#endregion

#region MVP Screen System

/// <summary>
/// Manages dynamic registration and display of screens via Addressables.
/// </summary>
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

    public void RegisterScreen(
        string alias,
        string addressableKey,
        IScreenModel modelInstance,
        Func<IScreenView, IScreenPresenter> presenterFactory)
    {
        _registry[alias] = new ScreenRegistration(addressableKey, modelInstance, presenterFactory);
    }

    public async Task ShowScreenAsync(string alias)
    {
        if (!_registry.TryGetValue(alias, out var reg))
            throw new ArgumentException($"No screen registered under alias '{alias}'");

        // Tear down existing
        _currentPresenter?.OnDestroy();
        if (_currentGO != null) GameObject.Destroy(_currentGO);

        // Load prefab
        var handle = Addressables.LoadAssetAsync<GameObject>(reg.AddressableKey);
        await handle.Task;
        var prefab = handle.Result;

        // Instantiate under Canvas
        // Instantiate under Canvas, resetting local transform
_currentGO = GameObject.Instantiate(prefab, _screenParent, false); // worldPositionStays = false

// Ensure full-screen anchors and correct scale
if (_currentGO.TryGetComponent<RectTransform>(out var rt))
{
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.anchoredPosition = Vector2.zero;
    rt.sizeDelta = Vector2.zero;
    rt.localScale = Vector3.one; // reset scale in case prefab had different scale
}
        /*if (_currentGO.TryGetComponent<RectTransform>(out var rt))
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }*/

        // MVP wiring
        var view = _currentGO.GetComponent<IScreenView>();
        if (view == null)
            throw new InvalidOperationException($"Prefab '{reg.AddressableKey}' must implement IScreenView");

        _currentPresenter = reg.PresenterFactory(view);
        view.SetPresenter(_currentPresenter);
        _currentPresenter.Initialize();
    }

    private class ScreenRegistration
    {
        public string AddressableKey;
        public IScreenModel Model;
        public Func<IScreenView, IScreenPresenter> PresenterFactory;

        public ScreenRegistration(string key, IScreenModel model, Func<IScreenView, IScreenPresenter> factory)
        {
            AddressableKey = key;
            Model = model;
            PresenterFactory = factory;
        }
    }
}

#endregion

#region GameScreen MVP Implementation

/// <summary>
/// Data model for the Game screen.
/// </summary>
public class GameScreenModel : IScreenModel
{
    public int Score { get; set; }
}

/// <summary>
/// View interface for Game screen UI.
/// </summary>
public interface IGameScreenView : IScreenView
{
    event Action OnPlayButtonClicked;
    void DisplayScore(int score);
}

/// <summary>
/// Presenter for Game screen: handles play button logic.
/// </summary>
public class GameScreenPresenter : IScreenPresenter
{
    private readonly IGameScreenView _view;
    private readonly GameScreenModel _model;
    private readonly ISaveManager _saveManager;

    public GameScreenPresenter(IGameScreenView view, GameScreenModel model, ISaveManager saveManager)
    {
        _view = view;
        _model = model;
        _saveManager = saveManager;
    }

    public void Initialize()
    {
        _view.OnPlayButtonClicked += HandlePlayClicked;
        _view.DisplayScore(_model.Score);
    }

    public void OnDestroy()
    {
        _view.OnPlayButtonClicked -= HandlePlayClicked;
    }

    private void HandlePlayClicked()
    {
        _model.Score++;
        _view.DisplayScore(_model.Score);
        _saveManager.SaveProgress(new UserProgress { Level = 1, Score = _model.Score });
    }
}

#endregion

#region BootstrapperBehaviour - Composition Root

#endregion