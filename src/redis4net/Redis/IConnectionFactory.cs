using System;

namespace redis4net.Redis
{
	using System.Threading;

	public class ConnectionFactory : IConnectionFactory
	{
		private static readonly object Lock = new object();

		private readonly string _hostname;
		private readonly int _portNumber;
		private readonly int _failedConnectionRetryTimeoutInSeconds;
		private readonly string _listName;
		private readonly IConnection _connection;
	    private string _connectionString;
	    private readonly bool _usingConnectionString = false;

	    public ConnectionFactory(IConnection connection, string connectionString, int failedConnectionRetryTimeoutInSeconds, string listName)
	    {
            _connection = connection;
            _connectionString = connectionString;
            _failedConnectionRetryTimeoutInSeconds = failedConnectionRetryTimeoutInSeconds;
            _listName = listName;
	        _usingConnectionString = true;
	    }

		public ConnectionFactory(IConnection connection, string hostName, int portNumber, int failedConnectionRetryTimeoutInSeconds, string listName)
		{
			_connection = connection;

			_hostname = hostName;
			_portNumber = portNumber;
			_failedConnectionRetryTimeoutInSeconds = failedConnectionRetryTimeoutInSeconds;
			_listName = listName;
		}

		public IConnection GetConnection()
		{
			InitializeConnection();
			return _connection;
		}

		private void InitializeConnection()
		{
			if (_connection.IsOpen())
			{
				return;
			}

			lock (Lock)
			{
				try
				{
					OpenConnection();

					if (!_connection.IsOpen())
					{
						Thread.Sleep(TimeSpan.FromSeconds(_failedConnectionRetryTimeoutInSeconds));
						OpenConnection();
					}
				}
				catch
				{
					// Nothing to do if this fails
				}
			}
		}

		private void OpenConnection()
		{
			if (!_connection.IsOpen())
			{
			    if (_usingConnectionString)
			    {
			        _connection.OpenWithConnectionString(_connectionString,_listName);
			    }
			    else
			    {
                    _connection.Open(_hostname, _portNumber, _listName);
                }
				
			}
		}
	}
}
