using System;
using System.Collections.Generic;

namespace redis4netUsage
{
	using log4net;
	using log4net.Config;

	class Program
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Program).Name);

		static Program()
		{
			XmlConfigurator.Configure();
		}

		static void Main(string[] args)
		{
			logger.Info("Hello world from redis appender");
			var dict = new Dictionary<string, object>();
			dict.Add("mykey", "myvalue");
			dict.Add("mykey2", "myvalue2");
			dict.Add("Message", "my message hello world");
			dict.Add("IntegerValue", 123);
			logger.Info(dict);
			logger.Error(dict, new ArgumentException("Argument exception encountered"));
		}
	}
}