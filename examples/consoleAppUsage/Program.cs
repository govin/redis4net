using System;
using System.Collections.Generic;
using System.IO;
using log4net.Repository.Hierarchy;

namespace redis4netUsage
{
	using log4net;
	using log4net.Config;

	class Program
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

		static Program()
		{
#if NETCOREAPP
			XmlConfigurator.Configure(new Hierarchy(), File.OpenRead("log4net.config"));
#else
			XmlConfigurator.Configure(File.OpenRead("log4net.config"));
#endif
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