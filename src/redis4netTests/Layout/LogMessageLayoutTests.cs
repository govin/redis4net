using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using log4net.Util;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;

namespace redis4netTests.Layout
{
	using redis4net;
	using redis4net.Layout;

	[TestFixture]
	public class LogMessageLayoutTests
	{
		[Test]
		public void StringFormat()
		{
			var layout = new LogMessageLayout();

			var message = new SystemStringFormat(CultureInfo.CurrentCulture, "This is a {0}", "test");
			var loggingEvent = GetLogginEvent(message);

			var result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(message.ToString(), result.Message);
		}

		[Test]
		public void Strings()
		{
			var layout = new LogMessageLayout();

			var message = "test";
			var loggingEvent = GetLogginEvent(message);

			var result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(message, result.Message);
		}

		[Test]
		public void CustomObjectAddedAsAdditionalProperties()
		{
			var layout = new LogMessageLayout();

			var message = new { Test = 1, Test2 = "YES!!!" };
			var loggingEvent = GetLogginEvent(message);

			var result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(result["Test"], message.Test.ToString());
			Assert.AreEqual(result["Test2"], message.Test2);
		}

		[Test]
		public void ThreadContextPropertiesAddedAsAdditionalProperties()
		{
			var layout = new LogMessageLayout();

			var message = "test";

			ThreadContext.Properties["TraceID"] = 1;

			var loggingEvent = GetLogginEvent(message);

			var result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(result.Message, message);
			Assert.AreEqual(result["TraceID"], "1");
		}

		[Test]
		public void BaseLogMessageIncluded()
		{
			var layout = new LogMessageLayout();
			var message = "test";
			var loggingEvent = GetLogginEvent(message);

			var result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(result.Message, message);
			Assert.AreEqual(result.Host, Environment.MachineName);
			Assert.AreEqual(result.SysLogLevel, (int)LocalSyslogAppender.SyslogSeverity.Debug);
			Assert.IsTrue(result.TimeStamp >= DateTime.Now.AddMinutes(-1));
		}

		[Test]
		public void CustomPropertyWithUnderscoreIsAddedCorrectly()
		{
			var layout = new LogMessageLayout();

			var message = "test";

			ThreadContext.Properties["TraceID"] = 1;

			var loggingEvent = GetLogginEvent(message);

			var result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(result.Message, message);
			Assert.AreEqual(result["TraceID"], "1");
		}

		[Test]
		public void CustomObjectWithMessageProperty()
		{
			var layout = new LogMessageLayout();
			var message = new { Message = "Success", Test = 1 };

			var loggingEvent = GetLogginEvent(message);
			var result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(result.Message, message.Message);
			Assert.AreEqual(result["Test"], message.Test.ToString());

			var message2 = new { FullMessage = "Success", Test = 1 };
			loggingEvent = GetLogginEvent(message2);
			result = GetMessage(layout, loggingEvent);


			Assert.AreEqual(result.Message, message2.FullMessage);
			Assert.AreEqual(result["Test"], message2.Test.ToString());

			var message3 = new { message = "Success", Test = 1 };
			loggingEvent = GetLogginEvent(message3);
			result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(result.Message, message3.message);
			Assert.AreEqual(result["Test"], message3.Test.ToString());

			var message4 = new { full_Message = "Success", Test = 1 };
			loggingEvent = GetLogginEvent(message4);
			result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(result.Message, message4.full_Message);
			Assert.AreEqual(result["Test"], message4.Test.ToString());
		}

		[Test]
		public void DictionaryMessage()
		{
			var layout = new LogMessageLayout();
			var message = new Dictionary<string, string> { { "Test", "1" }, { "Test2", "2" } };

			var loggingEvent = GetLogginEvent(message);

			var result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(result["Test"], message["Test"]);
			Assert.AreEqual(result["Test2"], message["Test2"]);
		}

		[Test]
		public void PatternConversionInAdditionalProperties()
		{
			var layout = new LogMessageLayout();
			layout.AdditionalFields = "Level:%level,AppDomain:%a,LoggerName:%c{1},ThreadName:%t";
			var message = new { Message = "Test" };

			var loggingEvent = GetLogginEvent(message);

			var result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(result["Level"], "DEBUG");
			Assert.IsTrue(!string.IsNullOrWhiteSpace(result["AppDomain"].ToString()));
			Assert.IsTrue(!string.IsNullOrWhiteSpace(result["ThreadName"].ToString()));
			Assert.AreEqual("Class", result["LoggerName"]);
		}

		[Test]
		public void PatternConversionLayoutSpecified()
		{
			var layout = new LogMessageLayout();
			layout.ConversionPattern = "[%level] - [%c{1}]";
			var message = new { Message = "Test" };

			var loggingEvent = GetLogginEvent(message);

			var result = GetMessage(layout, loggingEvent);

			Assert.AreEqual("[DEBUG] - [Class]", result["message"]);
		}
		
		[Test]
		public void ToStringOnObjectIfNoMessageIsProvided()
		{
			var layout = new LogMessageLayout();

			var message = new { Test = 1, Test2 = 2 };

			var loggingEvent = GetLogginEvent(message);

			var result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(result["Test"], message.Test.ToString());
			Assert.AreEqual(result["Test2"], message.Test2.ToString());
			Assert.AreEqual(result.Message, message.ToString());
		}

		[Test]
		public void IncludeLocationInformation()
		{
			var layout = new LogMessageLayout();
			layout.IncludeLocationInformation = true;

			var message = "test";
			var loggingEvent = GetLogginEvent(message);

			var result = GetMessage(layout, loggingEvent);

			Assert.AreEqual(message, result.Message);
			Assert.IsNotNullOrEmpty(result.Line);
			Assert.IsNotNullOrEmpty(result.File);
		}

		private LogMessage GetMessage(LogMessageLayout layout, LoggingEvent message)
		{
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb))
				layout.Format(sw, message);

			return JsonConvert.DeserializeObject<LogMessage>(sb.ToString());
		}

		private static LoggingEvent GetLogginEvent(object message)
		{
			
			return new LoggingEvent((Type)null, (ILoggerRepository)null, "Test.Logger.Class", Level.Debug, message, null);
		}
	}
}
