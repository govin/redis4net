using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace redis4net
{
    public static class Extensions
    {
        public static IDictionary ToDictionary(this object values)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (values != null)
            {
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(values))
                {
                    object obj = propertyDescriptor.GetValue(values);
                    dict.Add(propertyDescriptor.Name, obj);
                }
            }

            return dict;
        }

        public static double ToUnixTimestamp(this DateTime d)
        {
          var duration = d.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0);

          return duration.TotalSeconds;
        }

        public static DateTime FromUnixTimestamp(this double d)
        {

          var datetime = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(d*1000).ToLocalTime();

          return datetime;
        }
    }
}
