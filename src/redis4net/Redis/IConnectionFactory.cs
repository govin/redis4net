namespace redis4net.Redis
{

	public class ConnectionFactory : IConnectionFactory
	{
		private static readonly object Lock = new object();

		private readonly IConnection _connection;

		public ConnectionFactory(IConnection connection)
		{
			_connection = connection;
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
					_connection.Open();
				}
				catch
				{
					// Nothing to do if this fails
				}
			}
		}
	}
}
