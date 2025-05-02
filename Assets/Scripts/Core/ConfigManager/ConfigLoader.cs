using UnityEngine;

namespace Core.ConfigManager
{
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
}