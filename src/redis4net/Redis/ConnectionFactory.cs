namespace redis4net.Redis
{
	public interface IConnectionFactory
	{
		IConnection GetConnection();
	}
}
