using log4net.Util;
using Newtonsoft.Json;
using System.Collections;
namespace redis4net.ObjectRenderer
{
	using log4net.ObjectRenderer;

	using Newtonsoft.Json.Serialization;

	public class RedisMessageRenderer : IObjectRenderer
	{
		public void RenderObject(RendererMap rendererMap, object obj, System.IO.TextWriter writer)
		{
			var dictionary = obj as IDictionary;
			var jsonSerializerSettings = new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};
			var serializedLog = JsonConvert.SerializeObject(dictionary, Formatting.None, jsonSerializerSettings);
			if (dictionary == null)
			{
				writer.Write(SystemInfo.NullText);
			}
			else
			{
				writer.Write(serializedLog);
			}
		}
	}
}
