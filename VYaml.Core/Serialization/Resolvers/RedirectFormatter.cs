using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization.Resolvers
{
    internal class RedirectFormatter<T> : IYamlFormatter<T>,IYamlFormatterResolver
    {
        public T Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var type = typeof(T);
            IYamlFormatter formatter;
            if (type.IsAbstract)
            {
                parser.TryGetCurrentTag(out var tag);
                formatter = NexYamlSerializerRegistry.Default.FindAbstractFormatter<T>(tag);
            }
            else if(type.IsInterface)
            {
                parser.TryGetCurrentTag(out var tag);
                formatter = NexYamlSerializerRegistry.Default.FindInterfaceFormatter<T>(tag);
            }
            else
            {
                formatter = NexYamlSerializerRegistry.Default.GetFormatter<T>();
            }
            
            return ((IYamlFormatter<T>)formatter).Deserialize(ref parser,context);
        }

        public IYamlFormatter<T1>? GetFormatter<T1>()
        {
            return NexYamlSerializerRegistry.Default.GetFormatter<T1>();
        }
        
        public void Serialize(ref Utf8YamlEmitter emitter, T value, YamlSerializationContext context)
        {
            Type type = typeof(T);
            IYamlFormatter formatter;
            if (type.IsInterface)
            {
                formatter = NexYamlSerializerRegistry.Default.FindInterfaceTypeBased<T>(value.GetType());
            }
            else if (type.IsAbstract)
            {
                formatter = NexYamlSerializerRegistry.Default.FindInterfaceTypeBased<T>(value.GetType());
            }
            else
            {
                formatter = NexYamlSerializerRegistry.Default.GetFormatter<T>();
            }
            
            ???formatter???.Serialize(ref emitter, value, context);
        }
    }
}
