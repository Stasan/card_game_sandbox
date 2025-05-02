using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace Game
{
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
}