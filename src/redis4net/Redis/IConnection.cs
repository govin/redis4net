namespace redis4net.Redis
{
	public interface IConnection
	{
		void Open();
		bool IsOpen();
		void AddToList(string content);
	}
}
