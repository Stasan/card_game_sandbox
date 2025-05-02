namespace Core.ConfigManager
{
	public interface IConfigLoader
	{
		void Initialize();
		T LoadConfig<T>(string configName);
	}
}