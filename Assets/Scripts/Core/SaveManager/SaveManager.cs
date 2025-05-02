
using UnityEngine;

namespace Core.SaveManager
{
	public class SaveManager<T> : ISaveManager<T> where T : new()
	{
		private const string SaveKey = "USER_PROGRESS";

		public T LoadProgress()
		{
			if (PlayerPrefs.HasKey(SaveKey)) return JsonUtility.FromJson<T>(PlayerPrefs.GetString(SaveKey));
			return new T();
		}

		public void SaveProgress(T progress)
		{
			PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(progress));
			PlayerPrefs.Save();
		}
	}
}