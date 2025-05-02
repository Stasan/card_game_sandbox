using System;
using TMPro;
using UnityEngine;

/// <summary>
/// MonoBehaviour attached to GameScreen prefab for UI logic.
/// Uses TextMeshPro for text elements.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class GameScreenView : MonoBehaviour, IGameScreenView
{
	[SerializeField] private UnityEngine.UI.Button playButton;
	[SerializeField] private TextMeshProUGUI scoreText; // TextMeshPro field

	public event Action OnPlayButtonClicked;
	private IScreenPresenter _presenter;

	public void SetPresenter(IScreenPresenter presenter)
	{
		_presenter = presenter;
	}

	private void Awake()
	{
		playButton.onClick.AddListener(() => OnPlayButtonClicked?.Invoke());
	}

	public void DisplayScore(int score)
	{
		scoreText.text = score.ToString();
	}
}