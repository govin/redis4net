using System;

using log4net.Appender;
using log4net.Core;

namespace redis4net.Appender
{
	using redis4net.Redis;

	public class RedisAppender : AppenderSkeleton
	{
		protected ConnectionFactory ConnectionFactory { get; set; }
		public string RemoteAddress { get; set; }
		public int RemotePort { get; set; }
		public string ListName { get; set; }
        public bool AbortOnConnectFail { get; set; }
        public int ConnectRetry { get; set; }
        public int ConnectTimeOut {get; set; }

	    public RedisAppender()
	    {
	        AbortOnConnectFail = false;
	        ConnectRetry = 2;
	        ConnectTimeOut = 1000;
	    }

		public override void ActivateOptions()
		{
			base.ActivateOptions();

			InitializeConnectionFactory();
		}

		protected virtual void InitializeConnectionFactory()
		{
            var connection = new Connection(RemoteAddress, RemotePort, ConnectTimeOut, ConnectRetry, AbortOnConnectFail, ListName);
			ConnectionFactory = new ConnectionFactory(connection);
		}

		protected override void Append(log4net.Core.LoggingEvent loggingEvent)
		{
			try
			{
				var message = RenderLoggingEvent(loggingEvent);
				ConnectionFactory.GetConnection().AddToList(message);
			}
			catch(Exception exception)
			{
				ErrorHandler.Error("Unable to send logging event to remote redis host " + RemoteAddress + " on port " + RemotePort, exception, ErrorCode.WriteFailure);
			}
		}
	}
}
