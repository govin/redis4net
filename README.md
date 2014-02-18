# redis4net  
redis4net is a log4net appender that formats logs and adds them to redis lists. This is useful when you have another system like logstash which indexes yours 
logs. The redis lists that you write to can feed into the logstash indexer.

## Installation

Working on a nuget package. Meantime, please check out the source code. 

## Configuration

redis4net gives you the ability to log messages to redis lists. The library usage is similar to and inspired by the [gelf4net library][1] library. However, 
this library steers clear of the GELF message format since redis does not impose any such message formats. The base log message format used by this library consists of just 5 fields
- type, message, sysloglevel, host and timestamp. Every other field can be supplied by the user and is customizable. 

**Note**
If you later pump the redis list to logstash, it is stronly recommended that you supply a name unique to your application for the "type" field. Otherwise, differences in 
data types of fields within the same "type", will cause logstash indexer to choke.

**Sample Configuration**

```  
<?xml version="1.0"?>
<configuration>
	<configSections>
		<section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler,Log4net"/>
	</configSections>

	<log4net>
		<root>
		  <level value="ALL"/>
		  <appender-ref ref="RedisAppender"/>
		</root>

		<appender name="RedisAppender" type="redis4net.Appender.RedisAppender, redis4net">
			<remoteAddress value="127.0.0.1" />
			<remotePort value="6379" />
			<listName value="logstash" />
			<layout type="redis4net.Layout.LogMessageLayout, redis4net">
				<param name="IncludeLocationInformation" value="true" />
				<param name="AdditionalFields" value="type:MyApp,Environment:local,Level:%level" />
			</layout>
		</appender>
	</log4net>
</configuration>
```  

## Additional Properties

There are several ways that additional properties can be added to a log.

**Configuration**  
Any static information can be set through configuration by adding a comma separated list of key:value pairs.  
You can also use conversion patterns like you would if you were using the [PatternLayout class][3]:

```  
<layout type="redis4net.Layout.LogMessageLayout, redis4net">
    <param name="AdditionalFields" value="type:MyAppType, AppName:MyAppName,AppInstance:1.0,Level:%level" />
</layout>
```  

This will add the following fields to your redis log:

```  
{
    ...
	"type":"MyAppType",
	"AppName":"MyAppName",
	"AppInstance": "1.0"
	"Level":"DEBUG",
	...
}
```  

**Custom Properties**  
Any properties you add to the `log4net.ThreadContext.Properties` object 
will automatically be added to the message as additional fields

```  
log4net.ThreadContext.Properties["TraceID"] = Guid.NewGuid();
```  

This will be added to the log:

```  
{
    ...
    "TraceID":"3449DDF8-C3B4-46DD-8B83-0BDF1ABC92E2",
    ...
}
```  

**Custom Objects**  
You can use custom objects to log additional fields to the output. Here is an example:

```  
_logger.Debug(new {
    Type = "Request",
    Method = request.Method,
    Url = request.Url
});

```  

This will add the following additional fields to the output:

```  
{
    ...
    "Type":"Request",
    "Method":"GET",
    "Url":"http://whatever.com/gelf",
    ...
}
``` 

You can also supply a dictionary directly and get the same output. 

```  
_logger.Debug(new Dictionary<string,string>{
    { "Type", "Request" },
    { "Method", request.Method },
    { "Url", request.Url.ToString() }
});

```  

## Formatting Messages  
If you are just logging a simple string then that message will show up in the `message` field.

```  
_logger.Debug("Hello World Redis Logs");

```  

Hello World Redis Logs, this will be your output:

If you want to format your message using a conversion pattern you can do so by specifying the `ConversionPattern` parameter.  
Again you can specify all the same parameters that you would if you were using the [PatternLayout][3].

```  
		  <layout type="redis4net.Layout.LogMessageLayout, redis4net">
			<param name="ConversionPattern" value="[%t] %c{1} - %m" />
		  </layout>  
```  


You can also specify the message when logging custom objects:

```  
_logger.Debug(new Dictionary<string,string>{
    { "Type", "Request" },
    { "Method", request.Method },
    { "Message", request.RawUrl }
});

``` 

If the custom object does not have a `Message` or 'FullMessage' or 'Full_Message' field than the message will be the 
output of the `ToString()` of that object.

## License
This project is licensed under the [Apache 2.0](2) license

[1]: https://github.com/jjchiw/gelf4net
[2]: http://www.apache.org/licenses/LICENSE-2.0.html
[3]: http://logging.apache.org/log4net/release/sdk/log4net.Layout.PatternLayout.html
