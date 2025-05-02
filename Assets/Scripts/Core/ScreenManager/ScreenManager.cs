using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Architecture;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.ScreenManager
{
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
}