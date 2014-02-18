using System;
using System.Threading.Tasks;
using BookSleeve;

namespace redis4net.Redis
{
	public class Connection : IConnection, IDisposable
	{
		private RedisConnection _connection;
		private string _listName;

		public void Open(string hostname, int port, string listName)
		{
			try
			{
				_listName = listName;

				_connection = new RedisConnection(hostname, port);
				_connection.Open();
			}
			catch
			{
				_connection = null;
			}
		}

		public bool IsOpen()
		{
			return _connection != null && _connection.State == RedisConnectionBase.ConnectionState.Open;
		}

		public Task<long> AddToList(string content)
		{
			return _connection.Lists.AddLast(0, _listName, content);
		}

		public void Dispose()
		{
			if (_connection == null)
			{
				return;
			}

			_connection.Close(false);
			_connection.Dispose();
		}
	}
}
