using log4net.Appender;

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
			var message = RenderLoggingEvent(loggingEvent);
			ConnectionFactory.GetConnection().AddToList(message);
		}
	}
}
