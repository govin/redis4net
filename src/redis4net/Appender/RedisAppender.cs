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

		public override void ActivateOptions()
		{
			base.ActivateOptions();

			InitializeConnectionFactory();
		}

		protected virtual void InitializeConnectionFactory()
		{
			var connection = new Connection();
			ConnectionFactory = new ConnectionFactory(connection, RemoteAddress, RemotePort, 1, ListName);
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
