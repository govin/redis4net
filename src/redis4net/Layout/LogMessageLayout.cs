using log4net.Core;
using log4net.Layout;
using log4net.Util;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace redis4net.Layout
{
	using log4net.Appender;

	public class LogMessageLayout : LayoutSkeleton
	{
		private Dictionary<string, string> innerAdditionalFields;
		private string additionalFields;
		private readonly PatternLayout patternLayout;

		public LogMessageLayout()
		{
			IncludeLocationInformation = false;
			LogStackTraceFromMessage = true;
			IgnoresException = false;
			this.patternLayout = new PatternLayout();
		}

		/// <summary>
		/// The content type output by this layout.
		/// </summary>
		public override string ContentType
		{
			get
			{
				return "application/json";
			}
		}

		/// <summary>
		/// The additional fields configured in log4net config.
		/// </summary>
		public string AdditionalFields
		{
			get { return this.additionalFields; }
			set
			{
				this.additionalFields = value;

				if (this.additionalFields == null)
				{
					innerAdditionalFields.Clear();
				}
				else
				{
					innerAdditionalFields = new Dictionary<string, string>();
					innerAdditionalFields = this.additionalFields.Split(',')
							.ToDictionary(it => it.Split(':')[0], it => it.Split(':')[1]);
				}
			}
		}

		/// <summary>
		/// The conversion pattern to use for the message body
		/// </summary>
		public string ConversionPattern { get; set; }

		/// <summary>
		/// If should append the stack trace to the message if the message is an exception
		/// </summary>
		public bool LogStackTraceFromMessage { get; set; }

		/// <summary>
		/// Specifies wehter location inormation should be included in the message
		/// </summary>
		public bool IncludeLocationInformation { get; set; }

		public override void ActivateOptions()
		{
		}

		public override void Format(System.IO.TextWriter writer, LoggingEvent loggingEvent)
		{
			var logMessage = this.GetBaseLogMessage(loggingEvent);

			AddLoggingEventToMessage(loggingEvent, logMessage);

			AddAdditionalFields(loggingEvent, logMessage);

			writer.Write(JsonConvert.SerializeObject(logMessage, Formatting.Indented));
		}

		private void AddLoggingEventToMessage(LoggingEvent loggingEvent, LogMessage logMessage)
		{
			//If conversion pattern is specified then defer to PatternLayout for building the message body
			if (!string.IsNullOrWhiteSpace(ConversionPattern))
			{
				var message = GetValueFromPattern(loggingEvent, ConversionPattern);
				logMessage.Message = message;
			}
			else //Otherwise do our custom message builder stuff
			{
				var messageObject = loggingEvent.MessageObject;
				if (messageObject == null)
				{
					logMessage.Message = SystemInfo.NullText;
				}

				if (messageObject is string || messageObject is SystemStringFormat)
				{
					var fullMessage = messageObject.ToString();
					logMessage.Message = fullMessage;
				}
				else if (messageObject is IDictionary)
				{
					AddToMessage(logMessage, messageObject as IDictionary);
				}
				else
				{
					AddToMessage(logMessage, messageObject.ToDictionary());
				}

				logMessage.Message = logMessage.Message ?? messageObject.ToString();
			}

			if (LogStackTraceFromMessage && loggingEvent.ExceptionObject != null)
			{
				logMessage.Message = string.Format("{0} - {1}.", logMessage.Message, loggingEvent.GetExceptionString());
			}
		}

		private LogMessage GetBaseLogMessage(LoggingEvent loggingEvent)
		{
			var message = new LogMessage
			{
				Host = Environment.MachineName,
				SysLogLevel = GetSyslogSeverity(loggingEvent.Level),
				TimeStamp = loggingEvent.TimeStamp,
			};

			message.Add("LoggerName", loggingEvent.LoggerName);

			if (this.IncludeLocationInformation)
			{
				message.File = loggingEvent.LocationInformation.FileName;
				message.Line = loggingEvent.LocationInformation.LineNumber;
			}

			return message;
		}

		private void AddToMessage(LogMessage logMessage, IDictionary messageObject)
		{
			foreach (DictionaryEntry entry in messageObject)
			{
				var key = entry.Key.ToString();
				var value = (entry.Value ?? string.Empty).ToString();
				if (MessageKeyValues.Contains(key, StringComparer.OrdinalIgnoreCase))
				{
					logMessage.Message = value;
				}
				else
				{
					logMessage[entry.Key.ToString()] = value;
				}

			}
		}

		private void AddAdditionalFields(LoggingEvent loggingEvent, LogMessage message)
		{
			var moreFields = (innerAdditionalFields == null)
								? new Dictionary<string, string>()
								: new Dictionary<string, string>(innerAdditionalFields);

			foreach (DictionaryEntry item in loggingEvent.GetProperties())
			{
				var key = item.Key as string;
				if (key != null && !key.StartsWith("log4net:") /*exclude log4net built-in properties */ )
				{
					var val = item.Value == null ? null : item.Value.ToString();
					moreFields.Add(key, val);
				}
			}

			foreach (var kvp in moreFields)
			{
				//If the value starts with a '%' then defer to the pattern layout
				var value = kvp.Value.StartsWith("%") ? GetValueFromPattern(loggingEvent, kvp.Value) : kvp.Value;
				message[kvp.Key] = value;
			}
		}

		private string GetValueFromPattern(LoggingEvent loggingEvent, string pattern)
		{
			//Reset the pattern layout
			this.patternLayout.ConversionPattern = pattern;
			this.patternLayout.ActivateOptions();

			//Write the results
			var sb = new StringBuilder();
			using (var writer = new System.IO.StringWriter(sb))
			{
				this.patternLayout.Format(writer, loggingEvent);
				writer.Flush();
				return sb.ToString();
			}
		}

		private static long GetSyslogSeverity(Level level)
		{
			if (level == log4net.Core.Level.Alert)
				return (long)LocalSyslogAppender.SyslogSeverity.Alert;

			if (level == log4net.Core.Level.Critical || level == log4net.Core.Level.Fatal)
				return (long)LocalSyslogAppender.SyslogSeverity.Critical;

			if (level == log4net.Core.Level.Debug)
				return (long)LocalSyslogAppender.SyslogSeverity.Debug;

			if (level == log4net.Core.Level.Emergency)
				return (long)LocalSyslogAppender.SyslogSeverity.Emergency;

			if (level == log4net.Core.Level.Error)
				return (long)LocalSyslogAppender.SyslogSeverity.Error;

			if (level == log4net.Core.Level.Fine
				|| level == log4net.Core.Level.Finer
				|| level == log4net.Core.Level.Finest
				|| level == log4net.Core.Level.Info
				|| level == log4net.Core.Level.Off)
				return (long)LocalSyslogAppender.SyslogSeverity.Informational;

			if (level == log4net.Core.Level.Notice
				|| level == log4net.Core.Level.Verbose
				|| level == log4net.Core.Level.Trace)
				return (long)LocalSyslogAppender.SyslogSeverity.Notice;

			if (level == log4net.Core.Level.Severe)
				return (long)LocalSyslogAppender.SyslogSeverity.Emergency;

			if (level == log4net.Core.Level.Warn)
				return (long)LocalSyslogAppender.SyslogSeverity.Warning;

			return (long)LocalSyslogAppender.SyslogSeverity.Debug;
		}

		private static readonly IEnumerable<string> MessageKeyValues = new[] { "FULLMESSAGE", "FULL_MESSAGE", "MESSAGE" };
	}
}
