using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace redis4netUsage
{
	using System.Net.Mime;

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
			var dict = new Dictionary<string, string>();
			dict.Add("mykey", "myvalue");
			dict.Add("mykey2", "myvalue2");
			dict.Add("Message", "my message hello world");
			logger.Info(dict);
			logger.Error(dict, new ArgumentException("Argument exception encountered"));
		}
	}
}
