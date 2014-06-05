using System.Threading.Tasks;

namespace redis4net.Redis
{
	public interface IConnection
	{
		void Open(string hostname, int port, string listName);
		bool IsOpen();
		Task<long> AddToList(string content);
	}
}
