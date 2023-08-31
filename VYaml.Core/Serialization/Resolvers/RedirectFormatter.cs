﻿using System;
using System.Reflection;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization.Resolvers
{
    
    internal class RedirectFormatter<T> : IYamlFormatter<T>,IYamlFormatterResolver
    {
        public T Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var type = typeof(T);
            parser.TryGetCurrentTag(out var tag);

            var alias = NexYamlSerializerRegistry.Default.GetAliasType(tag.Handle);

            var formatter = NexYamlSerializerRegistry.Default.GetFormatter(alias);
            MethodInfo method = formatter.GetType().GetMethod(nameof(Deserialize));
            return (T)method.Invoke(formatter, new object[] { parser,context });
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
                formatter = NexYamlSerializerRegistry.Default.FindAbstractTypeBased<T>(value.GetType());
            }
            else
            {
                formatter = NexYamlSerializerRegistry.Default.GetFormatter<T>();
            }
            MethodInfo method = formatter.GetType().GetMethod("Serialize");
            method.Invoke(formatter, new object[] { emitter, value, context });
        }
    }
}
