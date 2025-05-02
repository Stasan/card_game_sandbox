namespace Core.SaveManager
{
	public interface ISaveManager<T>
	{
		T LoadProgress();
		void SaveProgress(T progress);
	}
}