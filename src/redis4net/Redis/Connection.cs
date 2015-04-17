using System;

using StackExchange.Redis;

namespace redis4net.Redis
{
	
	public class Connection : IConnection, IDisposable
	{
	    private readonly int _connectTimeOut;
	    private readonly int _connectRetry;
	    private readonly string _listName;
        private ConnectionMultiplexer _redis;
	    private readonly ConfigurationOptions _configurationOptions;
	    private readonly string _remoteAddress;
	    private readonly int _remotePort;
	    private readonly bool _abortOnConnectFail;

	    public Connection(string remoteAddress, int remotePort, int connectTimeOut, int connectRetry, bool abortOnConnectFail, string listName)
	    {
	        _remoteAddress = remoteAddress;
	        _remotePort = remotePort;
	        _connectTimeOut = connectTimeOut;
	        _connectRetry = connectRetry;
            _abortOnConnectFail = abortOnConnectFail;

            _configurationOptions = CreateConfiguration();
            
            _listName = listName;
	    }

	    private ConfigurationOptions CreateConfiguration()
	    {
	        ConfigurationOptions configurationOptions = new ConfigurationOptions
            {
                EndPoints =
                {
                     { _remoteAddress, _remotePort }
                },
                ConnectTimeout = _connectTimeOut,
                ConnectRetry = _connectRetry,
                AbortOnConnectFail = _abortOnConnectFail
            };

	        return configurationOptions;
	    }

		public void Open()
		{
			try
			{
                _redis = ConnectionMultiplexer.Connect(_configurationOptions);
			}
			catch
			{
				_redis = null;
			}
		}

		public bool IsOpen()
		{
			return _redis != null;
		}

		public void AddToList(string content)
		{
            var database = _redis.GetDatabase();
            database.ListRightPushAsync(_listName, new RedisValue[] { content }, CommandFlags.FireAndForget);
		}

		public void Dispose()
		{
            if (_redis == null)
            {
                return;
            }

            _redis.Close(false);
            _redis.Dispose();
		}
	}
}
