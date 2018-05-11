using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FMData.Xml
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// https://stackoverflow.com/a/4944547/86860
        /// </summary>
        public static T ToObject<T>(this IDictionary<string, object> source) where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType().GetTypeInfo();

            foreach (var item in source)
            {
                someObjectType
                    .DeclaredProperties
                    .FirstOrDefault(k => k.Name.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase))
                    ?.SetValue(someObject, item.Value, null);
            }

            return someObject;
        }

        /// <summary>
        /// https://stackoverflow.com/a/4944547/86860
        /// </summary>
        public static IDictionary<string, object> AsDictionary(this object source, bool includeNulls = true)
        {
            var props = source.GetType().GetTypeInfo().DeclaredProperties;

            if (!includeNulls)
            {
                props = props.Where(p => p.GetValue(source, null) != null);
            }

            return props.ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );
        }
    }
}