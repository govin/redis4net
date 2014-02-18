using System;
using System.Collections.Generic;
namespace redis4net
{
	using System.Globalization;

	public class LogMessage : Dictionary<string, object>
	{
		public string Type
		{
			get
			{
				if (!this.ContainsKey("type"))
					return null;

				return (string)this["type"];
			}
			set
			{
				if (!this.ContainsKey("type"))
					this.Add("type", value);
				else
					this["type"] = value;
			}
		}

		public string Message
		{
			get
			{
				if (!this.ContainsKey("message"))
					return null;

				return (string)this["message"];
			}
			set
			{
				if (!this.ContainsKey("message"))
					this.Add("message", value);
				else
					this["message"] = value;
			}
		}

		public long SysLogLevel
		{
			get
			{
				if (!this.ContainsKey("sysloglevel"))
					return int.MinValue;

				return (long)this["sysloglevel"];
			}
			set
			{
				if (!this.ContainsKey("sysloglevel"))
					this.Add("sysloglevel", value);
				else
					this["sysloglevel"] = value;
			}
		}

		public string Host
		{
			get
			{
				if (!this.ContainsKey("host"))
					return null;

				return (string)this["host"];
			}
			set
			{
				if (!this.ContainsKey("host"))
					this.Add("host", value);
				else
					this["host"] = value;
			}
		}

		public string File
		{
			get
			{
				if (!this.ContainsKey("file"))
					return null;

				return (string)this["file"];
			}
			set
			{
				if (!this.ContainsKey("file"))
					this.Add("file", value);
				else
					this["file"] = value;
			}
		}

		public string Line
		{
			get
			{
				if (!this.ContainsKey("line"))
					return null;

				return (string)this["line"];
			}
			set
			{
				if (!this.ContainsKey("line"))
					this.Add("line", value);
				else
					this["line"] = value;
			}
		}

		public DateTime TimeStamp
		{
			get
			{
				if (!this.ContainsKey("timestamp"))
					return DateTime.MinValue;

				var val = this["timestamp"];
				double value;
				var parsed = double.TryParse(val as string, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
				return parsed ? value.FromUnixTimestamp() : DateTime.MinValue;
			}
			set
			{
				if (!this.ContainsKey("timestamp"))
					this.Add("timestamp", value.ToUnixTimestamp().ToString(CultureInfo.InvariantCulture));
				else
					this["timestamp"] = value.ToUnixTimestamp().ToString(CultureInfo.InvariantCulture);
			}
		}
	}
}
