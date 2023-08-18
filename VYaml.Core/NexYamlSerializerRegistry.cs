using System;
using System.Collections.Generic;
using System.Text;
using VYaml.Serialization;

namespace VYaml
{
    public class NexYamlSerializerRegistry : IYamlFormatterResolver
    {
        public static NexYamlSerializerRegistry Default = new NexYamlSerializerRegistry();
        Dictionary<Type, IYamlFormatter> TypeFormatter = new Dictionary<Type, IYamlFormatter>();

        public IYamlFormatter<T>? GetFormatter<T>()
        {
            if(TypeFormatter.ContainsKey(typeof(T)))
            {
                return (IYamlFormatter<T>)TypeFormatter[typeof(T)];
            }
            return null;
        }
        public void RegisterFormatter<T>(IYamlFormatter<T> formatter)
        {
            Type keyType = typeof(T);
            if (!TypeFormatter.ContainsKey(keyType))
            {
                TypeFormatter[keyType] = formatter;
            }
            TypeFormatter[keyType] = formatter;
        }
    }
}
